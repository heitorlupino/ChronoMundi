#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Ferramenta de validação do ChronoMundi (somente Editor).
/// Verifica cada cena e reporta configurações faltantes ou erradas.
///
/// Como usar:
///   Menu Unity → ChronoMundi → Validar Projeto
/// </summary>
public static class ChronoMundiValidator
{
    [MenuItem("ChronoMundi/✅ Validar Projeto")]
    public static void ValidateProject()
    {
        var report = new List<string>();
        int errors = 0, warnings = 0;

        // ── 1. Scripts essenciais ──────────────────────────────────────────
        report.Add("=== SCRIPTS ===");

        CheckType<GameManager>(report, ref errors, ref warnings);
        CheckType<NarratorSystem>(report, ref errors, ref warnings);
        CheckType<LoadingScreen>(report, ref errors, ref warnings);
        CheckType<SceneBootstrapper>(report, ref errors, ref warnings, isStatic: true);
        CheckType<ArtifactInfoPanel>(report, ref errors, ref warnings);
        CheckType<SceneFader>(report, ref errors, ref warnings);
        CheckType<IInteractable>(report, ref errors, ref warnings, isInterface: true);

        // ── 2. Build Settings ──────────────────────────────────────────────
        report.Add("\n=== BUILD SETTINGS ===");
        var scenes = EditorBuildSettings.scenes;
        string[] requiredScenes = {
            "MenuScene", "MuseuHubScene",
            "PreHistoriaScene", "IdadeMediaScene", "FuturoTechScene"
        };

        foreach (var req in requiredScenes)
        {
            bool found = false;
            foreach (var s in scenes)
                if (s.path.Contains(req) && s.enabled) { found = true; break; }

            if (found) report.Add($"  ✔ {req} está no Build Settings");
            else { report.Add($"  ✘ ERRO: {req} não está no Build Settings"); errors++; }
        }

        // ── 3. Cenas ──────────────────────────────────────────────────────
        report.Add("\n=== CENAS ===");
        foreach (var sceneBuild in scenes)
        {
            if (!sceneBuild.enabled) continue;
            ValidateScene(sceneBuild.path, report, ref errors, ref warnings);
        }

        // ── 4. Resumo ─────────────────────────────────────────────────────
        report.Add($"\n=== RESUMO: {errors} erro(s), {warnings} aviso(s) ===");
        string log = string.Join("\n", report);

        if (errors > 0)
            Debug.LogError("[ChronoMundi Validator]\n" + log);
        else if (warnings > 0)
            Debug.LogWarning("[ChronoMundi Validator]\n" + log);
        else
            Debug.Log("[ChronoMundi Validator] Tudo OK!\n" + log);

        EditorUtility.DisplayDialog(
            "ChronoMundi – Validação",
            $"{errors} erro(s) encontrado(s)\n{warnings} aviso(s)\n\nVeja o Console para detalhes.",
            "OK");
    }

    // ──────────────────────────────────────────────────────────────────────
    static void ValidateScene(string path, List<string> report,
                               ref int errors, ref int warnings)
    {
        string sceneName = System.IO.Path.GetFileNameWithoutExtension(path);
        report.Add($"\n  Cena: {sceneName}");

        var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);

        // GameObjects básicos
        bool hasCamera = false, hasLight = false;
        foreach (var go in scene.GetRootGameObjects())
        {
            if (go.GetComponentInChildren<Camera>()) hasCamera = true;
            if (go.GetComponentInChildren<Light>()) hasLight = true;
        }

        if (!hasCamera) { report.Add($"    ✘ ERRO: Sem câmera"); errors++; }
        else report.Add($"    ✔ Camera OK");

        if (!hasLight) { report.Add($"    ⚠ AVISO: Sem luz direcional"); warnings++; }
        else report.Add($"    ✔ Light OK");

        // Por cena — validações específicas
        if (sceneName == "MenuScene")
            ValidateMenuScene(scene, report, ref errors, ref warnings);
        else if (sceneName == "MuseuHubScene")
            ValidateHubScene(scene, report, ref errors, ref warnings);
        else
            ValidateHistoricalScene(scene, sceneName, report, ref errors, ref warnings);

        EditorSceneManager.CloseScene(scene, true);
    }

    static void ValidateMenuScene(Scene scene, List<string> report,
                                   ref int errors, ref int warnings)
    {
        var mgr = FindInScene<MenuManager>(scene);
        if (mgr == null) { report.Add("    ⚠ AVISO: MenuManager não encontrado"); warnings++; }
        else report.Add("    ✔ MenuManager OK");
    }

    static void ValidateHubScene(Scene scene, List<string> report,
                                  ref int errors, ref int warnings)
    {
        var doors = FindAllInScene<MuseumDoor>(scene);
        if (doors.Count == 0) { report.Add("    ⚠ AVISO: Nenhuma MuseumDoor na cena Hub"); warnings++; }
        else report.Add($"    ✔ {doors.Count} MuseumDoor(s) OK");

        foreach (var door in doors)
            if (string.IsNullOrEmpty(door.nomeScene))
            { report.Add($"    ✘ ERRO: MuseumDoor '{door.gameObject.name}' sem cena destino"); errors++; }
    }

    static void ValidateHistoricalScene(Scene scene, string sceneName,
                                         List<string> report, ref int errors, ref int warnings)
    {
        var em = FindInScene<ExhibitManager>(scene);
        if (em == null) { report.Add("    ⚠ AVISO: ExhibitManager não encontrado"); warnings++; }
        else
        {
            report.Add($"    ✔ ExhibitManager com {em.exhibits.Count} exhibit(s)");
            for (int i = 0; i < em.exhibits.Count; i++)
                if (em.exhibits[i] == null)
                { report.Add($"    ✘ ERRO: Exhibit[{i}] é null no ExhibitManager"); errors++; }
        }

        var ret = FindInScene<SimpleReturnToMuseum>(scene);
        if (ret == null) { report.Add("    ⚠ AVISO: Sem SimpleReturnToMuseum"); warnings++; }
        else report.Add("    ✔ SimpleReturnToMuseum OK");
    }

    // ── Helpers ────────────────────────────────────────────────────────────
    static void CheckType<T>(List<string> report, ref int errors, ref int warnings,
                              bool isStatic = false, bool isInterface = false)
    {
        string typeName = typeof(T).Name;
        bool exists = isStatic || isInterface
            ? FindScriptAsset(typeName)
            : FindScriptAsset(typeName);

        if (exists) report.Add($"  ✔ {typeName}.cs encontrado");
        else { report.Add($"  ✘ ERRO: {typeName}.cs NÃO encontrado!"); errors++; }
    }

    static bool FindScriptAsset(string name)
    {
        var guids = AssetDatabase.FindAssets($"{name} t:MonoScript");
        return guids.Length > 0;
    }

    static T FindInScene<T>(Scene scene) where T : Component
    {
        foreach (var go in scene.GetRootGameObjects())
        {
            var c = go.GetComponentInChildren<T>(true);
            if (c != null) return c;
        }
        return null;
    }

    static List<T> FindAllInScene<T>(Scene scene) where T : Component
    {
        var result = new List<T>();
        foreach (var go in scene.GetRootGameObjects())
            result.AddRange(go.GetComponentsInChildren<T>(true));
        return result;
    }
}
#endif
