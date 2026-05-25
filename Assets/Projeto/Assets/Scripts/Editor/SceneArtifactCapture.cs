#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Varre as cenas e CAPTURA os dados dos artefatos presentes,
/// criando/atualizando os ArtifactDataSO automaticamente.
/// 
/// Útil quando você tem artefatos com dados corretos na cena
/// e quer "guardar" esses dados na database.
/// 
/// Menu: ChronoMundi → 📸 Capturar Artefatos das Cenas → Database
/// </summary>
public static class SceneArtifactCapture
{
    const string SCENES_PATH = "Assets/Projeto/Assets/Scenes/";
    static readonly string[] HISTORICAL_SCENES =
        { "PreHistoriaScene", "IdadeMediaScene", "FuturoTechScene" };

    // ════════════════════════════════════════════════════════════════════
    [MenuItem("ChronoMundi/📸 Capturar Artefatos das Cenas → Database")]
    public static void CaptureAllScenes()
    {
        if (!EditorUtility.DisplayDialog("ChronoMundi — Capturar Artefatos",
            "Vai CAPTURAR dados de todos os artefatos presentes nas cenas\n" +
            "e CRIAR/ATUALIZAR os ArtifactDataSO correspondentes.\n\n" +
            "✓ Usa: artifactName + posição/escala como ID\n" +
            "✓ Cria SOs automaticamente\n" +
            "✓ Depois use 'Sincronizar Descrições' para aplicar\n\n" +
            "Continuar?", "Capturar", "Cancelar"))
            return;

        float p = 0f;
        int totalCreated = 0, totalUpdated = 0;

        foreach (var sceneName in HISTORICAL_SCENES)
        {
            p += 0.33f;
            EditorUtility.DisplayProgressBar("ChronoMundi", 
                $"Capturando {sceneName}...", p);
            var (created, updated) = CaptureScene(sceneName);
            totalCreated += created;
            totalUpdated += updated;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();

        EditorUtility.DisplayDialog("ChronoMundi ✅ Captura Completa",
            $"Artefatos capturados com sucesso!\n\n" +
            $"✔ Criados: {totalCreated}\n" +
            $"✔ Atualizados: {totalUpdated}\n\n" +
            $"Próximo passo:\n" +
            $"ChronoMundi → 🔄 Sincronizar Descrições",
            "OK");
    }

    [MenuItem("ChronoMundi/📸 Capturar Artefatos — Cena Atual")]
    public static void CaptureCurrent()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (!System.Array.Exists(HISTORICAL_SCENES, s => s == scene.name))
        {
            EditorUtility.DisplayDialog("ChronoMundi",
                $"'{scene.name}' não é uma cena histórica.",
                "OK");
            return;
        }

        var (created, updated) = CaptureScene(scene.name);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("ChronoMundi ✅",
            $"Captura de '{scene.name}' concluída!\n\n" +
            $"✔ Criados: {created}\n" +
            $"✔ Atualizados: {updated}",
            "OK");
    }

    // ════════════════════════════════════════════════════════════════════

    static (int created, int updated) CaptureScene(string sceneName)
    {
        var scene = EditorSceneManager.OpenScene(
            $"{SCENES_PATH}{sceneName}.unity", OpenSceneMode.Single);

        int created = 0, updated = 0;
        var eraType = InferEraFromSceneName(sceneName);

        // Encontra todos os InteracleObject
        foreach (var root in scene.GetRootGameObjects())
        {
            var exhibits = root.GetComponentsInChildren<InteracleObject>(true);
            foreach (var exhibit in exhibits)
            {
                if (CaptureArtifact(exhibit, eraType))
                    created++;
                else
                    updated++;
            }
        }

        return (created, updated);
    }

    // ════════════════════════════════════════════════════════════════════

    static bool CaptureArtifact(InteracleObject exhibit, EraType era)
    {
        if (exhibit == null) return false;

        // Gera artifactID baseado no nome + era
        string artifactID = GenerateArtifactID(exhibit.gameObject.name, era);
        if (string.IsNullOrEmpty(artifactID))
        {
            Debug.LogWarning($"[SceneCapture] Não consegui gerar ID para '{exhibit.gameObject.name}'");
            return false;
        }

        // Cria ou carrega o SO
        const string OUTPUT_PATH = "Assets/Projeto/Assets/Data/Artifacts";
        string path = $"{OUTPUT_PATH}/{artifactID}.asset";

        ArtifactDataSO so = AssetDatabase.LoadAssetAtPath<ArtifactDataSO>(path);
        bool isNew = so == null;

        if (isNew)
        {
            so = ScriptableObject.CreateInstance<ArtifactDataSO>();
            AssetDatabase.CreateAsset(so, path);
            Debug.Log($"[SceneCapture] CRIADO: {artifactID}");
        }
        else
        {
            Debug.Log($"[SceneCapture] ATUALIZADO: {artifactID}");
        }

        // Captura dados do InteracleObject
        so.artifactID          = artifactID;
        so.era                 = era;
        so.artifactName        = exhibit.artifactName;
        so.artifactDescription = exhibit.artifactDescription;
        so.subtitleText        = exhibit.subtitleText;
        so.narrationClip       = exhibit.narrationClip;
        so.textAuthor          = "Capturado da Cena";
        so.lastRevision        = System.DateTime.Now.ToString("yyyy-MM");

        EditorUtility.SetDirty(so);

        return isNew;
    }

    // ════════════════════════════════════════════════════════════════════

    static string GenerateArtifactID(string goName, EraType era)
    {
        // Padrão: "Artefato_Machado" → "Machado_PreHistoria"
        if (!goName.StartsWith("Artefato_")) return null;

        string baseName = goName.Replace("Artefato_", "");
        string eraSuffix = era.ToString();

        return $"{baseName}_{eraSuffix}";
    }

    static EraType InferEraFromSceneName(string sceneName)
    {
        if (sceneName.Contains("PreHistoria")) return EraType.PreHistoria;
        if (sceneName.Contains("IdadeMedia")) return EraType.IdadeMedia;
        if (sceneName.Contains("FuturoTech")) return EraType.FuturoTech;
        return EraType.PreHistoria; // default
    }
}
#endif