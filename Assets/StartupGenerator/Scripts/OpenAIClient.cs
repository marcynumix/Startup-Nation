using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Unity.EditorCoroutines.Editor; // Nécessaire pour les coroutines dans l'éditeur

public static class OpenAIClient
{
    private static readonly string apiUrl = "https://api.openai.com/v1/chat/completions";
    private static string apiKey;

    static OpenAIClient()
    {
        // Charger la clé API depuis .env lors de la première utilisation
        EnvLoader.LoadEnv();
        apiKey = EnvLoader.GetEnv("OPENAI_API_KEY");

        if (string.IsNullOrEmpty(apiKey))
        {
            Debug.LogError("API Key not found! Please check your .env file.");
        }
        else
        {
            Debug.Log("API Key loaded successfully.");
        }
    }

    /// <summary>
    /// Envoie un prompt à OpenAI et retourne la réponse.
    /// </summary>
    /// <param name="prompt">Le prompt à envoyer.</param>
    /// <returns>La réponse de l'API sous forme de texte.</returns>
    public static async Task<string> SendPromptAsync(string prompt)
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            Debug.LogError("API Key is missing. Please check your .env file.");
            return "Error: API Key is missing.";
        }

        if (string.IsNullOrEmpty(prompt))
        {
            Debug.LogWarning("Prompt is empty. Please provide a valid prompt.");
            return "Error: Prompt cannot be empty.";
        }

        // Envoyer la requête et attendre la réponse
        return await Task.Run(() =>
        {
            var tcs = new TaskCompletionSource<string>();
            EditorCoroutineUtility.StartCoroutineOwnerless(SendRequest(prompt, tcs));
            return tcs.Task;
        });
    }

    private static IEnumerator SendRequest(string prompt, TaskCompletionSource<string> tcs)
    {
        // Préparer les données de la requête
        ChatRequest requestData = new ChatRequest
        {
            model = "gpt-3.5-turbo",
            messages = new List<Message>
            {
                new Message { role = "system", content = "You are a helpful assistant." },
                new Message { role = "user", content = prompt }
            },
            max_tokens = 100,
            temperature = 0.7f
        };

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
                string response = request.downloadHandler.text;
                Debug.Log("Response: " + response);
                tcs.SetResult(response); // Renvoie la réponse au Task
            }
            else
            {
                string error = $"Error: {request.error}\nResponse Code: {request.responseCode}\nResponse Body: {request.downloadHandler.text}";
                Debug.LogError(error);
                tcs.SetResult(error); // Renvoie l'erreur au Task
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
