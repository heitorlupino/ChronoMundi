#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.XR.Management;
using UnityEngine;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;

/// <summary>
/// Desfaz tudo que o ChronoMundiXRSetup fez.
///
/// Menu: ChronoMundi → ↩ Desfazer Setup VR (Reverter)
/// </summary>
public static class ChronoMundiXRRevert
{
    const string SCENES_PATH = "Assets/Projeto/Assets/Scenes/";

    static readonly string[] SCENE_NAMES =
        { "MuseuHubScene", "PreHistoriaScene", "IdadeMediaScene", "FuturoTechScene" };

    // nomes que o setup pode ter dado ao XR Origin
    static readonly string[] XR_ORIGIN_NAMES =
        { "XR Origin (VR)", "Complete XR Origin Set Up Variant", "XR Origin", "XR Rig" };

    // ════════════════════════════════════════════════════════════════════
    [MenuItem("ChronoMundi/↩ Desfazer Setup VR (Reverter)")]
    public static void Revert()
    {
        if (!EditorUtility.DisplayDialog("ChronoMundi — Reverter VR",
            "Isso vai DESFAZER o setup VR:\n\n" +
            "• Remove XR Origin de todas as cenas\n" +
            "• Remove VRModeManager de todas as cenas\n" +
            "• Desativa OpenXR Loader (PC + Android)\n\n" +
            "O DesktopPlayer e os artefatos NÃO são afetados.\n\n" +
            "Continuar?", "Sim, reverter", "Cancelar"))
            return;

        Prog("Revertendo OpenXR Loader...", 0.1f);
        int loaderReverted = DeactivateOpenXRLoader();

        int scenesReverted = 0;
        float p = 0.2f;
        foreach (var name in SCENE_NAMES)
        {
            Prog($"Limpando {name}...", p += 0.18f);
            if (CleanScene(name)) scenesReverted++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();

        EditorUtility.DisplayDialog("ChronoMundi ↩ Revertido",
            $"Setup VR desfeito!\n\n" +
            $"✔ OpenXR Loader desativado em {loaderReverted} plataforma(s)\n" +
            $"✔ XR Origin / VRModeManager removidos de {scenesReverted} cena(s)\n\n" +
            "O projeto volta a rodar apenas em modo Desktop.\n" +
            "Para ativar VR novamente: ChronoMundi → 🥽 ATIVAR VR",
            "OK");
    }

    // ════════════════════════════════════════════════════════════════════
    // DESATIVA LOADER
    // ════════════════════════════════════════════════════════════════════

    static int DeactivateOpenXRLoader()
    {
        int count = 0;
        count += DeactivateFor(BuildTargetGroup.Standalone, "PC");
        count += DeactivateFor(BuildTargetGroup.Android,    "Android");
        return count;
    }

    static int DeactivateFor(BuildTargetGroup group, string label)
    {
        var settings = XRGeneralSettingsPerBuildTarget
            .XRGeneralSettingsForBuildTarget(group);

        if (settings == null)
        {
            Debug.Log($"[XRRevert] Nenhum XRGeneralSettings para {label}.");
            return 0;
        }

        var so       = new SerializedObject(settings);
        var autoLoad = so.FindProperty("m_AutomaticLoading");
        var autoRun  = so.FindProperty("m_AutomaticRunning");

        if (autoLoad != null) autoLoad.boolValue = false;
        if (autoRun  != null) autoRun.boolValue  = false;

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(settings);

        Debug.Log($"[XRRevert] AutomaticLoading/Running desativados para {label}.");
        return 1;
    }

    // ════════════════════════════════════════════════════════════════════
    // LIMPA CENAS
    // ════════════════════════════════════════════════════════════════════

    static bool CleanScene(string sceneName)
    {
        string path = SCENES_PATH + sceneName + ".unity";
        if (!System.IO.File.Exists(path)) return false;

        var scene  = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        bool dirty = false;

        foreach (var root in scene.GetRootGameObjects())
        {
            // Remove XR Origin
            foreach (var n in XR_ORIGIN_NAMES)
            {
                if (root.name == n)
                {
                    Object.DestroyImmediate(root);
                    Debug.Log($"[XRRevert] '{n}' removido de {sceneName}.");
                    dirty = true;
                    break;
                }
            }

            // Remove VRModeManager
            if (root == null) continue;
            if (root.name == "VRModeManager")
            {
                Object.DestroyImmediate(root);
                Debug.Log($"[XRRevert] VRModeManager removido de {sceneName}.");
                dirty = true;
            }
        }

        if (dirty)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        return dirty;
    }

    static void Prog(string msg, float p) =>
        EditorUtility.DisplayProgressBar("ChronoMundi — Revertendo VR", msg, p);
}
#endif
