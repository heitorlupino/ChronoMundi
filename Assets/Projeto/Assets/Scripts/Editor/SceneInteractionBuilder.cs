#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Monta o sistema completo de interação em todas as cenas históricas:
///
///   1. PLAYER — adiciona DesktopPlayerController + PlayerInteraction
///      com prompt "[E] Nome do artefato" ao olhar para o objeto
///
///   2. PAINEL DE INFO — ArtifactInfoPanel com título, descrição e botão fechar
///      aparece ao pressionar E no artefato
///
///   3. NARRATOR — NarratorSystem com canvas de legenda já conectado
///
///   4. BARRA DE PROGRESSO — ExhibitManager com Slider e texto "X / N artefatos"
///      sobe a cada artefato explorado
///
///   5. RECONECTA — ExhibitManager.exhibits[] com os artefatos reais da cena
///
/// Pré-requisito: execute os RoomBuilders antes para que os artefatos existam.
///
/// Menu: ChronoMundi → 🎮 Montar Sistema de Interação — Todas as Cenas
/// </summary>
public static class SceneInteractionBuilder
{
    const string SCENES_PATH = "Assets/Projeto/Assets/Scenes/";

    static readonly string[] HISTORICAL_SCENES =
        { "PreHistoriaScene", "IdadeMediaScene", "FuturoTechScene" };

    // Cores por cena
    static readonly System.Collections.Generic.Dictionary<string, Color> ERA_COLORS = new()
    {
        { "PreHistoriaScene", new Color(0.9f, 0.5f, 0.15f) },   // laranja
        { "IdadeMediaScene",  new Color(0.85f, 0.72f, 0.15f) },  // dourado
        { "FuturoTechScene",  new Color(0.1f, 0.85f, 1f) },      // ciano
    };

    // ════════════════════════════════════════════════════════════════════
    [MenuItem("ChronoMundi/🎮 Montar Sistema de Interação — Todas as Cenas")]
    public static void BuildAll()
    {
        if (!EditorUtility.DisplayDialog("ChronoMundi — Sistema de Interação",
            "Vai montar em PreHistoria, IdadeMedia e FuturoTech:\n\n" +
            "• Prompt '[E] Nome do Artefato' ao olhar\n" +
            "• Painel de informações (nome + descrição)\n" +
            "• Legenda do NarratorSystem\n" +
            "• Barra de progresso (artefatos explorados)\n" +
            "• Reconexão do ExhibitManager\n\n" +
            "Pré-requisito: RoomBuilders já executados.",
            "Montar tudo", "Cancelar"))
            return;

        float p = 0f;
        foreach (var name in HISTORICAL_SCENES)
        {
            p += 0.3f;
            Prog($"Montando {name}...", p);
            BuildScene(name);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();

        EditorUtility.DisplayDialog("ChronoMundi ✅",
            "Sistema de interação montado!\n\n" +
            "✔ PreHistoriaScene\n" +
            "✔ IdadeMediaScene\n" +
            "✔ FuturoTechScene\n\n" +
            "Teste: entre na cena, aproxime-se de\n" +
            "um artefato e pressione [E].",
            "OK");
    }

    [MenuItem("ChronoMundi/🎮 Montar Sistema de Interação — Cena Atual")]
    public static void BuildCurrent()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (!System.Array.Exists(HISTORICAL_SCENES, s => s == scene.name))
        {
            EditorUtility.DisplayDialog("ChronoMundi",
                $"'{scene.name}' não é uma cena histórica.", "OK");
            return;
        }
        BuildScene(scene.name);
        EditorUtility.DisplayDialog("ChronoMundi ✅",
            $"Sistema montado em {scene.name}!", "OK");
    }

    // ════════════════════════════════════════════════════════════════════

    static void BuildScene(string sceneName)
    {
        var scene = EditorSceneManager.OpenScene(
            SCENES_PATH + sceneName + ".unity", OpenSceneMode.Single);

        var color = ERA_COLORS.TryGetValue(sceneName, out var c) ? c : Color.white;

        // EventSystem — obrigatório para qualquer clique de UI funcionar
        EnsureEventSystem(scene);

        // 1. Player
        var player = EnsurePlayer(scene, sceneName);

        // 2. Prompt de interação (filho do player)
        var prompt = BuildPromptUI(scene, player);

        // 3. NarratorSystem canvas
        EnsureNarratorCanvas(scene);

        // 4. ArtifactInfoPanel
        BuildInfoPanel(scene, color);

        // 5. Barra de progresso
        var (progressPanel, progressBar, progressText) = BuildProgressBar(scene, color);

        // 6. Reconecta ExhibitManager
        ReconnectExhibitManager(scene, progressPanel, progressBar, progressText);

        // 7. PlayerInteraction → aponta prompt
        var pi = player.GetComponent<PlayerInteraction>();
        if (pi == null) pi = player.AddComponent<PlayerInteraction>();
        pi.interactionDistance = 3.5f;
        pi.interactableLayer   = ~0;
        pi.isVRMode            = false;
        pi.interactionKey      = KeyCode.E;
        pi.interactionPrompt   = prompt;
        var promptTMP = prompt.GetComponentInChildren<TextMeshProUGUI>();
        pi.promptText = promptTMP;

        var camObj = player.GetComponentInChildren<Camera>();
        if (camObj != null)
        {
            var so   = new SerializedObject(pi);
            var prop = so.FindProperty("playerCamera");
            if (prop != null) { prop.objectReferenceValue = camObj; so.ApplyModifiedProperties(); }
        }

        EditorUtility.SetDirty(player);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    // ════════════════════════════════════════════════════════════════════
    // EVENT SYSTEM — sem isso nenhum botão de UI responde a cliques
    // ════════════════════════════════════════════════════════════════════

    static void EnsureEventSystem(UnityEngine.SceneManagement.Scene scene)
    {
        // Verifica se já existe
        foreach (var root in scene.GetRootGameObjects())
            if (root.GetComponentInChildren<UnityEngine.EventSystems.EventSystem>(true) != null)
                return;

        var esGO = new GameObject("EventSystem");
        SceneManager.MoveGameObjectToScene(esGO, scene);
        esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
        esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

        EditorUtility.SetDirty(esGO);
        Debug.Log("[InteractionBuilder] EventSystem criado.");
    }

    // ════════════════════════════════════════════════════════════════════
    // 1. PLAYER
    // ════════════════════════════════════════════════════════════════════

    static GameObject EnsurePlayer(UnityEngine.SceneManagement.Scene scene, string sceneName)
    {
        // Procura player existente
        foreach (var root in scene.GetRootGameObjects())
        {
            if (root.CompareTag("Player")) return root;
            if (root.name.Contains("Player") &&
                root.GetComponent<CharacterController>() != null) return root;
        }

        // Cria player mínimo
        var go = new GameObject(sceneName + "_Player");
        SceneManager.MoveGameObjectToScene(go, scene);
        go.tag = "Player";
        go.transform.position = new Vector3(0, 0, -4f);

        var cc = go.AddComponent<CharacterController>();
        cc.height = 1.8f;
        cc.radius = 0.3f;
        cc.center = new Vector3(0, 0.9f, 0);

        var dpc = go.AddComponent<DesktopPlayerController>();
        dpc.moveSpeed        = 4f;
        dpc.sprintSpeed      = 7f;
        dpc.mouseSensitivity = 2f;

        var camGO = new GameObject("PlayerCamera");
        camGO.tag = "MainCamera";
        camGO.transform.SetParent(go.transform, false);
        camGO.transform.localPosition = new Vector3(0, 1.6f, 0);
        var cam = camGO.AddComponent<Camera>();
        cam.nearClipPlane = 0.1f;
        camGO.AddComponent<AudioListener>();
        var dpcSO = new SerializedObject(dpc);
        var camProp = dpcSO.FindProperty("playerCamera");
        var ccProp  = dpcSO.FindProperty("characterController");
        if (camProp != null) { camProp.objectReferenceValue = camGO.transform; }
        if (ccProp  != null) { ccProp.objectReferenceValue  = cc; }
        dpcSO.ApplyModifiedProperties();

        Debug.Log($"[InteractionBuilder] Player criado em {sceneName}.");
        return go;
    }

    // ════════════════════════════════════════════════════════════════════
    // 2. PROMPT "[E] Nome do artefato"
    // ════════════════════════════════════════════════════════════════════

    static GameObject BuildPromptUI(
        UnityEngine.SceneManagement.Scene scene, GameObject player)
    {
        // Remove prompt antigo
        var oldPrompt = player.transform.Find("InteractionPrompt");
        if (oldPrompt != null) Object.DestroyImmediate(oldPrompt.gameObject);

        // Canvas em Screen Space Overlay
        var canvasGO = new GameObject("InteractionPrompt");
        canvasGO.transform.SetParent(player.transform, false);

        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        canvasGO.AddComponent<CanvasScaler>();

        // Fundo semi-transparente
        var bg = new GameObject("Background");
        bg.transform.SetParent(canvasGO.transform, false);
        var bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0f, 0f, 0f, 0.55f);
        var bgRT = bg.GetComponent<RectTransform>();
        bgRT.anchorMin        = new Vector2(0.5f, 0.08f);
        bgRT.anchorMax        = new Vector2(0.5f, 0.08f);
        bgRT.sizeDelta        = new Vector2(340f, 64f);
        bgRT.anchoredPosition = Vector2.zero;
        bg.AddComponent<CanvasGroup>();

        // Texto TMP
        var textGO = new GameObject("PromptText");
        textGO.transform.SetParent(bg.transform, false);
        var tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text      = "[E] Interagir";
        tmp.fontSize  = 22f;
        tmp.color     = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        var tRT = textGO.GetComponent<RectTransform>();
        tRT.anchorMin  = Vector2.zero;
        tRT.anchorMax  = Vector2.one;
        tRT.offsetMin  = new Vector2(12f, 0f);
        tRT.offsetMax  = new Vector2(-12f, 0f);

        canvasGO.SetActive(false); // começa oculto
        EditorUtility.SetDirty(canvasGO);
        return canvasGO;
    }

    // ════════════════════════════════════════════════════════════════════
    // 3. NARRATOR CANVAS
    // ════════════════════════════════════════════════════════════════════

    static void EnsureNarratorCanvas(UnityEngine.SceneManagement.Scene scene)
    {
        // Verifica se já existe um NarratorSystem na cena
        foreach (var root in scene.GetRootGameObjects())
            if (root.GetComponentInChildren<NarratorSystem>(true) != null) return;

        // Cria canvas de legenda
        var canvasGO = new GameObject("NarratorCanvas");
        SceneManager.MoveGameObjectToScene(canvasGO, scene);
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20;
        canvasGO.AddComponent<CanvasScaler>();

        // Painel de legenda (parte inferior da tela)
        var panel = new GameObject("SubtitlePanel");
        panel.transform.SetParent(canvasGO.transform, false);
        var panelImg = panel.AddComponent<Image>();
        panelImg.color = new Color(0f, 0f, 0f, 0.7f);
        var panelRT = panel.GetComponent<RectTransform>();
        panelRT.anchorMin        = new Vector2(0.1f, 0.02f);
        panelRT.anchorMax        = new Vector2(0.9f, 0.14f);
        panelRT.offsetMin        = Vector2.zero;
        panelRT.offsetMax        = Vector2.zero;

        // Texto
        var textGO = new GameObject("SubtitleText");
        textGO.transform.SetParent(panel.transform, false);
        var tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text      = "";
        tmp.fontSize  = 20f;
        tmp.color     = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        var tRT = textGO.GetComponent<RectTransform>();
        tRT.anchorMin = Vector2.zero;
        tRT.anchorMax = Vector2.one;
        tRT.offsetMin = new Vector2(16f, 8f);
        tRT.offsetMax = new Vector2(-16f, -8f);

        // NarratorSystem
        var narrator = canvasGO.AddComponent<NarratorSystem>();
        narrator.subtitleText  = tmp;
        narrator.subtitlePanel = panel;
        narrator.textOnlyDuration = 4f;

        var audioSrc = canvasGO.AddComponent<AudioSource>();
        audioSrc.spatialBlend = 0f;
        narrator.audioSource = audioSrc;

        panel.SetActive(false);
        EditorUtility.SetDirty(canvasGO);
        Debug.Log($"[InteractionBuilder] NarratorCanvas criado.");
    }

    // ════════════════════════════════════════════════════════════════════
    // 4. PAINEL DE INFORMAÇÕES DO ARTEFATO
    // ════════════════════════════════════════════════════════════════════

    static void BuildInfoPanel(
        UnityEngine.SceneManagement.Scene scene, Color accentColor)
    {
        // Remove antigo
        foreach (var root in scene.GetRootGameObjects())
            if (root.name == "ArtifactInfoCanvas") Object.DestroyImmediate(root);

        var canvasGO = new GameObject("ArtifactInfoCanvas");
        SceneManager.MoveGameObjectToScene(canvasGO, scene);
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 30;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // Painel central
        var panel = new GameObject("InfoPanel");
        panel.transform.SetParent(canvasGO.transform, false);
        var panelImg = panel.AddComponent<Image>();
        panelImg.color = new Color(0.04f, 0.04f, 0.08f, 0.93f);
        var panelRT = panel.GetComponent<RectTransform>();
        panelRT.anchorMin        = new Vector2(0.15f, 0.2f);
        panelRT.anchorMax        = new Vector2(0.85f, 0.85f);
        panelRT.offsetMin        = Vector2.zero;
        panelRT.offsetMax        = Vector2.zero;

        // Barra de cor no topo
        var bar = new GameObject("ColorBar");
        bar.transform.SetParent(panel.transform, false);
        var barImg = bar.AddComponent<Image>();
        barImg.color = accentColor;
        var barRT = bar.GetComponent<RectTransform>();
        barRT.anchorMin        = new Vector2(0f, 1f);
        barRT.anchorMax        = new Vector2(1f, 1f);
        barRT.sizeDelta        = new Vector2(0f, 6f);
        barRT.anchoredPosition = new Vector2(0f, -3f);

        // Título
        var titleGO = new GameObject("Title");
        titleGO.transform.SetParent(panel.transform, false);
        var titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
        titleTMP.text      = "Artefato";
        titleTMP.fontSize  = 28f;
        titleTMP.color     = accentColor;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.alignment = TextAlignmentOptions.Left;
        var titleRT = titleGO.GetComponent<RectTransform>();
        titleRT.anchorMin        = new Vector2(0f, 1f);
        titleRT.anchorMax        = new Vector2(1f, 1f);
        titleRT.sizeDelta        = new Vector2(-32f, 50f);
        titleRT.anchoredPosition = new Vector2(16f, -40f);

        // Separador
        var sep = new GameObject("Separator");
        sep.transform.SetParent(panel.transform, false);
        var sepImg = sep.AddComponent<Image>();
        sepImg.color = new Color(1f, 1f, 1f, 0.12f);
        var sepRT = sep.GetComponent<RectTransform>();
        sepRT.anchorMin        = new Vector2(0f, 1f);
        sepRT.anchorMax        = new Vector2(1f, 1f);
        sepRT.sizeDelta        = new Vector2(-32f, 2f);
        sepRT.anchoredPosition = new Vector2(0f, -72f);

        // Descrição (ScrollRect para textos longos)
        var scrollGO = new GameObject("ScrollView");
        scrollGO.transform.SetParent(panel.transform, false);
        var scroll = scrollGO.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        var scrollRT = scrollGO.GetComponent<RectTransform>();
        scrollRT.anchorMin = new Vector2(0f, 0f);
        scrollRT.anchorMax = new Vector2(1f, 1f);
        scrollRT.offsetMin = new Vector2(16f, 60f);
        scrollRT.offsetMax = new Vector2(-16f, -80f);

        var viewportGO = new GameObject("Viewport");
        viewportGO.transform.SetParent(scrollGO.transform, false);
        viewportGO.AddComponent<RectTransform>();
        var viewportImg = viewportGO.AddComponent<Image>();
        viewportImg.color = Color.clear;
        viewportGO.AddComponent<Mask>().showMaskGraphic = false;
        var viewRT = viewportGO.GetComponent<RectTransform>();
        viewRT.anchorMin = Vector2.zero;
        viewRT.anchorMax = Vector2.one;
        viewRT.offsetMin = Vector2.zero;
        viewRT.offsetMax = Vector2.zero;

        var contentGO = new GameObject("Content");
        contentGO.transform.SetParent(viewportGO.transform, false);
        contentGO.AddComponent<RectTransform>();
        var contentRT = contentGO.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0f, 1f);
        contentRT.anchorMax = new Vector2(1f, 1f);
        contentRT.offsetMin = Vector2.zero;
        contentRT.offsetMax = Vector2.zero;
        contentRT.pivot     = new Vector2(0.5f, 1f);
        var csf = contentGO.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var descGO = new GameObject("Description");
        descGO.transform.SetParent(contentGO.transform, false);
        var descTMP = descGO.AddComponent<TextMeshProUGUI>();
        descTMP.text      = "";
        descTMP.fontSize  = 18f;
        descTMP.color     = new Color(0.9f, 0.9f, 0.9f, 1f);
        descTMP.alignment = TextAlignmentOptions.TopLeft;
        descTMP.enableWordWrapping = true;
        var descRT = descGO.GetComponent<RectTransform>();
        descRT.anchorMin = Vector2.zero;
        descRT.anchorMax = Vector2.one;
        descRT.offsetMin = Vector2.zero;
        descRT.offsetMax = Vector2.zero;

        scroll.viewport = viewRT;
        scroll.content  = contentRT;

        // Dica "[F] Fechar" no rodapé (substituiu o botão)
        var hintGO = new GameObject("CloseHint");
        hintGO.transform.SetParent(panel.transform, false);
        var hintTMP = hintGO.AddComponent<TextMeshProUGUI>();
        hintTMP.text      = "[F]  Fechar";
        hintTMP.fontSize  = 16f;
        hintTMP.color     = new Color(1f, 1f, 1f, 0.5f);
        hintTMP.alignment = TextAlignmentOptions.Right;
        hintTMP.fontStyle = FontStyles.Italic;
        var hintRT = hintGO.GetComponent<RectTransform>();
        hintRT.anchorMin        = new Vector2(0f, 0f);
        hintRT.anchorMax        = new Vector2(1f, 0f);
        hintRT.sizeDelta        = new Vector2(-32f, 36f);
        hintRT.anchoredPosition = new Vector2(-8f, 20f);

        // ArtifactInfoPanel component
        var aip = canvasGO.AddComponent<ArtifactInfoPanel>();
        aip.panel             = panel;
        aip.titleText         = titleTMP;
        aip.descriptionText   = descTMP;
        aip.closeKey          = KeyCode.F;
        aip.closeButton       = null;
        aip.autoCloseDuration = 0f;

        panel.SetActive(false);
        EditorUtility.SetDirty(canvasGO);
        Debug.Log("[InteractionBuilder] ArtifactInfoPanel criado.");
    }

    // ════════════════════════════════════════════════════════════════════
    // 5. BARRA DE PROGRESSO
    // ════════════════════════════════════════════════════════════════════

    static (GameObject panel, Slider bar, TextMeshProUGUI text)
        BuildProgressBar(UnityEngine.SceneManagement.Scene scene, Color accentColor)
    {
        // Remove antigo
        foreach (var root in scene.GetRootGameObjects())
            if (root.name == "ProgressCanvas") Object.DestroyImmediate(root);

        var canvasGO = new GameObject("ProgressCanvas");
        SceneManager.MoveGameObjectToScene(canvasGO, scene);
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 5;
        canvasGO.AddComponent<CanvasScaler>();

        // Painel canto superior esquerdo
        var panel = new GameObject("ProgressPanel");
        panel.transform.SetParent(canvasGO.transform, false);
        var panelImg = panel.AddComponent<Image>();
        panelImg.color = new Color(0f, 0f, 0f, 0.5f);
        var panelRT = panel.GetComponent<RectTransform>();
        panelRT.anchorMin        = new Vector2(0f, 1f);
        panelRT.anchorMax        = new Vector2(0f, 1f);
        panelRT.sizeDelta        = new Vector2(260f, 56f);
        panelRT.anchoredPosition = new Vector2(140f, -36f);

        // Texto "0 / 3 artefatos explorados"
        var textGO = new GameObject("ProgressText");
        textGO.transform.SetParent(panel.transform, false);
        var tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text      = "0 / 0 artefatos explorados";
        tmp.fontSize  = 14f;
        tmp.color     = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        var tRT = textGO.GetComponent<RectTransform>();
        tRT.anchorMin        = new Vector2(0f, 0.5f);
        tRT.anchorMax        = new Vector2(1f, 1f);
        tRT.offsetMin        = new Vector2(8f, 0f);
        tRT.offsetMax        = new Vector2(-8f, -4f);

        // Slider (barra)
        var sliderGO = new GameObject("ProgressBar");
        sliderGO.transform.SetParent(panel.transform, false);
        sliderGO.AddComponent<RectTransform>();
        var sliderRT = sliderGO.GetComponent<RectTransform>();
        sliderRT.anchorMin        = new Vector2(0f, 0f);
        sliderRT.anchorMax        = new Vector2(1f, 0.5f);
        sliderRT.offsetMin        = new Vector2(8f, 6f);
        sliderRT.offsetMax        = new Vector2(-8f, -2f);

        var slider = sliderGO.AddComponent<Slider>();
        slider.minValue     = 0f;
        slider.maxValue     = 1f;
        slider.value        = 0f;
        slider.wholeNumbers = false;

        // Background do slider
        var bgGO = new GameObject("Background");
        bgGO.transform.SetParent(sliderGO.transform, false);
        bgGO.AddComponent<RectTransform>();
        var bgImg = bgGO.AddComponent<Image>();
        bgImg.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        var bgRT = bgGO.GetComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = Vector2.zero;
        bgRT.offsetMax = Vector2.zero;
        slider.targetGraphic = bgImg;

        // Fill Area
        var fillAreaGO = new GameObject("Fill Area");
        fillAreaGO.transform.SetParent(sliderGO.transform, false);
        fillAreaGO.AddComponent<RectTransform>();
        var fillAreaRT = fillAreaGO.GetComponent<RectTransform>();
        fillAreaRT.anchorMin = Vector2.zero;
        fillAreaRT.anchorMax = Vector2.one;
        fillAreaRT.offsetMin = Vector2.zero;
        fillAreaRT.offsetMax = Vector2.zero;

        var fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(fillAreaGO.transform, false);
        fillGO.AddComponent<RectTransform>();
        var fillImg = fillGO.AddComponent<Image>();
        fillImg.color = accentColor;
        var fillRT = fillGO.GetComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = new Vector2(0f, 1f);
        fillRT.offsetMin = Vector2.zero;
        fillRT.offsetMax = Vector2.zero;
        slider.fillRect = fillRT;

        EditorUtility.SetDirty(canvasGO);
        Debug.Log("[InteractionBuilder] ProgressBar criada.");
        return (panel, slider, tmp);
    }

    // ════════════════════════════════════════════════════════════════════
    // 6. RECONECTA EXHIBITMANAGER
    // ════════════════════════════════════════════════════════════════════

    static void ReconnectExhibitManager(
        UnityEngine.SceneManagement.Scene scene,
        GameObject progressPanel, Slider progressBar, TextMeshProUGUI progressText)
    {
        ExhibitManager em  = null;
        TimelineEra    era = null;

        foreach (var root in scene.GetRootGameObjects())
        {
            if (em  == null) em  = root.GetComponentInChildren<ExhibitManager>(true);
            if (era == null) era = root.GetComponentInChildren<TimelineEra>(true);
        }

        if (em == null)
        {
            // Cria ExhibitManager em um objeto dedicado
            var emGO = new GameObject("ExhibitManager");
            SceneManager.MoveGameObjectToScene(emGO, scene);
            em = emGO.AddComponent<ExhibitManager>();
            Debug.Log("[InteractionBuilder] ExhibitManager criado.");
        }

        // Conecta UI
        em.progressPanel = progressPanel;
        em.progressBar   = progressBar;
        em.progressText  = progressText;
        if (era != null) em.eraController = era;

        // Coleta todos os InteracleObjects da cena
        var exhibits = new List<InteracleObject>();
        foreach (var root in scene.GetRootGameObjects())
            exhibits.AddRange(root.GetComponentsInChildren<InteracleObject>(true));

        em.exhibits = exhibits;

        // Configura layer Interactable em todos os artefatos
        int layer = LayerMask.NameToLayer("Interactable");
        if (layer < 0) layer = 0; // fallback Default

        foreach (var ex in exhibits)
        {
            ex.gameObject.layer = layer;

            // Garante Collider para o raycast funcionar
            if (ex.GetComponent<Collider>() == null)
            {
                ex.gameObject.AddComponent<BoxCollider>();
                Debug.LogWarning($"[InteractionBuilder] BoxCollider adicionado a '{ex.name}'.");
            }

            EditorUtility.SetDirty(ex.gameObject);
        }

        EditorUtility.SetDirty(em);
        Debug.Log($"[InteractionBuilder] ExhibitManager: {exhibits.Count} artefato(s) conectados.");
    }

    // ════════════════════════════════════════════════════════════════════

    static void Prog(string msg, float p) =>
        EditorUtility.DisplayProgressBar("ChronoMundi — Interação", msg, p);
}
#endif