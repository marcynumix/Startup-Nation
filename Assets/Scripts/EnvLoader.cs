using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class EnvLoader
{
    private static Dictionary<string, string> envVariables = new Dictionary<string, string>();

    // Méthode pour charger le fichier .env
    public static void LoadEnv()
    {
        string envPath = Path.Combine(Application.dataPath, "../.env"); // Chemin vers la racine du projet

        if (File.Exists(envPath))
        {
            string[] lines = File.ReadAllLines(envPath);

            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue; // Ignorer les lignes vides ou les commentaires

                string[] keyValue = line.Split('=');
                if (keyValue.Length == 2)
                {
                    string key = keyValue[0].Trim();
                    string value = keyValue[1].Trim();
                    envVariables[key] = value;
                }
            }
        }
        else
        {
            Debug.LogError($".env file not found at path: {envPath}");
        }
    }

    // Méthode pour récupérer une variable d'environnement
    public static string GetEnv(string key)
    {
        if (envVariables.TryGetValue(key, out string value))
        {
            return value;
        }

        Debug.LogWarning($"Environment variable '{key}' not found.");
        return null;
    }
}
