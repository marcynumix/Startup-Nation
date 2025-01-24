using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class OpenAIClient : MonoBehaviour
{
    // URL de l'API OpenAI
    private string apiUrl = "https://api.openai.com/v1/chat/completions";

    // Clé API (chargée dynamiquement)
    private string apiKey;

    private void Start()
    {
        // Charger la clé API depuis .env
        EnvLoader.LoadEnv();
        apiKey = EnvLoader.GetEnv("OPENAI_API_KEY");
        Debug.Log("API Key: " + apiKey);

        if (string.IsNullOrEmpty(apiKey))
        {
            Debug.LogError("API Key not found! Please check your .env file.");
        }

        SendPrompt("Hello, how are you?");
    }

    // Méthode publique pour envoyer un message (appelable dans l'inspecteur)
    public void SendPrompt(string prompt)
    {
        if (!string.IsNullOrEmpty(prompt))
        {
            Debug.Log("Sending prompt: " + prompt);
            SendMessageToGPT(prompt);
        }
        else
        {
            Debug.LogWarning("Prompt is empty. Please enter a prompt.");
        }
    }

    private void SendMessageToGPT(string userPrompt)
    {
        if (!string.IsNullOrEmpty(apiKey))
        {
            StartCoroutine(SendRequest(userPrompt));
        }
        else
        {
            Debug.LogError("Cannot send request: API Key is missing.");
        }
    }

    private IEnumerator SendRequest(string prompt)
    {
        // Création d'un objet requestData valide
        ChatRequest requestData = new ChatRequest
        {
            model = "gpt-3.5-turbo", // Assurez-vous que votre compte supporte ce modèle
            messages = new List<Message>
            {
                new Message { role = "system", content = "You are a helpful assistant." },
                new Message { role = "user", content = prompt }
            },
            max_tokens = 100,
            temperature = 0.7f
        };

        // Conversion de l'objet en JSON
        string json = JsonUtility.ToJson(requestData);

        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);

            Debug.Log($"Request Details:\nURL: {apiUrl}\nPayload: {json}\nAuthorization: Bearer {apiKey.Substring(0, 4)}****");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Response: " + request.downloadHandler.text);
            }
            else
            {
                Debug.LogError($"Error: {request.error}");
                Debug.LogError($"Response Code: {request.responseCode}");
                Debug.LogError($"Response Body: {request.downloadHandler.text}");
            }
        }
    }
}

// Classes de requête et réponse
[System.Serializable]
public class ChatRequest
{
    public string model;
    public List<Message> messages;
    public int max_tokens;
    public float temperature;
}

[System.Serializable]
public class Message
{
    public string role;
    public string content;
}

[System.Serializable]
public class ChatResponse
{
    public Choice[] choices;
}

[System.Serializable]
public class Choice
{
    public Message message;
}
