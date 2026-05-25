#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SyncTwoArtifacts
{
    const string SCENES_PATH = "Assets/Projeto/Assets/Scenes/";
    static readonly string[] SCENES = { "PreHistoriaScene", "IdadeMediaScene", "FuturoTechScene" };

    [MenuItem("ChronoMundi/🔄 Sincronizar — Paquicefalossauro e Fogueira")]
    public static void SyncTwo()
    {
        var map = new Dictionary<string, string>
        {
            { "Artefato_Paquicefalossauro", "Paquicefalossauro_PreHistoria" },
            { "Artefato_Fogueira", "Fogueira_PreHistoria" },
        };

        int totalUpdated = 0;

        foreach (var sceneName in SCENES)
        {
            var scene = EditorSceneManager.OpenScene($"{SCENES_PATH}{sceneName}.unity", OpenSceneMode.Single);
            int updatedThisScene = 0;

            foreach (var kv in map)
            {
                var go = FindInScene(scene, kv.Key);
                if (go == null)
                {
                    Debug.Log($"[SyncTwo] Não encontrado: {kv.Key} em {sceneName}");
                    continue;
                }

                var io = go.GetComponent<InteracleObject>();
                if (io == null)
                {
                    Debug.LogWarning($"[SyncTwo] {kv.Key} não tem InteracleObject");
                    continue;
                }

                var so = ArtifactDatabaseBuilder.Load(kv.Value);
                if (so == null)
                {
                    Debug.LogWarning($"[SyncTwo] ArtifactDataSO não encontrado: {kv.Value}");
                    continue;
                }

                // Aplica APENAS textos/áudio (preserva design)
                io.artifactName = so.artifactName;
                io.artifactDescription = so.artifactDescription;
                io.subtitleText = so.subtitleText;
                io.narrationClip = so.narrationClip;

                EditorUtility.SetDirty(go);
                Debug.Log($"[SyncTwo] Atualizado {kv.Key} ← {kv.Value} ({sceneName})");
                updatedThisScene++;
            }

            if (updatedThisScene > 0)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }

            totalUpdated += updatedThisScene;
        }

        EditorUtility.DisplayDialog("ChronoMundi — Sync", $"Descrições atualizadas: {totalUpdated}", "OK");
    }

    static GameObject FindInScene(Scene scene, string name)
    {
        foreach (var root in scene.GetRootGameObjects())
        {
            if (root.name == name) return root;
            var transforms = root.GetComponentsInChildren<Transform>(true);
            foreach (var t in transforms)
                if (t.name == name)
                    return t.gameObject;
        }
        return null;
    }
}
#endif