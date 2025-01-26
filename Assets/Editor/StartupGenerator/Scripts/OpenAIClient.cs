using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Unity.EditorCoroutines.Editor;
using System.Drawing; // Nécessaire pour les coroutines dans l'éditeur

public class OpenAIClient : MonoBehaviour
{
    private static readonly string apiUrl = "https://api.openai.com/v1/chat/completions";
    private static readonly string apiImageUrl = "https://api.openai.com/v1/images/generations"; // URL pour les images
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
    /// Envoie un prompt à OpenAI avec les paramètres personnalisés et retourne la réponse.
    /// </summary>
    /// <param name="prompt">Le prompt à envoyer.</param>
    /// <param name="model">Le modèle OpenAI à utiliser.</param>
    /// <param name="maxTokens">Le nombre maximum de tokens pour la réponse.</param>
    /// <param name="temperature">La température pour la génération de texte.</param>
    /// <returns>La réponse de l'API sous forme de texte.</returns>
    public static async Task<string> SendPromptAsync(string systemPrompt, string prompt, string model, int maxTokens, float temperature)
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
            EditorCoroutineUtility.StartCoroutineOwnerless(SendRequest(systemPrompt, prompt, model, maxTokens, temperature, tcs));
            return tcs.Task;
        });
    }

    /// <summary>
    /// Envoie un prompt à OpenAI pour générer une image et retourne la texture correspondante.
    /// </summary>
    /// <param name="prompt">Le prompt pour générer l'image.</param>
    /// <returns>Une Texture2D représentant l'image générée.</returns>
    public static async Task<Texture2D> SendPromptImageAsync(string prompt, string size, string model)
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            Debug.LogError("API Key is missing. Please check your .env file.");
            return null;
        }

        if (string.IsNullOrEmpty(prompt))
        {
            Debug.LogWarning("Prompt is empty. Please provide a valid prompt.");
            return null;
        }

        return await Task.Run(() =>
        {
            var tcs = new TaskCompletionSource<Texture2D>();
            EditorCoroutineUtility.StartCoroutineOwnerless(SendImageRequest(prompt, size, model, tcs));
            return tcs.Task;
        });
    }

    private static IEnumerator SendImageRequest(string prompt, string _size, string _model, TaskCompletionSource<Texture2D> tcs)
    {
        // Préparer les données de la requête
        ImageRequest requestData = new ImageRequest
        {
            prompt = prompt,
            n = 1,
            size = _size,
            model = _model // Ajouter le modèle explicitement
        };

        string json = JsonUtility.ToJson(requestData);

        using (UnityWebRequest request = new UnityWebRequest(apiImageUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string response = request.downloadHandler.text;
                ImageResponse imageResponse = JsonUtility.FromJson<ImageResponse>(response);

                if (imageResponse != null && imageResponse.data != null && imageResponse.data.Length > 0)
                {
                    string imageUrl = imageResponse.data[0].url;
                    yield return DownloadImage(imageUrl, tcs);
                }
                else
                {
                    Debug.LogError("Image generation failed: No data returned.");
                    tcs.SetResult(null);
                }
            }
            else
            {
                string error = $"Error: {request.error}\nResponse Code: {request.responseCode}\nResponse Body: {request.downloadHandler.text}";
                Debug.LogError(error);
                tcs.SetResult(null);
            }
        }
    }

    private static IEnumerator DownloadImage(string imageUrl, TaskCompletionSource<Texture2D> tcs)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
                tcs.SetResult(texture);
            }
            else
            {
                Debug.LogError($"Failed to download image: {request.error}");
                tcs.SetResult(null);
            }
        }
    }

    private static IEnumerator SendRequest(string systemPrompt, string prompt, string model, int maxTokens, float temperature, TaskCompletionSource<string> tcs)
    {
        // Préparer les données de la requête
        ChatRequest requestData = new ChatRequest
        {
            model = model,
            messages = new List<Message>
            {
                new Message { role = "system", content = systemPrompt },
                new Message { role = "user", content = prompt }
            },
            max_tokens = maxTokens,
            temperature = temperature
        };

        string json = JsonUtility.ToJson(requestData);

        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string response = request.downloadHandler.text;
                tcs.SetResult(response); // Renvoie la réponse au Task
            }
            else
            {
                string error = $"Error: {request.error}\nResponse Code: {request.responseCode}\nResponse Body: {request.downloadHandler.text}";
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

[System.Serializable]
public class ImageRequest
{
    public string prompt;
    public int n;
    public string size;
    public string model;
}

[System.Serializable]
public class ImageResponse
{
    public ImageData[] data;
}

[System.Serializable]
public class ImageData
{
    public string url;
}
