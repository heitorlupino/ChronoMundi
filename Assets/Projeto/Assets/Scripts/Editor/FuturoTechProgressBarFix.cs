#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Builder rápido para corrigir a barra de progresso da FuturoTechScene.
/// 
/// Problema: artefatos não tinham XRSimpleInteractable + XRIExhibitAdapter
/// Solução: adiciona os componentes XR necessários para VR funcionar
/// 
/// Menu: ChronoMundi → 🔬 CORRIGIR Barra FuturoTech
/// </summary>
public static class FuturoTechProgressBarFix
{
    const string SCENE_PATH = "Assets/Projeto/Assets/Scenes/FuturoTechScene.unity";

    [MenuItem("ChronoMundi/🔬 CORRIGIR Barra FuturoTech")]
    public static void Fix()
    {
        var scene = EditorSceneManager.OpenScene(SCENE_PATH, OpenSceneMode.Single);

        int fixed_count = 0;
        foreach (var root in scene.GetRootGameObjects())
        {
            foreach (var exhibit in root.GetComponentsInChildren<InteracleObject>(true))
            {
                if (AddXRComponentsToExhibit(exhibit.gameObject))
                    fixed_count++;
            }
        }

        if (fixed_count > 0)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            EditorUtility.DisplayDialog("✅ FuturoTechScene Corrigida",
                $"Barra de progresso corrigida!\n\n" +
                $"• {fixed_count} artefato(s) configurado(s) com XR\n" +
                $"• Cena salva com sucesso",
                "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("ℹ️ Nada a Fazer",
                "Todos os artefatos já estão configurados.",
                "OK");
        }
    }

    static bool AddXRComponentsToExhibit(GameObject go)
    {
        bool added = false;

        // Collider
        if (go.GetComponent<Collider>() == null)
        {
            go.AddComponent<BoxCollider>();
            added = true;
        }

        // Rigidbody kinematic
        var rb = go.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = go.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
            added = true;
        }

        // XRSimpleInteractable
        var xrInteractable = go.GetComponent<XRSimpleInteractable>();
        if (xrInteractable == null)
        {
            xrInteractable = go.AddComponent<XRSimpleInteractable>();
            xrInteractable.interactionLayers = InteractionLayerMask.GetMask("Default");
            added = true;
        }

        // XRIExhibitAdapter
        if (go.GetComponent<XRIExhibitAdapter>() == null)
        {
            go.AddComponent<XRIExhibitAdapter>();
            added = true;
        }

        if (added)
        {
            Debug.Log($"[FuturoTechFix] ✅ {go.name} corrigido com componentes XR");
            EditorUtility.SetDirty(go);
        }

        return added;
    }
}
#endif
