#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Corrige dois problemas detectados:
///
///   1. FuturoTechScene — artefatos sem InteracleObject
///      (Reator Nuclear, KRONOS-7, Inteligência Artificial, Super Computador)
///      → adiciona InteracleObject preservando os textos existentes
///
///   2. Todas as cenas — ArtifactInfoPanel e NarratorSystem ausentes
///      → cria os dois em cada cena
///
///   3. ExhibitManager — reconecta com artefatos reais
///
/// Menu: ChronoMundi → 🔧 Fix Completo — Artefatos + Painéis
/// </summary>
public static class ArtifactCompleteFix
{
    const string SCENES_PATH = "Assets/Projeto/Assets/Scenes/";

    static readonly string[] SCENES =
        { "PreHistoriaScene", "IdadeMediaScene", "FuturoTechScene" };

    static readonly Dictionary<string, Color> ERA_COLORS = new()
    {
        { "PreHistoriaScene", new Color(0.9f,  0.5f,  0.15f) },
        { "IdadeMediaScene",  new Color(0.85f, 0.72f, 0.15f) },
        { "FuturoTechScene",  new Color(0.1f,  0.85f, 1f   ) },
    };

    // ════════════════════════════════════════════════════════════════════
    [MenuItem("ChronoMundi/🔧 Fix Completo — Artefatos + Painéis")]
    public static void Run()
    {
        if (!EditorUtility.DisplayDialog("ChronoMundi — Fix Completo",
            "Corrige nas 3 cenas históricas:\n\n" +
            "• Adiciona InteracleObject nos artefatos que estão sem\n" +
            "• Cria NarratorSystem (legenda) se não existir\n" +
            "• Cria ArtifactInfoPanel (painel de info) se não existir\n" +
            "• Reconecta ExhibitManager com os artefatos reais\n\n" +
            "Nenhum texto existente será alterado.",
            "Corrigir tudo", "Cancelar"))
            return;

        string report = "";
        float p = 0f;

        foreach (var sceneName in SCENES)
        {
            p += 0.3f;
            Prog($"Corrigindo {sceneName}...", p);
            var (fixed_io, total) = FixScene(sceneName);
            report += $"\n• {sceneName}: {total} artefato(s) ({fixed_io} InteracleObject adicionado(s))";
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();

        EditorUtility.DisplayDialog("ChronoMundi ✅ Fix Completo",
            "Concluído!" + report +
            "\n\nAo pressionar [E] num artefato agora:\n" +
            "• Legenda aparece em baixo\n" +
            "• Painel com nome + descrição abre no centro\n" +
            "• [F] fecha o painel",
            "OK");
    }

    // ════════════════════════════════════════════════════════════════════

    static (int fixedIO, int total) FixScene(string sceneName)
    {
        var scene = EditorSceneManager.OpenScene(
            SCENES_PATH + sceneName + ".unity", OpenSceneMode.Single);

        var color = ERA_COLORS[sceneName];

        // 1. Encontra todos os GameObjects com nome de artefato
        //    e garante que têm InteracleObject
        int fixedIO = EnsureInteracleObjects(scene);

        // 2. Coleta todos os InteracleObjects válidos
        var artifacts = CollectArtifacts(scene);

        // 3. Preenche subtitleText vazio
        foreach (var io in artifacts)
        {
            if (string.IsNullOrWhiteSpace(io.subtitleText) &&
                !string.IsNullOrWhiteSpace(io.artifactName))
            {
                io.subtitleText = io.artifactName;
                EditorUtility.SetDirty(io);
            }
        }

        // 4. NarratorSystem
        EnsureNarratorSystem(scene);

        // 5. ArtifactInfoPanel
        EnsureArtifactInfoPanel(scene, color);

        // 6. ExhibitManager
        ConnectExhibitManager(scene, artifacts);

        // 7. EventSystem
        EnsureEventSystem(scene);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        return (fixedIO, artifacts.Count);
    }

    // ════════════════════════════════════════════════════════════════════
    // PASSO 1 — garante InteracleObject em todos os artefatos
    // Detecta GameObjects pelo nome (contém "Artefato_") ou que já
    // tenham HighlightController (adicionado pelos RoomBuilders)
    // ════════════════════════════════════════════════════════════════════

    static int EnsureInteracleObjects(UnityEngine.SceneManagement.Scene scene)
    {
        int count = 0;

        foreach (var root in scene.GetRootGameObjects())
        {
            // Busca em toda a hierarquia
            foreach (Transform t in GetAllChildren(root.transform))
            {
                var go = t.gameObject;

                // Pula se já tem InteracleObject
                if (go.GetComponent<InteracleObject>() != null) continue;

                // Candidato: tem HighlightController (adicionado pelo RoomBuilder)
                // OU nome começa com "Artefato_"
                bool isArtifact = go.GetComponent<HighlightController>() != null
                               || go.name.StartsWith("Artefato_");

                if (!isArtifact) continue;

                // Adiciona InteracleObject
                var io = go.AddComponent<InteracleObject>();

                // Tenta preencher nome a partir do nome do GameObject
                if (string.IsNullOrWhiteSpace(io.artifactName))
                {
                    io.artifactName = go.name.Replace("Artefato_", "").Replace("_", " ");
                }

                io.interactOnlyOnce = true;

                // Garante Collider
                if (go.GetComponent<Collider>() == null)
                    go.AddComponent<BoxCollider>();

                // Layer Interactable
                int layer = LayerMask.NameToLayer("Interactable");
                if (layer >= 0) go.layer = layer;

                EditorUtility.SetDirty(go);
                count++;
                Debug.Log($"[CompleteFix] InteracleObject adicionado: '{go.name}'");
            }
        }

        return count;
    }

    static List<Transform> GetAllChildren(Transform parent)
    {
        var list = new List<Transform>();
        foreach (Transform child in parent)
        {
            list.Add(child);
            list.AddRange(GetAllChildren(child));
        }
        return list;
    }

    // ════════════════════════════════════════════════════════════════════
    // PASSO 2 — coleta todos os InteracleObjects válidos
    // ════════════════════════════════════════════════════════════════════

    static List<InteracleObject> CollectArtifacts(UnityEngine.SceneManagement.Scene scene)
    {
        var list = new List<InteracleObject>();
        foreach (var root in scene.GetRootGameObjects())
            foreach (var io in root.GetComponentsInChildren<InteracleObject>(true))
                if (!string.IsNullOrWhiteSpace(io.artifactName))
                    list.Add(io);
        return list;
    }

    // ════════════════════════════════════════════════════════════════════
    // NARRATOR SYSTEM
    // ════════════════════════════════════════════════════════════════════

    static void EnsureNarratorSystem(UnityEngine.SceneManagement.Scene scene)
    {
        foreach (var root in scene.GetRootGameObjects())
            if (root.GetComponentInChildren<NarratorSystem>(true) != null) return;

        var go = new GameObject("NarratorCanvas");
        SceneManager.MoveGameObjectToScene(go, scene);
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20;
        go.AddComponent<CanvasScaler>();

        var panel = new GameObject("SubtitlePanel");
        panel.transform.SetParent(go.transform, false);
        panel.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.75f);
        var pRT = panel.GetComponent<RectTransform>();
        pRT.anchorMin = new Vector2(0.05f, 0.02f);
        pRT.anchorMax = new Vector2(0.95f, 0.15f);
        pRT.offsetMin = Vector2.zero;
        pRT.offsetMax = Vector2.zero;

        var textGO = new GameObject("SubtitleText");
        textGO.transform.SetParent(panel.transform, false);
        var tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.fontSize           = 20f;
        tmp.color              = Color.white;
        tmp.alignment          = TextAlignmentOptions.Center;
        tmp.enableWordWrapping = true;
        var tRT = textGO.GetComponent<RectTransform>();
        tRT.anchorMin = Vector2.zero;
        tRT.anchorMax = Vector2.one;
        tRT.offsetMin = new Vector2(16f, 8f);
        tRT.offsetMax = new Vector2(-16f, -8f);

        var narrator = go.AddComponent<NarratorSystem>();
        narrator.subtitleText     = tmp;
        narrator.subtitlePanel    = panel;
        narrator.textOnlyDuration = 5f;
        narrator.audioSource      = go.AddComponent<AudioSource>();

        panel.SetActive(false);
        EditorUtility.SetDirty(go);
    }

    // ════════════════════════════════════════════════════════════════════
    // ARTIFACT INFO PANEL
    // ════════════════════════════════════════════════════════════════════

    static void EnsureArtifactInfoPanel(
        UnityEngine.SceneManagement.Scene scene, Color accent)
    {
        // Remove qualquer painel antigo e recria limpo
        foreach (var root in scene.GetRootGameObjects())
            if (root.name == "ArtifactInfoCanvas") Object.DestroyImmediate(root);

        var go = new GameObject("ArtifactInfoCanvas");
        SceneManager.MoveGameObjectToScene(go, scene);
        go.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        go.GetComponent<Canvas>().sortingOrder = 30;
        go.AddComponent<CanvasScaler>();
        go.AddComponent<GraphicRaycaster>();

        // Painel central
        var panel = new GameObject("InfoPanel");
        panel.transform.SetParent(go.transform, false);
        panel.AddComponent<RectTransform>();
        panel.AddComponent<Image>().color = new Color(0.04f, 0.04f, 0.08f, 0.95f);
        var pRT = panel.GetComponent<RectTransform>();
        pRT.anchorMin = new Vector2(0.12f, 0.12f);
        pRT.anchorMax = new Vector2(0.88f, 0.90f);
        pRT.offsetMin = Vector2.zero;
        pRT.offsetMax = Vector2.zero;

        // Barra colorida
        var bar = new GameObject("TopBar");
        bar.transform.SetParent(panel.transform, false);
        bar.AddComponent<RectTransform>();
        bar.AddComponent<Image>().color = accent;
        var bRT = bar.GetComponent<RectTransform>();
        bRT.anchorMin = new Vector2(0f, 1f);
        bRT.anchorMax = new Vector2(1f, 1f);
        bRT.sizeDelta        = new Vector2(0f, 6f);
        bRT.anchoredPosition = new Vector2(0f, -3f);

        // Título
        var titleGO = new GameObject("Title");
        titleGO.transform.SetParent(panel.transform, false);
        titleGO.AddComponent<RectTransform>();
        var titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
        titleTMP.fontSize  = 24f;
        titleTMP.color     = accent;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.alignment = TextAlignmentOptions.Left;
        var tRT2 = titleGO.GetComponent<RectTransform>();
        tRT2.anchorMin        = new Vector2(0f, 1f);
        tRT2.anchorMax        = new Vector2(1f, 1f);
        tRT2.sizeDelta        = new Vector2(-32f, 52f);
        tRT2.anchoredPosition = new Vector2(16f, -44f);

        // Separador
        var sep = new GameObject("Sep");
        sep.transform.SetParent(panel.transform, false);
        sep.AddComponent<RectTransform>();
        sep.AddComponent<Image>().color = new Color(1f,1f,1f,0.1f);
        var sRT = sep.GetComponent<RectTransform>();
        sRT.anchorMin        = new Vector2(0f, 1f);
        sRT.anchorMax        = new Vector2(1f, 1f);
        sRT.sizeDelta        = new Vector2(-32f, 2f);
        sRT.anchoredPosition = new Vector2(0f, -76f);

        // ScrollView
        var scrollGO = new GameObject("ScrollView");
        scrollGO.transform.SetParent(panel.transform, false);
        scrollGO.AddComponent<RectTransform>();
        var scroll = scrollGO.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        var scRT = scrollGO.GetComponent<RectTransform>();
        scRT.anchorMin = new Vector2(0f, 0f);
        scRT.anchorMax = new Vector2(1f, 1f);
        scRT.offsetMin = new Vector2(16f, 48f);
        scRT.offsetMax = new Vector2(-16f, -82f);

        var viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollGO.transform, false);
        viewport.AddComponent<RectTransform>();
        viewport.AddComponent<Image>().color = Color.clear;
        viewport.AddComponent<Mask>().showMaskGraphic = false;
        var vRT = viewport.GetComponent<RectTransform>();
        vRT.anchorMin = Vector2.zero;
        vRT.anchorMax = Vector2.one;
        vRT.offsetMin = Vector2.zero;
        vRT.offsetMax = Vector2.zero;

        var content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        content.AddComponent<RectTransform>();
        var cRT = content.GetComponent<RectTransform>();
        cRT.anchorMin = new Vector2(0f, 1f);
        cRT.anchorMax = new Vector2(1f, 1f);
        cRT.pivot     = new Vector2(0.5f, 1f);
        cRT.offsetMin = Vector2.zero;
        cRT.offsetMax = Vector2.zero;
        var csf = content.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var descGO = new GameObject("Description");
        descGO.transform.SetParent(content.transform, false);
        descGO.AddComponent<RectTransform>();
        var dRT = descGO.GetComponent<RectTransform>();
        dRT.anchorMin = Vector2.zero;
        dRT.anchorMax = Vector2.one;
        dRT.offsetMin = new Vector2(4f, 8f);
        dRT.offsetMax = new Vector2(-4f, 0f);
        var descTMP = descGO.AddComponent<TextMeshProUGUI>();
        descTMP.fontSize           = 18f;
        descTMP.color              = new Color(0.92f, 0.92f, 0.92f);
        descTMP.alignment          = TextAlignmentOptions.TopLeft;
        descTMP.enableWordWrapping = true;

        scroll.viewport = vRT;
        scroll.content  = cRT;

        // Dica [F]
        var hint = new GameObject("CloseHint");
        hint.transform.SetParent(panel.transform, false);
        hint.AddComponent<RectTransform>();
        var hRT = hint.GetComponent<RectTransform>();
        hRT.anchorMin        = new Vector2(0f, 0f);
        hRT.anchorMax        = new Vector2(1f, 0f);
        hRT.sizeDelta        = new Vector2(-32f, 30f);
        hRT.anchoredPosition = new Vector2(-8f, 18f);
        var hTMP = hint.AddComponent<TextMeshProUGUI>();
        hTMP.text      = "[F]  Fechar";
        hTMP.fontSize  = 14f;
        hTMP.color     = new Color(1f,1f,1f,0.4f);
        hTMP.fontStyle = FontStyles.Italic;
        hTMP.alignment = TextAlignmentOptions.Right;

        // Componente
        var aip             = go.AddComponent<ArtifactInfoPanel>();
        aip.panel           = panel;
        aip.titleText       = titleTMP;
        aip.descriptionText = descTMP;
        aip.closeKey        = KeyCode.F;

        panel.SetActive(false);
        EditorUtility.SetDirty(go);
    }

    // ════════════════════════════════════════════════════════════════════
    // EXHIBIT MANAGER
    // ════════════════════════════════════════════════════════════════════

    static void ConnectExhibitManager(
        UnityEngine.SceneManagement.Scene scene, List<InteracleObject> artifacts)
    {
        ExhibitManager em = null;
        foreach (var root in scene.GetRootGameObjects())
        {
            em = root.GetComponentInChildren<ExhibitManager>(true);
            if (em != null) break;
        }

        if (em == null)
        {
            var go = new GameObject("ExhibitManager");
            SceneManager.MoveGameObjectToScene(go, scene);
            em = go.AddComponent<ExhibitManager>();
        }

        em.exhibits = artifacts;

        // Conecta barra de progresso
        foreach (var root in scene.GetRootGameObjects())
        {
            var slider = root.GetComponentInChildren<Slider>(true);
            if (slider != null) { em.progressBar = slider; break; }
        }
        foreach (var root in scene.GetRootGameObjects())
        {
            foreach (var t in root.GetComponentsInChildren<TextMeshProUGUI>(true))
                if (t.name == "ProgressText") { em.progressText = t; break; }
        }

        EditorUtility.SetDirty(em);
    }

    // ════════════════════════════════════════════════════════════════════

    static void EnsureEventSystem(UnityEngine.SceneManagement.Scene scene)
    {
        foreach (var root in scene.GetRootGameObjects())
            if (root.GetComponentInChildren<UnityEngine.EventSystems.EventSystem>(true) != null)
                return;
        var go = new GameObject("EventSystem");
        SceneManager.MoveGameObjectToScene(go, scene);
        go.AddComponent<UnityEngine.EventSystems.EventSystem>();
        go.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
    }

    static void Prog(string msg, float p) =>
        EditorUtility.DisplayProgressBar("ChronoMundi — Fix Completo", msg, p);
}
#endif
