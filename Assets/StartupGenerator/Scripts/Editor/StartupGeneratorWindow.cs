using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class StartupGeneratorWindow : EditorWindow
{
    private Dictionary<string, List<string>> keywordLists = new Dictionary<string, List<string>>();
    private Vector2 scrollPosition; // Pour le défilement horizontal des mots-clés
    private string startupPrompt = ""; // Contenu du fichier de prompt
    private bool showConfiguration = true; // État du groupe "Configuration"
    private bool showConfigurationData = true; // État du groupe "Configuration Data"
    private bool showGenerateStartups = false; // État du groupe "Generate Startups"

    private int numberOfStartups = 1; // Nombre de startups à générer
    private bool deleteOldData = false; // Supprimer les anciennes données (checkbox)

    [MenuItem("Tools/Startup Generator")]
    public static void ShowWindow()
    {
        GetWindow<StartupGeneratorWindow>("Startup Generator");
    }

    private void OnEnable()
    {
        LoadKeywordFiles();
        LoadStartupPrompt();
    }

    private void LoadKeywordFiles()
    {
        keywordLists.Clear();

        // Chemin vers le dossier Data
        string dataFolderPath = Path.Combine(Application.dataPath, "StartupGenerator/Data");

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

    private void LoadStartupPrompt()
    {
        // Chemin vers le fichier de prompt
        string promptFilePath = Path.Combine(Application.dataPath, "StartupGenerator/Prompts/startup-generation-prompt.txt");

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
        string promptFilePath = Path.Combine(Application.dataPath, "StartupGenerator/Prompts/startup-generation-prompt.txt");

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
            GUILayout.Label("Startup Generation Prompt:", EditorStyles.label);

            var textAreaStyle = new GUIStyle(EditorStyles.textArea)
            {
                wordWrap = true // Active le retour à la ligne automatique
            };
            startupPrompt = EditorGUILayout.TextArea(startupPrompt, textAreaStyle, GUILayout.Height(100));

            if (GUILayout.Button("Save Prompt"))
            {
                SaveStartupPrompt(); // Sauvegarde du prompt
            }

            GUILayout.Space(10);

            showConfigurationData = EditorGUILayout.Foldout(showConfigurationData, "Data");
            if (showConfigurationData)
            {
                // Barre de défilement pour les données
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(position.height - 300));

                EditorGUILayout.BeginHorizontal();

                foreach (var entry in keywordLists)
                {
                    DrawKeywordList(entry.Key, entry.Value); // Afficher chaque liste
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndScrollView();
            }
            
            

            if (GUILayout.Button("Reload Data"))
            {
                LoadKeywordFiles(); // Recharger les fichiers de données
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

            // Checkbox pour supprimer les anciennes données
            deleteOldData = EditorGUILayout.Toggle("Delete Old Data", deleteOldData);

            // Bouton pour générer les startups
            if (GUILayout.Button("Generate Startups"))
            {
                GenerateStartups();
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
            Debug.Log("Old data deleted.");
            // Logique pour supprimer les anciennes données (si applicable)
        }

        // Générer les startups
        for (int i = 0; i < numberOfStartups; i++)
        {
            string finalPrompt = GeneratePromptWithKeywords(startupPrompt);

            // Appeler l'API OpenAI pour chaque startup générée
            string response = await OpenAIClient.SendPromptAsync(finalPrompt);

            // Afficher le résultat dans la console
            Debug.Log($"Generated Startup #{i + 1}: {response}");
        }

        Debug.Log("Startup generation complete.");
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

}
