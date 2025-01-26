using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public class StartupGeneratorWindow : EditorWindow
{
    private Dictionary<string, List<string>> keywordLists = new Dictionary<string, List<string>>();
    private Vector2 scrollPosition; // Pour le défilement horizontal des mots-clés
    private string systemPrompt = ""; // Contenu du fichier de system prompt
    private string startupPrompt = ""; // Contenu du fichier de prompt
    private bool showConfiguration = true; // État du groupe "Configuration"
    private bool showConfigurationData = true; // État du groupe "Configuration Data"
    private bool showGenerateStartups = true; // État du groupe "Generate Startups"

    private int numberOfStartups = 1; // Nombre de startups à générer
    private bool deleteOldData = false; // Supprimer les anciennes données (checkbox)
    private bool generateImage = true; // Générer une image (checkbox)

    // Nouveaux paramètres OpenAI
    private string selectedModel = "gpt-4o"; // Modèle OpenAI sélectionné
    private readonly string[] modelOptions = { "gpt-4o", "gpt-4o-mini", "gpt-3.5-turbo", "gpt-4", "gpt-4-turbo" }; // Modèles disponibles

    private string selectedImageModel = "stable-diffusion-3.5";
    private readonly string[] imageModelOptions = { "dall-e-3", "stable-diffusion-3.5" };

    private string selectedImageSize = "512x512";
    private readonly string[] imageSizeOptions = { "512x512" };

    private int maxTokens = 400; // Nombre maximum de tokens
    private float temperature = 0.7f; // Température

    [MenuItem("Tools/Startup Generator")]
    public static void ShowWindow()
    {
        GetWindow<StartupGeneratorWindow>("Startup Generator");
    }

    private void OnEnable()
    {
        LoadKeywordFiles();
        LoadStartupPrompt();
        LoadSystemPrompt();
    }

    private void LoadKeywordFiles()
    {
        keywordLists.Clear();

        // Chemin vers le dossier Data
        string dataFolderPath = Path.Combine(Application.dataPath, "Editor/StartupGenerator/Data");

        if (Directory.Exists(dataFolderPath))
        {
            string[] files = Directory.GetFiles(dataFolderPath, "*.txt");
            foreach (string file in files)
            {
                string fileName = Path.GetFileNameWithoutExtension(file); // Nom du fichier sans extension
                string[] keywords = File.ReadAllLines(file); // Lire toutes les lignes du fichier
                keywordLists[fileName] = new List<string>(keywords);
            }
        }
        else
        {
            Debug.LogError($"Data folder not found at: {dataFolderPath}");
        }
    }

    private void LoadSystemPrompt()
    {
        // Chemin vers le fichier de system prompt
        string systemPromptFilePath = Path.Combine(Application.dataPath, "Editor/StartupGenerator/Prompts/startup-generation-systemprompt.txt");

        if (File.Exists(systemPromptFilePath))
        {
            systemPrompt = File.ReadAllText(systemPromptFilePath); // Lire tout le fichier
        }
        else
        {
            Debug.LogError($"System prompt file not found at: {systemPromptFilePath}");
            systemPrompt = "System prompt file not found!";
        }
    }

    private void SaveSystemPrompt()
    {
        // Chemin vers le fichier de system prompt
        string systemPromptFilePath = Path.Combine(Application.dataPath, "Editor/StartupGenerator/Prompts/startup-generation-systemprompt.txt");

        try
        {
            File.WriteAllText(systemPromptFilePath, systemPrompt); // Sauvegarder le contenu dans le fichie
            Debug.Log("System prompt saved successfully.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to save system prompt: {ex.Message}");
        }
    }

    private void LoadStartupPrompt()
    {
        // Chemin vers le fichier de prompt
        string promptFilePath = Path.Combine(Application.dataPath, "Editor/StartupGenerator/Prompts/startup-generation-prompt.txt");

        if (File.Exists(promptFilePath))
        {
            startupPrompt = File.ReadAllText(promptFilePath); // Lire tout le fichier
        }
        else
        {
            Debug.LogError($"Prompt file not found at: {promptFilePath}");
            startupPrompt = "Prompt file not found!";
        }
    }

    private void SaveStartupPrompt()
    {
        // Chemin vers le fichier de prompt
        string promptFilePath = Path.Combine(Application.dataPath, "Editor/StartupGenerator/Prompts/startup-generation-prompt.txt");

        try
        {
            File.WriteAllText(promptFilePath, startupPrompt); // Sauvegarder le contenu dans le fichier
            Debug.Log("Prompt saved successfully.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to save prompt: {ex.Message}");
        }
    }    

    private void OnGUI()
    {
        GUILayout.Label("Startup Generator", EditorStyles.boldLabel);

        // Section "Configuration"
        showConfiguration = EditorGUILayout.Foldout(showConfiguration, "Configuration");
        if (showConfiguration)
        {
            GUILayout.Label("OpenAI text Parameters:", EditorStyles.boldLabel);

            // Modèle OpenAI
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("OpenAI Text Model:", GUILayout.Width(150));
            selectedModel = modelOptions[EditorGUILayout.Popup(System.Array.IndexOf(modelOptions, selectedModel), modelOptions)];
            EditorGUILayout.EndHorizontal();

            // Max Tokens
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Max Tokens:", GUILayout.Width(150));
            maxTokens = EditorGUILayout.IntField(maxTokens, GUILayout.Width(50));
            EditorGUILayout.EndHorizontal();

            // Température
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Temperature:", GUILayout.Width(150));
            temperature = EditorGUILayout.FloatField(temperature, GUILayout.Width(50));
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);
            GUILayout.Label("OpenAI image Parameters:", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("OpenAI Image Size:", GUILayout.Width(150));
            selectedImageSize = imageSizeOptions[EditorGUILayout.Popup(System.Array.IndexOf(imageSizeOptions, selectedImageSize), imageSizeOptions)];
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("OpenAI Image Model:", GUILayout.Width(150));
            selectedImageModel = imageModelOptions[EditorGUILayout.Popup(System.Array.IndexOf(imageModelOptions, selectedImageModel), imageModelOptions)];
            EditorGUILayout.EndHorizontal();




            GUILayout.Label("System Prompt:", EditorStyles.label);

            var textAreaStyle = new GUIStyle(EditorStyles.textArea)
            {
                wordWrap = true // Active le retour à la ligne automatique
            };
            systemPrompt = EditorGUILayout.TextArea(systemPrompt, textAreaStyle, GUILayout.Height(30));

            if (GUILayout.Button("Save System Prompt"))
            {
                SaveSystemPrompt(); // Sauvegarde du system prompt
            }

            GUILayout.Space(10);
            GUILayout.Label("Startup Generation Prompt:", EditorStyles.label);

            startupPrompt = EditorGUILayout.TextArea(startupPrompt, textAreaStyle, GUILayout.Height(100));

            if (GUILayout.Button("Save Prompt"))
            {
                SaveStartupPrompt(); // Sauvegarde du prompt
            }

            GUILayout.Space(10);    

            showConfigurationData = EditorGUILayout.Foldout(showConfigurationData, "Data");
            if (showConfigurationData)
            {
                // Conteneur avec une hauteur fixe de 500 pixels
                EditorGUILayout.BeginVertical(GUILayout.Height(200));

                // Barre de défilement à l'intérieur du conteneur
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

                EditorGUILayout.BeginHorizontal();

                foreach (var entry in keywordLists)
                {
                    DrawKeywordList(entry.Key, entry.Value); // Afficher chaque liste
                }

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();
            }
            if (GUILayout.Button("Reload Data"))
            {
                LoadKeywordFiles(); // Recharger les fichiers de données
                LoadSystemPrompt(); // Recharger le fichier de system prompt
                LoadStartupPrompt(); // Recharger le fichier de prompt
            }
        }

        GUILayout.Space(10);

        // Section "Generate Startups"
        showGenerateStartups = EditorGUILayout.Foldout(showGenerateStartups, "Generate Startups");
        if (showGenerateStartups)
        {
            // Input field pour le nombre de startups
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Number of Startups:", GUILayout.Width(150));
            numberOfStartups = EditorGUILayout.IntField(numberOfStartups, GUILayout.Width(50));
            EditorGUILayout.EndHorizontal();

            // Checkbox pour générer des images
            generateImage = EditorGUILayout.Toggle("Generate Image", generateImage);

            // Checkbox pour supprimer les anciennes données
            deleteOldData = EditorGUILayout.Toggle("Delete Old Data", deleteOldData);


            // Bouton pour générer les startups
            if (GUILayout.Button("Generate Startups"))
            {
                GenerateStartups();
            }

            GUILayout.Space(10);

            // Bouton pour reconstruire startups.json
            if (GUILayout.Button("Rebuild Startup list file (startups.json)"))
            {
                UpdateStartupsJson();
                Debug.Log("startups.json has been rebuilt.");
            }

        }
    }

    private void DrawKeywordList(string title, List<string> keywords)
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(200)); // Liste verticale avec largeur fixe
        GUILayout.Label(title, EditorStyles.boldLabel); // Titre de la liste

        foreach (string keyword in keywords)
        {
            GUILayout.Label(keyword, EditorStyles.label); // Afficher chaque mot-clé
        }

        EditorGUILayout.EndVertical();
    }

    private async void GenerateStartups()
    {
        // Supprimer les anciennes données si nécessaire
        if (deleteOldData)
        {
            string generatedFolderPath = Path.Combine(Application.streamingAssetsPath, "GeneratedStartups");
            if (Directory.Exists(generatedFolderPath))
            {
                Directory.Delete(generatedFolderPath, true); // Supprime tout le dossier
                Debug.Log("Old generated startups deleted.");
            }
        }

        // Générer les startups
        for (int i = 0; i < numberOfStartups; i++)
        {
            Debug.Log($"Generating startup #{i + 1}...");

            // Générer le prompt final avec des mots-clés
            string finalPrompt = GeneratePromptWithKeywords(startupPrompt);

            try
            {
                // Appeler l'API OpenAI pour chaque startup générée et attendre la réponse
                string response = await OpenAIClient.SendPromptAsync(systemPrompt, finalPrompt, selectedModel, maxTokens, temperature);


                // Parse la réponse pour extraire le contenu JSON
                string content = ExtractJsonContentFromResponse(response);

                Debug.Log("Response received" + content);

                if (!string.IsNullOrEmpty(content))
                {
                    SaveGeneratedStartupToFile(content); // Sauvegarder le contenu JSON dans un fichier
                    Debug.Log($"Startup #{i + 1} generated and saved. finalPrompt: {finalPrompt}");

                    if (generateImage)
                    {
                        // Générer une image pour la startup
                        await GenerateFounderImage(content);
                    }
                }
                else
                {
                    Debug.LogError($"Failed to extract JSON content for startup #{i + 1}");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error generating startup #{i + 1}: {ex.Message}");
            }
        }

        Debug.Log("Startup generation complete.");
    }

    private async Task GenerateFounderImage(string jsonContent)
    {
        // Construire le prompt basé sur le contenu JSON
        string prompt = ExtractImagePromptFromJson(jsonContent);

        try
        {
            Debug.Log("Generating founder image..." + selectedImageSize + "/" + selectedImageModel + " prompt:" + prompt);

            if (selectedImageModel.Equals("dall-e-3"))
            {
                Debug.Log("Generating dall-e-3 image...");
                Texture2D image = await OpenAIClient.SendPromptImageAsync(prompt, selectedImageSize, selectedImageModel);

                if (image != null)
                {
                    // Extraire le nom de la startup depuis le JSON pour nommer le fichier
                    string startupName = ExtractStartupNameFromJson(jsonContent);

                    if (string.IsNullOrEmpty(startupName))
                    {
                        Debug.LogError("Failed to extract 'StartupName' from the JSON content for founder image.");
                        return;
                    }

                    string fileName = $"{ToCamelCase(startupName)}_founder.png";

                    // Chemin complet pour l'image
                    string generatedFolderPath = Path.Combine(Application.streamingAssetsPath, "GeneratedStartups");

                    if (!Directory.Exists(generatedFolderPath))
                    {
                        Directory.CreateDirectory(generatedFolderPath);
                    }

                    string filePath = Path.Combine(generatedFolderPath, fileName);

                    // Sauvegarder l'image en PNG
                    File.WriteAllBytes(filePath, image.EncodeToPNG());
                    Debug.Log($"Founder image saved to: {filePath}");
                }
                else
                {
                    Debug.LogError("Failed to generate founder image.");
                }
            }
            else
            {
                string style = "Kawaii futuristic UI design, pastel color palette, retro 90s aesthetics, cyber minimalism, cute tech interface, cartoonish elements, soft grid background, neon glow accents, stylized 2D character, high-tech gadget with friendly expressions, sci-fi meets cute design";
                string exePath = @"Assets\Resources\main.exe";  // Ensure correct path
                string arguments = $"\"192.168.1.72:8188\" \"{prompt}{style}\"";  // Include quotes for parameters with spaces

                RunProcess(exePath, arguments);

                Debug.Log("Image generated successfully.");

                // Extraire le nom de la startup depuis le JSON pour nommer le fichier
                string startupName = ExtractStartupNameFromJson(jsonContent);

                //Image is generated in /output.png, move it to the correct folder
                string fileName = $"{ToCamelCase(startupName)}_founder.png";

                // Chemin complet pour l'image
                string generatedFolderPath = Path.Combine(Application.streamingAssetsPath, "GeneratedStartups");

                if (!Directory.Exists(generatedFolderPath))
                {
                    Directory.CreateDirectory(generatedFolderPath);
                }

                // Move the image to the correct folder
                string sourcePath = "output_50_0.png";
                string destinationPath = Path.Combine(generatedFolderPath, fileName);

                if (File.Exists(sourcePath))
                {
                    File.Move(sourcePath, destinationPath);
                    Debug.Log($"Founder image saved to: {destinationPath}");
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error generating founder image: {ex.Message}");
        }
    }

    static void RunProcess(string exePath, string arguments)
    {
        System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = exePath,
            Arguments = arguments,
            RedirectStandardOutput = true,  // Capture output if needed
            RedirectStandardError = true,
            UseShellExecute = false,  // Important for output redirection
            CreateNoWindow = true  // Prevent showing a command prompt window
        };

        try
        {
            using (System.Diagnostics.Process process = new System.Diagnostics.Process())
            {
                process.StartInfo = startInfo;
                process.Start();

                // Read output (optional)
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();  // Ensure the process completes before continuing
            }
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError("Error starting process: " + e.Message);
        }
    }

    /// <summary>
    /// Remplace les mots-clés entre accolades dans le prompt par des valeurs aléatoires des listes correspondantes.
    /// </summary>
    /// <param name="prompt">Le prompt brut contenant des mots-clés entre accolades.</param>
    /// <returns>Le prompt final avec les mots-clés remplacés.</returns>
    private string GeneratePromptWithKeywords(string prompt)
    {
        foreach (var entry in keywordLists)
        {
            string placeholder = $"{{{entry.Key}}}"; // Créer le placeholder avec des accolades
            if (prompt.Contains(placeholder))
            {
                // Remplacer le placeholder par un mot-clé aléatoire de la liste
                string randomKeyword = entry.Value[Random.Range(0, entry.Value.Count)];
                prompt = prompt.Replace(placeholder, randomKeyword);
            }
        }

        return prompt;
    }

    private void SaveGeneratedStartupToFile(string content)
    {
        // Extraire le champ "StartupName" depuis le JSON
        string startupName = ExtractStartupNameFromJson(content);

        if (string.IsNullOrEmpty(startupName))
        {
            Debug.LogError("Failed to extract 'StartupName' from the JSON content.");
            return;
        }

        // Convertir le nom en camelCase et supprimer les espaces
        string fileName = $"{ToCamelCase(startupName)}.json";

        // Chemin complet pour le fichier
        string generatedFolderPath = Path.Combine(Application.streamingAssetsPath, "GeneratedStartups");

        if (!Directory.Exists(generatedFolderPath))
        {
            Directory.CreateDirectory(generatedFolderPath);
            Debug.Log($"Created directory: {generatedFolderPath}");
        }

        string filePath = Path.Combine(generatedFolderPath, fileName);

        try
        {
            // Sauvegarder le contenu de la startup
            File.WriteAllText(filePath, content);
            Debug.Log($"Startup saved to: {filePath}");

            // Mettre à jour startups.json
            UpdateStartupsJson();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to save startup to file: {ex.Message}");
        }
    }



    private string ExtractJsonContentFromResponse(string response)
    {
        try
        {
            // Désérialiser la réponse dans un objet ChatResponse
            ChatResponse chatResponse = JsonUtility.FromJson<ChatResponse>(response);

            // Extraire le contenu JSON depuis choices[0].message.content
            if (chatResponse != null && chatResponse.choices != null && chatResponse.choices.Length > 0)
            {
                string jsonContent = chatResponse.choices[0].message.content.Replace("json", "").Replace("```", ""); // Supprimer les balises de code
                return jsonContent;
            }
            else
            {
                Debug.LogError("Invalid response format: choices array is empty or null.");
                return null;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to parse response JSON: {ex.Message}");
            return null;
        }
    }

    private string ExtractStartupNameFromJson(string jsonContent)
    {
        try
        {
            // Désérialiser le JSON partiellement pour accéder uniquement au StartupName
            var startupData = JsonUtility.FromJson<StartupData>(jsonContent);
            return startupData?.StartupName ?? "";
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to extract 'StartupName' from JSON: {ex.Message}");
            return null;
        }
    }

    private string ExtractImagePromptFromJson(string jsonContent)
    {
        try
        {
            // Désérialiser le JSON partiellement pour accéder uniquement à l'ImagePrompt
            var startupData = JsonUtility.FromJson<StartupData>(jsonContent);
            return startupData?.ImagePrompt ?? "";
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to extract 'ImagePrompt' from JSON: {ex.Message}");
            return null;
        }
    }

    private string ToCamelCase(string input)
    {
        if (string.IsNullOrEmpty(input)) return "";

        // Supprimer les espaces et capitaliser chaque mot
        string[] words = input.Split(' ', '-', '_');
        string camelCase = string.Join("", words.Select(w => char.ToUpperInvariant(w[0]) + w.Substring(1).ToLowerInvariant()));

        // Retourner le résultat avec la première lettre en minuscule
        return char.ToLowerInvariant(camelCase[0]) + camelCase.Substring(1);
    }


    private void UpdateStartupsJson()
    {
        string startupsFilePath = Path.Combine(Application.streamingAssetsPath, "startups.json");
        string generatedFolderPath = Path.Combine(Application.streamingAssetsPath, "GeneratedStartups");

        // Liste pour stocker les noms des startups
        List<string> startupNames = new List<string>();

        // Vérifier si le dossier existe
        if (Directory.Exists(generatedFolderPath))
        {
            // Parcourir tous les fichiers JSON dans le dossier
            string[] jsonFiles = Directory.GetFiles(generatedFolderPath, "*.json");
            foreach (string file in jsonFiles)
            {
                try
                {
                    // Lire le contenu du fichier
                    string content = File.ReadAllText(file);

                    // Extraire le nom de la startup
                    string startupName = ExtractStartupNameFromJson(content);
                    if (!string.IsNullOrEmpty(startupName))
                    {
                        startupNames.Add(ToCamelCase(startupName)); // Ajouter le nom à la liste en camelCase
                    }
                    else
                    {
                        Debug.LogWarning($"Unable to extract StartupName from file: {file}");
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Error reading or parsing file {file}: {ex.Message}");
                }
            }
        }
        else
        {
            Debug.LogError($"Generated startups folder not found at: {generatedFolderPath}");
            return;
        }

        // Construire l'objet StartupList
        StartupList startupList = new StartupList { startups = startupNames };

        // Sauvegarder le fichier startups.json
        try
        {
            string updatedJson = JsonUtility.ToJson(startupList, true);
            File.WriteAllText(startupsFilePath, updatedJson);
            Debug.Log($"startups.json successfully rebuilt with {startupNames.Count} startups.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to rebuild startups.json: {ex.Message}");
        }
    }



    // Classe temporaire pour extraire uniquement le StartupName
    [System.Serializable]
    private class StartupData
    {
        public string StartupName;
        public string ImagePrompt;
    }

    [System.Serializable]
    private class StartupList
    {
        public List<string> startups;
    }

}
