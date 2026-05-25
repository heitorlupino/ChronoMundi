#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ArtifactSyncDebugger
{
    const string SCENES_PATH = "Assets/Projeto/Assets/Scenes/";
    const string ARTIFACTS_PATH = "Assets/Projeto/Assets/Data/Artifacts";
    static readonly string[] HISTORICAL_SCENES =
        { "PreHistoriaScene", "IdadeMediaScene", "FuturoTechScene" };

    [MenuItem("ChronoMundi/🐛 DEBUG — Verificar Sincronização")]
    public static void DebugSync()
    {
        Debug.Log("\n" + new string('=', 70));
        Debug.Log("CHRONOMUNDI — DEBUG DE SINCRONIZAÇÃO");
        Debug.Log(new string('=', 70));

        foreach (var sceneName in HISTORICAL_SCENES)
        {
            Debug.Log($"\n--- CENA: {sceneName} ---");
            DebugScene(sceneName);
        }

        Debug.Log("\n" + new string('=', 70));
        Debug.Log("DEBUG CONCLUÍDO");
        Debug.Log(new string('=', 70) + "\n");
    }

    static void DebugScene(string sceneName)
    {
        var scene = EditorSceneManager.OpenScene(
            $"{SCENES_PATH}{sceneName}.unity", OpenSceneMode.Single);

        int count = 0;
        foreach (var root in scene.GetRootGameObjects())
        {
            var exhibits = root.GetComponentsInChildren<InteracleObject>(true);
            foreach (var exhibit in exhibits)
            {
                count++;
                DebugExhibit(count, exhibit);
            }
        }

        if (count == 0)
            Debug.Log("  ❌ Nenhum InteracleObject encontrado!");
    }

    static void DebugExhibit(int index, InteracleObject exhibit)
    {
        Debug.Log($"\n  [{index}] GameObject: {exhibit.gameObject.name}");
        Debug.Log($"      artifactName: {exhibit.artifactName}");
        Debug.Log($"      artifactDescription: {exhibit.artifactDescription.Substring(0, Mathf.Min(50, exhibit.artifactDescription.Length))}...");

        // Tenta gerar o ID
        string goName = exhibit.gameObject.name;
        if (!goName.StartsWith("Artefato_"))
        {
            Debug.Log($"      ❌ ERRO: GameObject não começa com 'Artefato_'");
            return;
        }

        string baseName = goName.Replace("Artefato_", "");
        Debug.Log($"      baseName extraído: {baseName}");

        // Busca SOs
        string[] assetGuids = AssetDatabase.FindAssets($"{baseName} t:ArtifactDataSO", new[] { ARTIFACTS_PATH });
        Debug.Log($"      Buscando: '{baseName}' em {ARTIFACTS_PATH}");
        Debug.Log($"      ✓ Encontrados: {assetGuids.Length} SO(s)");

        if (assetGuids.Length > 0)
        {
            for (int i = 0; i < assetGuids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(assetGuids[i]);
                var so = AssetDatabase.LoadAssetAtPath<ArtifactDataSO>(assetPath);
                if (so != null)
                {
                    Debug.Log($"        [{i}] {assetPath}");
                    Debug.Log($"            ID: {so.artifactID}");
                    Debug.Log($"            Name: {so.artifactName}");
                    Debug.Log($"            Desc: {so.artifactDescription.Substring(0, Mathf.Min(50, so.artifactDescription.Length))}...");
                }
            }
        }
        else
        {
            Debug.Log($"      ❌ Nenhum SO encontrado para '{baseName}'");
        }
    }

    [MenuItem("ChronoMundi/🐛 DEBUG — Listar Todos os Assets")]
    public static void ListAllAssets()
    {
        Debug.Log("\n" + new string('=', 70));
        Debug.Log("TODOS OS ARTIFACTDATASO NA PASTA:");
        Debug.Log(new string('=', 70));

        string[] guids = AssetDatabase.FindAssets("t:ArtifactDataSO", new[] { ARTIFACTS_PATH });
        Debug.Log($"Total: {guids.Length} SOs\n");

        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var so = AssetDatabase.LoadAssetAtPath<ArtifactDataSO>(path);
            if (so != null)
            {
                Debug.Log($"📄 {Path.GetFileName(path)}");
                Debug.Log($"   ID: {so.artifactID}");
                Debug.Log($"   Name: {so.artifactName}");
                Debug.Log($"   Era: {so.era}");
                Debug.Log("");
            }
        }

        Debug.Log(new string('=', 70) + "\n");
    }
}
#endif