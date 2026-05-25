#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Sincroniza APENAS as descrições (textos) dos artefatos com o database,
/// mantendo posição, escala, rotação, glow color e todo o design intacto.
/// 
/// VERSÃO DINÂMICA: procura todos os .asset na pasta automaticamente.
/// 
/// Menu: ChronoMundi → 🔄 Sincronizar Descrições (Design Preservado)
/// </summary>
public static class ArtifactDescriptionSyncPreserveDesign
{
    const string SCENES_PATH = "Assets/Projeto/Assets/Scenes/";
    const string ARTIFACTS_PATH = "Assets/Projeto/Assets/Data/Artifacts";
    static readonly string[] HISTORICAL_SCENES =
        { "PreHistoriaScene", "IdadeMediaScene", "FuturoTechScene" };

    // ════════════════════════════════════════════════════════════════════
    [MenuItem("ChronoMundi/🔄 Sincronizar Descrições — Todas as Cenas")]
    public static void SyncAllScenes()
    {
        if (!EditorUtility.DisplayDialog("ChronoMundi — Sincronizar Descrições",
            "Vai sincronizar APENAS textos/descrições de todos os artefatos.\n\n" +
            "✓ Posição, escala, rotação, cores — PRESERVADOS\n" +
            "✓ Descrições — ATUALIZADAS\n\n" +
            "Continuar?", "Sincronizar", "Cancelar"))
            return;

        float p = 0f;
        foreach (var sceneName in HISTORICAL_SCENES)
        {
            p += 0.33f;
            EditorUtility.DisplayProgressBar("ChronoMundi", 
                $"Sincronizando {sceneName}...", p);
            SyncScene(sceneName);
        }

        EditorUtility.ClearProgressBar();
        EditorUtility.DisplayDialog("ChronoMundi ✅",
            "Descrições sincronizadas com sucesso!\n\n" +
            "✔ Design preservado\n" +
            "✔ Textos atualizados",
            "OK");
    }

    [MenuItem("ChronoMundi/🔄 Sincronizar Descrições — Cena Atual")]
    public static void SyncCurrentScene()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (!System.Array.Exists(HISTORICAL_SCENES, s => s == scene.name))
        {
            EditorUtility.DisplayDialog("ChronoMundi",
                $"'{scene.name}' não é uma cena histórica.\n\n" +
                "Use: PreHistoriaScene, IdadeMediaScene ou FuturoTechScene",
                "OK");
            return;
        }

        SyncScene(scene.name);
        EditorUtility.DisplayDialog("ChronoMundi ✅",
            $"Descrições de '{scene.name}' sincronizadas!\n" +
            "Design preservado.",
            "OK");
    }

    // ════════════════════════════════════════════════════════════════════

    static void SyncScene(string sceneName)
    {
        var scene = EditorSceneManager.OpenScene(
            $"{SCENES_PATH}{sceneName}.unity", OpenSceneMode.Single);

        int synced = 0, failed = 0;

        // Encontra todos os InteracleObject
        foreach (var root in scene.GetRootGameObjects())
        {
            var exhibits = root.GetComponentsInChildren<InteracleObject>(true);
            foreach (var exhibit in exhibits)
            {
                if (SyncExhibitDescription(exhibit))
                    synced++;
                else
                    failed++;
            }
        }

        if (synced > 0)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[DescriptionSync] {sceneName}: {synced} sincronizado(s)");
        }

        if (failed > 0)
            Debug.LogWarning($"[DescriptionSync] {sceneName}: {failed} falharam");
    }

    // ════════════════════════════════════════════════════════════════════

    static bool SyncExhibitDescription(InteracleObject exhibit)
    {
        if (exhibit == null) return false;

        // Gera artifactID baseado no nome do GameObject
        string artifactID = GenerateArtifactIDFromName(exhibit.gameObject.name);
        if (string.IsNullOrEmpty(artifactID))
        {
            Debug.LogWarning($"[DescriptionSync] Não consegui gerar ID para '{exhibit.gameObject.name}'");
            return false;
        }

        // Tenta carregar o SO
        var data = ArtifactDatabaseBuilder.Load(artifactID);
        if (data == null)
        {
            Debug.LogWarning($"[DescriptionSync] SO não encontrado: {artifactID}");
            return false;
        }

        // Aplica APENAS os textos
        string oldDescription = exhibit.artifactDescription;
        exhibit.artifactName        = data.artifactName;
        exhibit.artifactDescription = data.artifactDescription;
        exhibit.subtitleText        = data.subtitleText;
        exhibit.narrationClip       = data.narrationClip;

        // Log se houve mudança
        if (oldDescription != data.artifactDescription)
        {
            Debug.Log($"[DescriptionSync] Atualizado: {exhibit.gameObject.name} " +
                     $"(ID: {artifactID})");
        }

        EditorUtility.SetDirty(exhibit.gameObject);
        return true;
    }

    // ════════════════════════════════════════════════════════════════════

    static string GenerateArtifactIDFromName(string goName)
    {
        // Padrão: "Artefato_Machado" → tenta encontrar "Machado_*" nos .assets
        if (!goName.StartsWith("Artefato_")) return null;

        string baseName = goName.Replace("Artefato_", "");

        // Busca dinamicamente no disco todos os .asset que contêm esse baseName
        string[] assetGuids = AssetDatabase.FindAssets($"{baseName} t:ArtifactDataSO", new[] { ARTIFACTS_PATH });

        if (assetGuids.Length > 0)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(assetGuids[0]);
            var so = AssetDatabase.LoadAssetAtPath<ArtifactDataSO>(assetPath);
            if (so != null)
                return so.artifactID;
        }

        // Fallback: tenta padrão "NomeBatalho_Era"
        var idMap = new Dictionary<string, string>
        {
            // Pré-História
            { "Machado", "Machado_PreHistoria" },
            { "Osso", "Osso_PreHistoria" },
            { "Pintura", "Pintura_PreHistoria" },

            // Idade Média
            { "Pergaminho", "Pergaminho_IdadeMedia" },
            { "Escudo", "Escudo_IdadeMedia" },
            { "Espada", "Espada_IdadeMedia" },

            // Futuro Tech
            { "Holograma", "Holograma_FuturoTech" },
            { "Chip", "Chip_FuturoTech" },
            { "Exo", "Exo_FuturoTech" },
        };

        return idMap.ContainsKey(baseName) ? idMap[baseName] : null;
    }
}
#endif