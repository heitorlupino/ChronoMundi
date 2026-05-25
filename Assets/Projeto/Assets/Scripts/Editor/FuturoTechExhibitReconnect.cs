#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// Reconecta todos os artefatos ao ExhibitManager.
/// 
/// Depois de reconstruir a barra, os artefatos ficam desconectados.
/// Este builder encontra todos os InteracleObject e reconecta ao ExhibitManager.
/// 
/// Menu: ChronoMundi → 🔧 RECONECTAR Exhibits FuturoTech
/// </summary>
public static class FuturoTechExhibitReconnect
{
    const string SCENE_PATH = "Assets/Projeto/Assets/Scenes/FuturoTechScene.unity";

    [MenuItem("ChronoMundi/🔧 RECONECTAR Exhibits FuturoTech")]
    public static void Reconnect()
    {
        var scene = EditorSceneManager.OpenScene(SCENE_PATH, OpenSceneMode.Single);

        // 1. Encontra ExhibitManager
        ExhibitManager em = null;
        foreach (var root in scene.GetRootGameObjects())
        {
            em = root.GetComponentInChildren<ExhibitManager>(true);
            if (em != null) break;
        }

        if (em == null)
        {
            EditorUtility.DisplayDialog("❌ Erro",
                "ExhibitManager não encontrado na cena!",
                "OK");
            return;
        }

        // 2. Encontra todos os InteracleObject
        List<InteracleObject> exhibits = new List<InteracleObject>();
        foreach (var root in scene.GetRootGameObjects())
        {
            foreach (var io in root.GetComponentsInChildren<InteracleObject>(true))
            {
                exhibits.Add(io);
            }
        }

        if (exhibits.Count == 0)
        {
            EditorUtility.DisplayDialog("❌ Erro",
                "Nenhum InteracleObject encontrado na cena!",
                "OK");
            return;
        }

        // 3. Reconecta
        em.exhibits = exhibits;
        EditorUtility.SetDirty(em);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        EditorUtility.DisplayDialog("✅ Reconectado!",
            $"✅ {exhibits.Count} artefatos reconectados ao ExhibitManager!\n\n" +
            $"Teste clicando em um artefato.",
            "OK");

        UnityEngine.Debug.Log($"[ExhibitReconnect] ✅ {exhibits.Count} artefatos reconectados!");
        for (int i = 0; i < exhibits.Count; i++)
        {
            UnityEngine.Debug.Log($"  [{i + 1}] {exhibits[i].gameObject.name}");
        }
    }
}
#endif
