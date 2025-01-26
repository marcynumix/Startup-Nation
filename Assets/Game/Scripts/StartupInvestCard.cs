using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.IO;
using System.Runtime.CompilerServices;

public class StartupInvestCard : MonoBehaviour
{
    public static StartupInvestCard Instance { get; private set; }
    public TMPro.TextMeshProUGUI startupName;
    public Image startupImage;
    public Image founderPortraitImage;
    public Sprite[] founderPortraits;
    public TMPro.TextMeshProUGUI startupPitch;
    public TMPro.TextMeshProUGUI founderName;
    public TMPro.TextMeshProUGUI founderNameShadow;

    public AudioSource audioSourcePitch;

    private string startupsFilePath;
    public List<string> startupNames;
    public List<string> availableStartups = new List<string>();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        
        // Initialisation du chemin du fichier startups.json
        startupsFilePath = Path.Combine(Application.streamingAssetsPath, "startups.json");

        // Charger la liste des startups
        StartCoroutine(LoadStartupsList());
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
        availableStartups = new List<string>(startupNames);
        
        
        Debug.Log($"Liste des startups chargée : {startupNames.Count} startups");
        if (startupNames == null || startupNames.Count == 0)
        {
            Debug.LogError("La liste des startups est vide ou invalide.");
            yield break;
        }

        // Charger une startup aléatoire au démarrage
        StartCoroutine(LoadRandomStartupCoroutine());
    }

    public void LoadRandomStartup(){
        StartCoroutine(LoadRandomStartupCoroutine());
    }

    private string GetAvailableRandomStartupName()
    {
        if (availableStartups.Count == 0)
        {
            availableStartups = new List<string>(startupNames);
        }

        int randomIndex = Random.Range(0, availableStartups.Count);
        string randomStartupName = availableStartups[randomIndex];
        availableStartups.RemoveAt(randomIndex);

        return randomStartupName;
    }

    private IEnumerator LoadRandomStartupCoroutine()
    {
        if (startupNames == null || startupNames.Count == 0)
        {
            Debug.LogError("La liste des startups n'est pas initialisée.");
            yield break;
        }

        // Choisir une startup aléatoire
        string randomStartupName = GetAvailableRandomStartupName();

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

        // MAJ Metrics UI
        MetricsUI.instance.SetMetrics(startup.SuccessRate);

        // Mettre à jour les champs de l'UI
        startupName.text = startup.StartupName;
        founderName.text = founderNameShadow.text = startup.FounderName;
        startupPitch.text = startup.Pitch;

        // Charger l'image correspondante
        string imageFilePath = Path.Combine(Application.streamingAssetsPath, "GeneratedStartups", randomStartupName + "_founder.png");
        // Debug.Log($"Chargement de l'image : {imageFilePath}");
        StartCoroutine(LoadStartupImage(imageFilePath));
        LoadFounderPortraitImage();
        string soundFileName = randomStartupName + "_founder.mp3";
        string soundFilePath = Path.Combine(Application.streamingAssetsPath, "GeneratedStartups", soundFileName);    
        StartCoroutine(LoadStartupPitchSound(soundFilePath));
    }

    public void MutePitch()
    {
        StopCoroutine(PlayPitchDelayed(0));
        audioSourcePitch.Stop();
    }
    public void PlayPitch(float delay=0){
        // play the audio clip
        StopCoroutine(PlayPitchDelayed(delay));
        StartCoroutine(PlayPitchDelayed(delay));
    }
    private IEnumerator PlayPitchDelayed(float delay){
        yield return new WaitForSeconds(delay);
        audioSourcePitch.Play();
    }

    private void LoadFounderPortraitImage(){
        // Randomize founder portrait image
        int id = Random.Range(0, founderPortraits.Length);
        founderPortraitImage.sprite = founderPortraits[id];
    }

    private IEnumerator LoadStartupImage(string filePath)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(filePath))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                startupImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            }
            else
            {
                Debug.LogWarning($"Erreur lors du chargement de l'image : {request.error}");
                startupImage.sprite=null;
            }
        }
    }

    private IEnumerator LoadStartupPitchSound(string filePath){
        Debug.Log($"Chargement du son : {filePath}");
        using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(filePath, AudioType.MPEG))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
                audioSourcePitch.clip = clip;
            }
            else
            {
                audioSourcePitch.clip =null;
                Debug.LogWarning($"Erreur lors du chargement du son : {request.error}");
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
        public float SuccessRate;
        public string FounderSex;
    }
}
