#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// DEBUG: Verifica se ExhibitManager tem os artefatos conectados.
/// </summary>
public static class FuturoTechExhibitDebug
{
    const string SCENE_PATH = "Assets/Projeto/Assets/Scenes/FuturoTechScene.unity";

    [MenuItem("ChronoMundi/🔍 DEBUG Exhibits FuturoTech")]
    public static void DebugExhibits()
    {
        var scene = EditorSceneManager.OpenScene(SCENE_PATH, OpenSceneMode.Single);

        UnityEngine.Debug.Log("===== DEBUG EXHIBITS =====\n");

        // 1. Encontra ExhibitManager
        ExhibitManager em = null;
        foreach (var root in scene.GetRootGameObjects())
        {
            em = root.GetComponentInChildren<ExhibitManager>(true);
            if (em != null) break;
        }

        if (em == null)
        {
            UnityEngine.Debug.LogError("❌ ExhibitManager NÃO ENCONTRADO!");
            return;
        }

        UnityEngine.Debug.Log($"✅ ExhibitManager encontrado");
        UnityEngine.Debug.Log($"   exhibits.Count: {em.exhibits.Count}");

        if (em.exhibits.Count == 0)
        {
            UnityEngine.Debug.LogError("\n❌ NENHUM ARTEFATO CONECTADO AO EXHIBITMANAGER!");
            UnityEngine.Debug.Log("   Procurando artefatos na cena...\n");

            // Procura por InteracleObject
            int found = 0;
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var io in root.GetComponentsInChildren<InteracleObject>(true))
                {
                    found++;
                    UnityEngine.Debug.Log($"   [{found}] Encontrado: {io.gameObject.name}");
                }
            }

            UnityEngine.Debug.Log($"\nTotal de InteracleObject encontrados na cena: {found}");
            UnityEngine.Debug.Log("\nSolução: Rode 'RECONECTAR Exhibits FuturoTech'");
            return;
        }

        // Lista os artefatos conectados
        UnityEngine.Debug.Log($"\n✅ Artefatos conectados:");
        for (int i = 0; i < em.exhibits.Count; i++)
        {
            UnityEngine.Debug.Log($"   [{i + 1}] {em.exhibits[i].gameObject.name}");
        }

        UnityEngine.Debug.Log($"\n✅ progressText: {(em.progressText != null ? em.progressText.name : "NULL")}");
        UnityEngine.Debug.Log($"✅ progressBar: {(em.progressBar != null ? em.progressBar.name : "NULL")}");
    }
}
#endif
