using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.IO;

public class StartupInvestCard : MonoBehaviour
{
    public TMPro.TextMeshProUGUI startupName;
    public Image founderImage;
    public TMPro.TextMeshProUGUI startupPitch;
    public TMPro.TextMeshProUGUI founderName;
    public Button nextButton;

    private string startupsFilePath;
    public List<string> startupNames;

    void Start()
    {
        // Initialisation du chemin du fichier startups.json
        startupsFilePath = Path.Combine(Application.streamingAssetsPath, "startups.json");

        // Charger la liste des startups
        StartCoroutine(LoadStartupsList());

        // Ajouter l'action au bouton "Next"
        nextButton.onClick.AddListener(() => StartCoroutine(LoadRandomStartup()));
    }

    private IEnumerator LoadStartupsList()
    {
        // Charger le fichier startups.json
        UnityWebRequest request = UnityWebRequest.Get(startupsFilePath);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Erreur lors du chargement de startups.json : {request.error}");
            yield break;
        }

        // Décoder les données JSON
        StartupList startupList = JsonUtility.FromJson<StartupList>(request.downloadHandler.text);
        startupNames = startupList.startups;
        Debug.Log($"Liste des startups chargée : {startupNames.Count} startups");
        if (startupNames == null || startupNames.Count == 0)
        {
            Debug.LogError("La liste des startups est vide ou invalide.");
            yield break;
        }

        // Charger une startup aléatoire au démarrage
        StartCoroutine(LoadRandomStartup());
    }

    private IEnumerator LoadRandomStartup()
    {
        if (startupNames == null || startupNames.Count == 0)
        {
            Debug.LogError("La liste des startups n'est pas initialisée.");
            yield break;
        }

        // Choisir une startup aléatoire
        string randomStartupName = startupNames[Random.Range(0, startupNames.Count)];

        // Charger les données JSON de la startup
        string jsonFilePath = Path.Combine(Application.streamingAssetsPath, "GeneratedStartups", randomStartupName + ".json");
        UnityWebRequest jsonRequest = UnityWebRequest.Get(jsonFilePath);
        yield return jsonRequest.SendWebRequest();

        if (jsonRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Erreur lors du chargement du fichier JSON path={jsonFilePath} error:{jsonRequest.error}");
            yield break;
        }

        // Convertir les données JSON
        StartupData startup = JsonUtility.FromJson<StartupData>(jsonRequest.downloadHandler.text);

        // Mettre à jour les champs de l'UI
        startupName.text = startup.StartupName;
        founderName.text = startup.FounderName;
        startupPitch.text = startup.Pitch;

        // Charger l'image correspondante
        string imageFilePath = Path.Combine(Application.streamingAssetsPath, "GeneratedStartups", randomStartupName + "_founder.png");
        Debug.Log($"Chargement de l'image : {imageFilePath}");
        StartCoroutine(LoadImage(imageFilePath));
    }

    private IEnumerator LoadImage(string filePath)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(filePath))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                founderImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            }
            else
            {
                Debug.LogWarning($"Erreur lors du chargement de l'image : {request.error}");
                founderImage.sprite=null;
            }
        }
    }

    [System.Serializable]
    private class StartupList
    {
        public List<string> startups;
    }

    [System.Serializable]
    private class StartupData
    {
        public string StartupName;
        public string FounderName;
        public string foundertrait;
        public string Pitch;
        public int SuccessRate;
    }
}
