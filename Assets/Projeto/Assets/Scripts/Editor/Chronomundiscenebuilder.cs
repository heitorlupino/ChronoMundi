#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// ChronoMundi — Construtor Completo de Cenas
/// Configura TUDO via código: Player, UI, Exhibits, ExhibitManager,
/// TimelineEra, PauseMenu, SimpleReturnToMuseum e MenuManager.
///
/// Como usar:
///   1. Coloque este arquivo em Assets/Projeto/Assets/Scripts/Editor/
///   2. Menu Unity → ChronoMundi → 🏗️ Construir Cenas Completas
///   3. Aguarde a barra de progresso terminar
///   4. Pronto — todas as 5 cenas estão configuradas
///
/// IMPORTANTE: Execute DEPOIS do "🎨 Popular Assets + Construir Ambientes"
/// </summary>
public static class ChronoMundiSceneBuilder
{
    const string SCENES_PATH = "Assets/Projeto/Assets/Scenes/";

    // ── Cores de UI ──────────────────────────────────────────────────────
    static readonly Color UI_PANEL_BG       = new Color(0.05f, 0.05f, 0.1f, 0.92f);
    static readonly Color UI_PROGRESS_BG    = new Color(0f,    0f,    0f,   0.75f);
    static readonly Color UI_PAUSE_BG       = new Color(0f,    0f,    0f,   0.88f);
    static readonly Color UI_BUTTON_NORMAL  = new Color(0.15f, 0.35f, 0.65f, 1f);
    static readonly Color UI_BUTTON_CONFIRM = new Color(0.15f, 0.55f, 0.25f, 1f);
    static readonly Color UI_BUTTON_DANGER  = new Color(0.65f, 0.15f, 0.15f, 1f);
    static readonly Color UI_WHITE          = Color.white;
    static readonly Color UI_GOLD           = new Color(1f, 0.85f, 0.2f, 1f);
    static readonly Color UI_SUBTITLE_BG    = new Color(0f, 0f, 0f, 0.75f);

    // ════════════════════════════════════════════════════════════════════
    // ENTRY POINT
    // ════════════════════════════════════════════════════════════════════

    [MenuItem("ChronoMundi/🏗️ Construir Cenas Completas")]
    public static void BuildAllScenes()
    {
        if (!EditorUtility.DisplayDialog("ChronoMundi",
            "Isso vai RECONFIGURAR todas as 5 cenas.\n\n" +
            "Execute APENAS se já rodou '🎨 Popular Assets + Construir Ambientes'.\n\n" +
            "Continuar?", "Sim, construir", "Cancelar"))
            return;

        float p = 0f;

        Prog("Configurando MenuScene...", p += 0.15f);
        BuildMenuScene();

        Prog("Configurando MuseuHubScene...", p += 0.15f);
        BuildHubScene();

        Prog("Configurando PreHistoriaScene...", p += 0.20f);
        BuildHistoricalScene("PreHistoriaScene",
            eraName:       "Pré-História",
            introSubtitle: "Bem-vindo à Pré-História.\nExplore os artefatos desta era primitiva.",
            playerPos:     new Vector3(0, 0, -4f),
            exitPos:       new Vector3(0, 1.5f, -5.5f),
            exhibits:      GetPreHistoriaExhibits());

        Prog("Configurando IdadeMediaScene...", p += 0.20f);
        BuildHistoricalScene("IdadeMediaScene",
            eraName:       "Idade Média",
            introSubtitle: "Bem-vindo à Idade Média.\nExplore as relíquias deste período histórico.",
            playerPos:     new Vector3(0, 0, -4f),
            exitPos:       new Vector3(0, 1.5f, -5.5f),
            exhibits:      GetIdadeMediaExhibits());

        Prog("Configurando FuturoTechScene...", p += 0.20f);
        BuildHistoricalScene("FuturoTechScene",
            eraName:       "Futuro Tecnológico",
            introSubtitle: "Bem-vindo ao Futuro.\nExplore as tecnologias que moldarão a humanidade.",
            playerPos:     new Vector3(0, 0.3f, -4.5f),
            exitPos:       new Vector3(0, 1.5f, -5.5f),
            exhibits:      GetFuturoExhibits());

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();

        EditorUtility.DisplayDialog("ChronoMundi ✅",
            "Todas as cenas foram configuradas!\n\n" +
            "• MenuScene: Canvas de menu pronto\n" +
            "• MuseuHubScene: Player + PauseMenu prontos\n" +
            "• 3 cenas históricas: Player, 3 exhibits cada, ExhibitManager, UI de progresso, PauseMenu\n\n" +
            "Próximo passo:\n" +
            "1. Adicione áudio nos InteracleObjects\n" +
            "2. Execute 'ChronoMundi → ✅ Validar Projeto'\n" +
            "3. Configure Build Settings com as 5 cenas",
            "OK");
    }

    // ════════════════════════════════════════════════════════════════════
    // MENU SCENE
    // ════════════════════════════════════════════════════════════════════

    static void BuildMenuScene()
    {
        var scene = OpenScene("MenuScene");

        // ── Câmera ───────────────────────────────────────────────────────
        var cam = EnsureCamera(scene, new Vector3(0, 1.6f, -4f), Vector3.zero);

        // ── Luz direcional ───────────────────────────────────────────────
        EnsureDirectionalLight(scene);

        // ── Canvas principal ─────────────────────────────────────────────
        var canvas = CreateCanvas(scene, "MenuCanvas", 0);

        // Fundo escuro com gradiente
        var bg = CreateUIPanel(canvas, "Background",
            Color.black,
            AnchorPreset.StretchAll, Vector2.zero, Vector2.zero);
        // Adicionar componente Gradient se disponível, ou usar imagem com gradiente
        // Para simplicidade, manter sólido, mas sugerir gradiente manual

        // Painel central do menu
        var menuPanel = CreateUIPanel(canvas, "MenuPanel",
            new Color(0.05f, 0.05f, 0.15f, 0.9f),
            AnchorPreset.MiddleCenter, new Vector2(640, 560), new Vector2(0, 20));

        // Título
        var titleGO = CreateTMPText(menuPanel, "TitleText",
            "CHRONO\nMUNDI",
            font: 56, color: UI_GOLD, bold: true,
            anchor: AnchorPreset.TopCenter,
            sizeDelta: new Vector2(560, 180),
            anchoredPos: new Vector2(0, -30));

        // Linha separadora
        var divider = CreateUIPanel(menuPanel, "Divider",
            new Color(1f, 0.85f, 0.2f, 0.6f),
            AnchorPreset.TopCenter, new Vector2(360, 2), new Vector2(0, -190));

        // Subtítulo
        CreateTMPText(menuPanel, "SubtitleText",
            "Um Museu Além do Tempo",
            font: 18, color: new Color(0.75f, 0.75f, 0.85f), bold: false,
            anchor: AnchorPreset.TopCenter,
            sizeDelta: new Vector2(440, 36),
            anchoredPos: new Vector2(0, -202));

        // Botão Jogar
        var btnPlay = CreateButton(menuPanel, "BtnPlay",
            "JOGAR",
            UI_BUTTON_NORMAL,
            AnchorPreset.TopCenter,
            new Vector2(300, 64), new Vector2(0, -260));

        // Botão Sair
        var btnQuit = CreateButton(menuPanel, "BtnQuit",
            "✕   SAIR",
            UI_BUTTON_DANGER,
            AnchorPreset.TopCenter,
            new Vector2(300, 64), new Vector2(0, -340));

        // Loading indicator (começa inativo)
        var loadingGO = CreateTMPText(canvas, "LoadingIndicator",
            "Carregando...",
            font: 20, color: Color.white, bold: false,
            anchor: AnchorPreset.BottomCenter,
            sizeDelta: new Vector2(300, 40),
            anchoredPos: new Vector2(0, 40));
        loadingGO.SetActive(false);

        // ── GameManager + MenuManager ─────────────────────────────────────
        var gmGO = EnsureGO(scene, "GameManager_Menu");
        var mm = gmGO.GetComponent<MenuManager>();
        if (mm == null) mm = gmGO.AddComponent<MenuManager>();
        mm.hubSceneName    = "MuseuHubScene";
        mm.btnPlay         = btnPlay;
        mm.btnQuit         = btnQuit;
        mm.titleText       = titleGO.GetComponent<TextMeshProUGUI>();
        mm.loadingIndicator = loadingGO;

        SaveScene(scene);
    }

    // ════════════════════════════════════════════════════════════════════
    // HUB SCENE
    // ════════════════════════════════════════════════════════════════════

    static void BuildHubScene()
    {
        var scene = OpenScene("MuseuHubScene");

        EnsureDirectionalLight(scene);

        // ── Player ───────────────────────────────────────────────────────
        var player = BuildDesktopPlayer(scene, new Vector3(0, 0, -5f), "MuseuHubScene_Player");

        // ── PauseMenu ────────────────────────────────────────────────────
        BuildPauseMenu(scene, "MenuScene");

        // ── Portais do museu ─────────────────────────────────────────────
        Debug.Log("[ChronoMundiSceneBuilder] Reconstruindo portais do MuseuHubScene...");
        BuildHubPortals(scene);

        foreach (var go in scene.GetRootGameObjects())
        {
            foreach (var door in go.GetComponentsInChildren<MuseumDoor>(true))
            {
                int layer = LayerMask.NameToLayer("Interactable");
                if (layer >= 0) door.gameObject.layer = layer;
            }
        }

        SaveScene(scene);
    }

    static void BuildHubPortals(Scene scene)
    {
        var parent = EnsureGO(scene, "MuseumPortals");
        parent.transform.position = Vector3.zero;

        BuildHubPortal(scene, parent.transform, "Portal_PreHistoria", new Vector3(-7f, 0, 5f), "Pré-História", "Wall_Cave", "PreHistoriaScene");
        BuildHubPortal(scene, parent.transform, "Portal_IdadeMedia",  new Vector3(0f, 0, 5f),  "Idade Média",  "Wall_Stone", "IdadeMediaScene");
        BuildHubPortal(scene, parent.transform, "Portal_FuturoTech",  new Vector3(7f, 0, 5f),  "Futuro Tech",  "Wall_Tech",  "FuturoTechScene");
    }

    static void BuildHubPortal(Scene scene, Transform parent, string name, Vector3 localPos, string label, string materialName, string destScene)
    {
        foreach (var go in scene.GetRootGameObjects())
            if (go.name == name) Object.DestroyImmediate(go);

        var portalGroup = new GameObject(name);
        SceneManager.MoveGameObjectToScene(portalGroup, scene);
        portalGroup.transform.SetParent(parent, false);
        portalGroup.transform.localPosition = localPos;

        var mat = LoadMaterial(materialName);

        var leftPillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leftPillar.name = "PillarL";
        SceneManager.MoveGameObjectToScene(leftPillar, scene);
        leftPillar.transform.SetParent(portalGroup.transform, false);
        leftPillar.transform.localPosition = new Vector3(-0.85f, 1.5f, 0);
        leftPillar.transform.localScale = new Vector3(0.3f, 3f, 0.3f);
        if (mat != null) leftPillar.GetComponent<Renderer>().sharedMaterial = mat;

        var rightPillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rightPillar.name = "PillarR";
        SceneManager.MoveGameObjectToScene(rightPillar, scene);
        rightPillar.transform.SetParent(portalGroup.transform, false);
        rightPillar.transform.localPosition = new Vector3(0.85f, 1.5f, 0);
        rightPillar.transform.localScale = new Vector3(0.3f, 3f, 0.3f);
        if (mat != null) rightPillar.GetComponent<Renderer>().sharedMaterial = mat;

        var arc = GameObject.CreatePrimitive(PrimitiveType.Cube);
        arc.name = "Arc";
        SceneManager.MoveGameObjectToScene(arc, scene);
        arc.transform.SetParent(portalGroup.transform, false);
        arc.transform.localPosition = new Vector3(0, 3.15f, 0);
        arc.transform.localScale = new Vector3(2f, 0.3f, 0.3f);
        if (mat != null) arc.GetComponent<Renderer>().sharedMaterial = mat;

        var portal = GameObject.CreatePrimitive(PrimitiveType.Cube);
        portal.name = "Portal";
        SceneManager.MoveGameObjectToScene(portal, scene);
        portal.transform.SetParent(portalGroup.transform, false);
        portal.transform.localPosition = new Vector3(0, 1.5f, 0);
        portal.transform.localScale = new Vector3(1.4f, 3f, 0.1f);
        var portalCollider = portal.GetComponent<Collider>();
        if (portalCollider != null)
            portalCollider.isTrigger = true;

        int layer = LayerMask.NameToLayer("Interactable");
        portal.layer = layer >= 0 ? layer : 0;

        var door = portal.AddComponent<MuseumDoor>();
        door.nomeScene = destScene;

        var labelGO = new GameObject("Label");
        labelGO.transform.SetParent(portalGroup.transform, false);
        labelGO.transform.localPosition = new Vector3(0, 3.6f, 0);
        labelGO.transform.localScale = Vector3.one * 0.012f;
        var cvs = labelGO.AddComponent<Canvas>();
        cvs.renderMode = RenderMode.WorldSpace;
        labelGO.AddComponent<CanvasScaler>();
        var tmp = labelGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 80;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.fontStyle = FontStyles.Bold;
        var rt = labelGO.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(500, 100);
    }

    static Material LoadMaterial(string name)
    {
        return AssetDatabase.LoadAssetAtPath<Material>($"Assets/Projeto/Assets/Materials/Environment/{name}.mat");
    }

    // ════════════════════════════════════════════════════════════════════
    // HISTORICAL SCENE (genérico para as 3 eras)
    // ════════════════════════════════════════════════════════════════════

    static void BuildHistoricalScene(
        string sceneName,
        string eraName,
        string introSubtitle,
        Vector3 playerPos,
        Vector3 exitPos,
        List<ExhibitData> exhibits)
    {
        var scene = OpenScene(sceneName);

        EnsureDirectionalLight(scene);

        // ── Player ───────────────────────────────────────────────────────
        var player = BuildDesktopPlayer(scene, playerPos, $"{sceneName}_Player");
        var dpc = player.GetComponent<DesktopPlayerController>();

        // ── Artefatos (Exhibits) ──────────────────────────────────────────
        var exhibitObjects = new List<InteracleObject>();
        foreach (var data in exhibits)
        {
            var exhibitGO = BuildExhibit(scene, data);
            exhibitObjects.Add(exhibitGO.GetComponent<InteracleObject>());
        }

        // ── UI de Progresso ───────────────────────────────────────────────
        var (progressPanel, progressText, progressBar) = BuildProgressUI(scene);

        // ── ExhibitManager ────────────────────────────────────────────────
        var emGO = EnsureGO(scene, "ExhibitManager");
        var em = emGO.GetComponent<ExhibitManager>();
        if (em == null) em = emGO.AddComponent<ExhibitManager>();
        em.exhibits        = exhibitObjects;
        em.progressPanel   = progressPanel;
        em.progressText    = progressText;
        em.progressBar     = progressBar;
        em.interactedColor = new Color(0.4f, 1f, 0.4f);

        // Tenta pegar o highlight material criado pelo AssetLinker
        var hlMat = AssetDatabase.LoadAssetAtPath<Material>(
            "Assets/Projeto/Assets/Materials/Highlight_Interactable.mat");
        if (hlMat != null) em.highlightMaterial = hlMat;

        // ── SimpleReturnToMuseum ──────────────────────────────────────────
        var returnGO = BuildReturnPoint(scene, exitPos);
        var srtm = returnGO.GetComponent<SimpleReturnToMuseum>();

        // ── TimelineEra ───────────────────────────────────────────────────
        var eraGO = EnsureGO(scene, "TimelineEra");
        var era = eraGO.GetComponent<TimelineEra>();
        if (era == null) era = eraGO.AddComponent<TimelineEra>();
        era.eraName             = eraName;
        era.introSubtitle       = introSubtitle;
        era.delayBeforeNarration = 1.2f;
        era.playerController    = dpc;
        era.returnPoint         = srtm;

        // Conecta TimelineEra ao ExhibitManager
        em.eraController = era;

        // ── PauseMenu ────────────────────────────────────────────────────
        BuildPauseMenu(scene, "MenuScene");

        SaveScene(scene);
    }

    // ════════════════════════════════════════════════════════════════════
    // BUILDERS DE COMPONENTES
    // ════════════════════════════════════════════════════════════════════

    // ── Desktop Player ────────────────────────────────────────────────
    static GameObject BuildDesktopPlayer(Scene scene, Vector3 pos, string name)
    {
        // Remove player antigo se existir
        foreach (var go in scene.GetRootGameObjects())
            if (go.name == name) Object.DestroyImmediate(go);

        var playerGO = new GameObject(name);
        SceneManager.MoveGameObjectToScene(playerGO, scene);
        playerGO.transform.position = pos;
        playerGO.tag = "Player";

        // CharacterController
        var cc = playerGO.AddComponent<CharacterController>();
        cc.center = new Vector3(0, 0.9f, 0);
        cc.height = 1.8f;
        cc.radius = 0.3f;

        // DesktopPlayerController
        var dpc = playerGO.AddComponent<DesktopPlayerController>();
        dpc.moveSpeed       = 4f;
        dpc.sprintSpeed     = 7f;
        dpc.mouseSensitivity = 2f;
        dpc.enabledOnStart  = true;

        // Camera (filho)
        var camGO = new GameObject("PlayerCamera");
        camGO.transform.SetParent(playerGO.transform);
        camGO.transform.localPosition = new Vector3(0, 1.6f, 0);
        camGO.transform.localRotation = Quaternion.identity;
        var cam = camGO.AddComponent<Camera>();
        cam.fieldOfView = 75f;
        cam.nearClipPlane = 0.05f;
        camGO.AddComponent<AudioListener>();

        // Conecta câmera ao controller
        dpc.cameraTransform = camGO.transform;

        // PlayerInteraction
        var pi = playerGO.AddComponent<PlayerInteraction>();
        pi.interactionDistance = 3f;
        pi.isVRMode = false;
        pi.interactionKey = KeyCode.E;

        int interLayer = LayerMask.NameToLayer("Interactable");
        if (interLayer >= 0)
            pi.interactableLayer = 1 << interLayer;

        // Canvas de prompt de interação (filho da câmera para seguir a tela)
        var promptCanvasGO = new GameObject("PromptCanvas");
        promptCanvasGO.transform.SetParent(camGO.transform);
        var promptCanvas = promptCanvasGO.AddComponent<Canvas>();
        promptCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var promptPanelGO = new GameObject("InteractionPrompt");
        promptPanelGO.transform.SetParent(promptCanvasGO.transform, false);
        var promptPanelImg = promptPanelGO.AddComponent<Image>();
        promptPanelImg.color = new Color(0, 0, 0, 0.65f);
        var promptPanelRT = promptPanelGO.GetComponent<RectTransform>();
        promptPanelRT.anchorMin = new Vector2(0.3f, 0f);
        promptPanelRT.anchorMax = new Vector2(0.7f, 0f);
        promptPanelRT.anchoredPosition = new Vector2(0, 80f);
        promptPanelRT.sizeDelta = new Vector2(0, 60f);

        var promptTextGO = new GameObject("PromptText");
        promptTextGO.transform.SetParent(promptPanelGO.transform, false);
        var promptTMP = promptTextGO.AddComponent<TextMeshProUGUI>();
        promptTMP.text = "Pressione [E] para interagir";
        promptTMP.fontSize = 20;
        promptTMP.alignment = TextAlignmentOptions.Center;
        promptTMP.color = UI_WHITE;
        var promptRT = promptTextGO.GetComponent<RectTransform>();
        promptRT.anchorMin = Vector2.zero;
        promptRT.anchorMax = Vector2.one;
        promptRT.offsetMin = new Vector2(12, 6);
        promptRT.offsetMax = new Vector2(-12, -6);

        promptPanelGO.SetActive(false);

        // Conecta ao PlayerInteraction
        pi.interactionPrompt = promptPanelGO;
        pi.promptText = promptTMP;

        return playerGO;
    }

    // ── Exhibit (artefato) ────────────────────────────────────────────
    static GameObject BuildExhibit(Scene scene, ExhibitData data)
    {
        // Remove se já existir
        foreach (var go in scene.GetRootGameObjects())
            if (go.name == data.goName) Object.DestroyImmediate(go);

        GameObject exhibitGO;
        switch (data.shape)
        {
            case PrimitiveType.Sphere:   exhibitGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);   break;
            case PrimitiveType.Cylinder: exhibitGO = GameObject.CreatePrimitive(PrimitiveType.Cylinder); break;
            case PrimitiveType.Capsule:  exhibitGO = GameObject.CreatePrimitive(PrimitiveType.Capsule);  break;
            default:                     exhibitGO = GameObject.CreatePrimitive(PrimitiveType.Cube);     break;
        }

        exhibitGO.name = data.goName;
        SceneManager.MoveGameObjectToScene(exhibitGO, scene);
        exhibitGO.transform.position   = data.position;
        exhibitGO.transform.localScale = data.scale;

        // Layer
        int layer = LayerMask.NameToLayer("Interactable");
        if (layer >= 0) exhibitGO.layer = layer;

        // Material
        var mat = AssetDatabase.LoadAssetAtPath<Material>(
            $"Assets/Projeto/Assets/Materials/Environment/{data.materialName}.mat");
        if (mat != null)
            exhibitGO.GetComponent<Renderer>().sharedMaterial = mat;

        // Collider já existe (CreatePrimitive adiciona automaticamente)

        // InteracleObject
        var io = exhibitGO.AddComponent<InteracleObject>();
        io.artifactName        = data.artifactName;
        io.artifactDescription = data.description;
        io.subtitleText        = data.subtitle;
        io.interactOnlyOnce    = true;

        // HighlightController
        var hc = exhibitGO.AddComponent<HighlightController>();
        hc.emissionColor  = data.glowColor;
        hc.pulseSpeed     = data.pulseSpeed;
        hc.minIntensity   = 0.05f;
        hc.maxIntensity   = 1.8f;
        hc.stopOnInteract = true;

        // Pedestal (base) abaixo do artefato
        var pedestal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pedestal.name = $"{data.goName}_Pedestal";
        SceneManager.MoveGameObjectToScene(pedestal, scene);
        pedestal.transform.position   = data.position + new Vector3(0, -data.scale.y * 0.5f - 0.3f, 0);
        pedestal.transform.localScale = new Vector3(0.4f, 0.3f, 0.4f);
        Object.DestroyImmediate(pedestal.GetComponent<Collider>());
        var pedestalMat = AssetDatabase.LoadAssetAtPath<Material>(
            "Assets/Projeto/Assets/Materials/Environment/Pillar_Stone.mat");
        if (pedestalMat != null)
            pedestal.GetComponent<Renderer>().sharedMaterial = pedestalMat;

        return exhibitGO;
    }

    // ── UI de Progresso ───────────────────────────────────────────────
    static (GameObject panel, TextMeshProUGUI text, Slider slider) BuildProgressUI(Scene scene)
    {
        // Remove se existir
        foreach (var go in scene.GetRootGameObjects())
            if (go.name == "ProgressCanvas") Object.DestroyImmediate(go);

        var canvasGO = new GameObject("ProgressCanvas");
        SceneManager.MoveGameObjectToScene(canvasGO, scene);
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 5;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // Painel de progresso (canto superior esquerdo)
        var panelGO = new GameObject("ProgressPanel");
        panelGO.transform.SetParent(canvasGO.transform, false);
        var panelImg = panelGO.AddComponent<Image>();
        panelImg.color = UI_PROGRESS_BG;
        var panelRT = panelGO.GetComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0, 1);
        panelRT.anchorMax = new Vector2(0, 1);
        panelRT.pivot     = new Vector2(0, 1);
        panelRT.anchoredPosition = new Vector2(16, -16);
        panelRT.sizeDelta = new Vector2(280, 72);

        // Ícone e texto de progresso
        var iconGO = new GameObject("ProgressIcon");
        iconGO.transform.SetParent(panelGO.transform, false);
        var iconImg = iconGO.AddComponent<Image>();
        iconImg.color = UI_GOLD; // Ícone dourado
        var iconRT = iconGO.GetComponent<RectTransform>();
        iconRT.anchorMin = new Vector2(0, 0.45f);
        iconRT.anchorMax = new Vector2(0, 0.45f);
        iconRT.pivot = new Vector2(0, 0.5f);
        iconRT.anchoredPosition = new Vector2(12, 0);
        iconRT.sizeDelta = new Vector2(20, 20);

        var textGO = new GameObject("ProgressText");
        textGO.transform.SetParent(panelGO.transform, false);
        var textTMP = textGO.AddComponent<TextMeshProUGUI>();
        textTMP.text      = "0 / 3 artefatos explorados";
        textTMP.fontSize  = 17;
        textTMP.color     = UI_WHITE;
        textTMP.alignment = TextAlignmentOptions.MidlineLeft;
        var textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = new Vector2(0, 0.45f);
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = new Vector2(40, 0); // Ajuste para ícone
        textRT.offsetMax = new Vector2(-12, -8);

        // Barra de progresso
        var sliderGO = new GameObject("ProgressBar");
        sliderGO.transform.SetParent(panelGO.transform, false);
        var slider = sliderGO.AddComponent<Slider>();
        slider.minValue = 0;
        slider.maxValue = 1;
        slider.value    = 0;
        slider.wholeNumbers = false;
        var sliderRT = sliderGO.GetComponent<RectTransform>();
        sliderRT.anchorMin = new Vector2(0, 0);
        sliderRT.anchorMax = new Vector2(1, 0.44f);
        sliderRT.offsetMin = new Vector2(12, 10);
        sliderRT.offsetMax = new Vector2(-12, 0);

        // Background do slider
        var bgGO = new GameObject("Background");
        bgGO.transform.SetParent(sliderGO.transform, false);
        var bgImg = bgGO.AddComponent<Image>();
        bgImg.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        var bgRT = bgGO.GetComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero; bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = Vector2.zero; bgRT.offsetMax = Vector2.zero;
        slider.targetGraphic = bgImg;

        // Fill area
        var fillAreaGO = new GameObject("Fill Area");
        fillAreaGO.transform.SetParent(sliderGO.transform, false);
        var fillAreaRT = fillAreaGO.GetComponent<RectTransform>();
        if (fillAreaRT == null) fillAreaRT = fillAreaGO.AddComponent<RectTransform>();
        fillAreaRT.anchorMin = Vector2.zero; fillAreaRT.anchorMax = Vector2.one;
        fillAreaRT.offsetMin = new Vector2(2, 2); fillAreaRT.offsetMax = new Vector2(-2, -2);

        var fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(fillAreaGO.transform, false);
        var fillImg = fillGO.AddComponent<Image>();
        fillImg.color = new Color(0.2f, 0.85f, 0.4f, 1f);
        var fillRT = fillGO.GetComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero; fillRT.anchorMax = Vector2.one;
        fillRT.offsetMin = Vector2.zero; fillRT.offsetMax = Vector2.zero;
        slider.fillRect = fillRT;

        return (panelGO, textTMP, slider);
    }

    // ── Return Point (saída da cena) ──────────────────────────────────
    static GameObject BuildReturnPoint(Scene scene, Vector3 pos)
    {
        foreach (var go in scene.GetRootGameObjects())
            if (go.name == "SaidaMuseu") Object.DestroyImmediate(go);

        var go2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go2.name = "SaidaMuseu";
        SceneManager.MoveGameObjectToScene(go2, scene);
        go2.transform.position   = pos;
        go2.transform.localScale = new Vector3(1.8f, 3f, 0.25f);

        int layer = LayerMask.NameToLayer("Interactable");
        if (layer >= 0) go2.layer = layer;

        // Material de porta de saída
        var mat = AssetDatabase.LoadAssetAtPath<Material>(
            "Assets/Projeto/Assets/Materials/Environment/Door_Wood.mat");
        if (mat != null) go2.GetComponent<Renderer>().sharedMaterial = mat;

        // SimpleReturnToMuseum
        var srtm = go2.AddComponent<SimpleReturnToMuseum>();
        srtm.museumSceneName  = "MuseuHubScene";
        srtm.activationMode   = SimpleReturnToMuseum.ActivationMode.Interactable;
        srtm.exitSubtitleText = "Retornando ao Museu...";
        srtm.askConfirmation  = false;

        // HighlightController (será ativado ao completar todos os exhibits)
        var hc = go2.AddComponent<HighlightController>();
        hc.emissionColor  = new Color(0.2f, 1f, 0.5f);
        hc.pulseSpeed     = 1.5f;
        hc.stopOnInteract = false;
        hc.enabled        = false; // começa desativado — TimelineEra ativa quando completo

        // Texto flutuante "SAÍDA"
        var labelGO = new GameObject("ExitLabel");
        labelGO.transform.SetParent(go2.transform, false);
        labelGO.transform.localPosition  = new Vector3(0, 1.8f, 0);
        labelGO.transform.localScale     = Vector3.one * 0.016f;
        var cvs = labelGO.AddComponent<Canvas>();
        cvs.renderMode = RenderMode.WorldSpace;
        labelGO.AddComponent<CanvasScaler>();
        var lbl = labelGO.AddComponent<TextMeshProUGUI>();
        lbl.text      = "[ E ] SAIR";
        lbl.fontSize  = 80;
        lbl.alignment = TextAlignmentOptions.Center;
        lbl.color     = new Color(0.8f, 1f, 0.8f);
        lbl.fontStyle = FontStyles.Bold;
        var labelRT = labelGO.GetComponent<RectTransform>();
        labelRT.sizeDelta = new Vector2(500, 120);

        return go2;
    }

    // ── Pause Menu ────────────────────────────────────────────────────
    static void BuildPauseMenu(Scene scene, string menuSceneName)
    {
        foreach (var go in scene.GetRootGameObjects())
            if (go.name == "PauseCanvas" || go.name == "PauseManager") Object.DestroyImmediate(go);

        var canvas = CreateCanvas(scene, "PauseCanvas", 30);

        // Overlay escuro
        CreateUIPanel(canvas, "Overlay",
            new Color(0, 0, 0, 0.55f), AnchorPreset.StretchAll, Vector2.zero, Vector2.zero);

        // Painel central
        var panel = CreateUIPanel(canvas, "PausePanel",
            UI_PAUSE_BG, AnchorPreset.MiddleCenter, new Vector2(380, 340), Vector2.zero);

        // Título
        CreateTMPText(panel, "TitleText", "⏸ PAUSADO",
            font: 34, color: UI_WHITE, bold: true,
            anchor: AnchorPreset.TopCenter,
            sizeDelta: new Vector2(340, 60), anchoredPos: new Vector2(0, -30));

        // Linha
        CreateUIPanel(panel, "Divider",
            new Color(1, 1, 1, 0.2f), AnchorPreset.TopCenter, new Vector2(320, 1), new Vector2(0, -95));

        // Botões
        var btnResume = CreateButton(panel, "BtnResume", "▶   Continuar",
            UI_BUTTON_NORMAL, AnchorPreset.TopCenter, new Vector2(300, 58), new Vector2(0, -120));
        var btnMenu = CreateButton(panel, "BtnMenu", "🏠   Menu Principal",
            new Color(0.35f, 0.35f, 0.45f, 1f), AnchorPreset.TopCenter, new Vector2(300, 58), new Vector2(0, -192));
        var btnQuit = CreateButton(panel, "BtnQuit", "✕   Sair do Jogo",
            UI_BUTTON_DANGER, AnchorPreset.TopCenter, new Vector2(300, 58), new Vector2(0, -264));

        panel.SetActive(false);

        // PauseManager
        var pmGO = EnsureGO(scene, "PauseManager");
        var pm = pmGO.GetComponent<PauseMenu>();
        if (pm == null) pm = pmGO.AddComponent<PauseMenu>();
        pm.pausePanel    = panel;
        pm.btnResume     = btnResume;
        pm.btnMenu       = btnMenu;
        pm.btnQuit       = btnQuit;
        pm.menuSceneName = menuSceneName;
        pm.pausedTimeScale = 0f;
    }

    // ════════════════════════════════════════════════════════════════════
    // EXHIBIT DATA (conteúdo dos artefatos por era)
    // ════════════════════════════════════════════════════════════════════

    static List<ExhibitData> GetPreHistoriaExhibits() => new List<ExhibitData>
    {
        new ExhibitData {
            goName       = "Artefato_Machado",
            artifactName = "Machado de Pedra",
            description  = "Ferramenta usada pelos primeiros humanos há 2,5 milhões de anos. " +
                           "Feita lascando pedras para criar gumes afiados, foi revolucionária " +
                           "para a caça e o corte de alimentos.",
            subtitle     = "Machado de pedra — 2,5 milhões de anos",
            position     = new Vector3(2f,  0.9f,  0f),
            scale        = new Vector3(0.28f, 0.06f, 0.16f),
            shape        = PrimitiveType.Cube,
            materialName = "Artifact_Stone",
            glowColor    = new Color(1f, 0.5f, 0.1f),
            pulseSpeed   = 2f
        },
        new ExhibitData {
            goName       = "Artefato_Pintura",
            artifactName = "Pintura Rupestre",
            description  = "Expressões artísticas feitas com pigmentos minerais há cerca de " +
                           "40.000 anos. Representavam animais, caçadas e rituais sagrados — " +
                           "a primeira forma de comunicação visual da humanidade.",
            subtitle     = "Arte nas pedras — os primeiros registros humanos",
            position     = new Vector3(-2f, 1.1f,  2.5f),
            scale        = new Vector3(0.8f, 0.6f, 0.06f),
            shape        = PrimitiveType.Cube,
            materialName = "Artifact_Bone",
            glowColor    = new Color(1f, 0.3f, 0f),
            pulseSpeed   = 1.5f
        },
        new ExhibitData {
            goName       = "Artefato_Osso",
            artifactName = "Osso Numerado",
            description  = "O osso de Ishango, datado de 25.000 anos, é considerado o primeiro " +
                           "objeto matemático da história. As marcações revelam noções primitivas " +
                           "de contagem e aritmética.",
            subtitle     = "Os primeiros números da humanidade",
            position     = new Vector3(0f, 1.0f,  3.5f),
            scale        = new Vector3(0.1f, 0.45f, 0.1f),
            shape        = PrimitiveType.Cylinder,
            materialName = "Artifact_Bone",
            glowColor    = new Color(0.9f, 0.8f, 0.5f),
            pulseSpeed   = 2.2f
        }
    };

    static List<ExhibitData> GetIdadeMediaExhibits() => new List<ExhibitData>
    {
        new ExhibitData {
            goName       = "Artefato_Pergaminho",
            artifactName = "Pergaminho Iluminado",
            description  = "Manuscritos produzidos à mão por monges medievais entre os séculos V e XV. " +
                           "Preservaram o conhecimento clássico e eram obras de arte riquíssimas em detalhes.",
            subtitle     = "Os livros da Idade Média — feitos à mão por anos",
            position     = new Vector3(0f, 1.15f, 4.5f),
            scale        = new Vector3(0.38f, 0.03f, 0.28f),
            shape        = PrimitiveType.Cube,
            materialName = "Artifact_Bone",
            glowColor    = new Color(1f, 0.85f, 0.1f),
            pulseSpeed   = 1.8f
        },
        new ExhibitData {
            goName       = "Artefato_Escudo",
            artifactName = "Escudo Heráldico",
            description  = "Os brasões medievais eram identidades visuais de famílias nobres. " +
                           "Cada símbolo, cor e animal tinha um significado preciso, registrado " +
                           "pelos heráldicos da época.",
            subtitle     = "Identidade e honra representadas em metal",
            position     = new Vector3(-3f, 1.3f, 2f),
            scale        = new Vector3(0.55f, 0.7f, 0.09f),
            shape        = PrimitiveType.Cube,
            materialName = "Artifact_Metal",
            glowColor    = new Color(1f, 0.1f, 0.1f),
            pulseSpeed   = 2f
        },
        new ExhibitData {
            goName       = "Artefato_Espada",
            artifactName = "Espada de Cavaleiro",
            description  = "A espada era o símbolo máximo do cavaleiro medieval. Forjada por " +
                           "ferreiros especializados, levava semanas para ser produzida e era " +
                           "passada de geração em geração como relíquia.",
            subtitle     = "Símbolo de poder e nobreza da Idade Média",
            position     = new Vector3(3f, 1.2f, 2f),
            scale        = new Vector3(0.08f, 0.75f, 0.06f),
            shape        = PrimitiveType.Cube,
            materialName = "Artifact_Metal",
            glowColor    = new Color(0.7f, 0.8f, 1f),
            pulseSpeed   = 2.4f
        }
    };

    static List<ExhibitData> GetFuturoExhibits() => new List<ExhibitData>
    {
        new ExhibitData {
            goName       = "Artefato_Holograma",
            artifactName = "Interface Holográfica",
            description  = "Tecnologia de projeção tridimensional sem superfície física. " +
                           "Baseada em modulação de feixes de laser e partículas de ar ionizado, " +
                           "tornará obsoletos monitores e telas físicas.",
            subtitle     = "O futuro das interfaces humano-máquina",
            position     = new Vector3(0f, 0.85f, 0.5f),
            scale        = new Vector3(0.5f, 0.75f, 0.5f),
            shape        = PrimitiveType.Sphere,
            materialName = "Artifact_Glow",
            glowColor    = new Color(0.1f, 1f, 0.9f),
            pulseSpeed   = 3f
        },
        new ExhibitData {
            goName       = "Artefato_Chip",
            artifactName = "Processador Quântico",
            description  = "Computadores quânticos utilizam qubits em superposição de estados. " +
                           "São capazes de resolver em segundos problemas que levariam milênios " +
                           "para supercomputadores clássicos.",
            subtitle     = "Computação quântica — além dos limites do silício",
            position     = new Vector3(-5f, 1.45f, 3f),
            scale        = new Vector3(0.2f, 0.03f, 0.2f),
            shape        = PrimitiveType.Cube,
            materialName = "Artifact_Metal",
            glowColor    = new Color(0.6f, 0.2f, 1f),
            pulseSpeed   = 2.8f
        },
        new ExhibitData {
            goName       = "Artefato_Exo",
            artifactName = "Exoesqueleto Neural",
            description  = "Trajes robóticos controlados por sinais cerebrais. Permitem que " +
                           "pessoas com paralisia recuperem mobilidade e que trabalhadores " +
                           "realizem tarefas físicas com força multiplicada por 10.",
            subtitle     = "A fusão entre humano e máquina",
            position     = new Vector3(5f, 1.2f, -2.5f),
            scale        = new Vector3(0.35f, 1.1f, 0.2f),
            shape        = PrimitiveType.Capsule,
            materialName = "Artifact_Metal",
            glowColor    = new Color(0.2f, 1f, 0.4f),
            pulseSpeed   = 2f
        }
    };

    // ════════════════════════════════════════════════════════════════════
    // UI HELPERS
    // ════════════════════════════════════════════════════════════════════

    enum AnchorPreset { MiddleCenter, TopCenter, BottomCenter, StretchAll, TopLeft }

    static GameObject CreateCanvas(Scene scene, string name, int sortOrder)
    {
        var go = EnsureGO(scene, name);
        var canvas = go.GetComponent<Canvas>();
        if (canvas == null) canvas = go.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortOrder;
        if (!go.GetComponent<CanvasScaler>())
        {
            var cs = go.AddComponent<CanvasScaler>();
            cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cs.referenceResolution = new Vector2(1920, 1080);
            cs.matchWidthOrHeight = 0.5f;
        }
        if (!go.GetComponent<GraphicRaycaster>())
            go.AddComponent<GraphicRaycaster>();
        return go;
    }

    static GameObject CreateUIPanel(GameObject parent, string name, Color color,
        AnchorPreset anchor, Vector2 sizeDelta, Vector2 anchoredPos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var img = go.AddComponent<Image>();
        img.color = color;
        var rt = go.GetComponent<RectTransform>();
        ApplyAnchor(rt, anchor);
        rt.sizeDelta     = sizeDelta;
        rt.anchoredPosition = anchoredPos;
        return go;
    }

    static Button CreateButton(GameObject parent, string name, string label,
        Color bgColor, AnchorPreset anchor, Vector2 sizeDelta, Vector2 anchoredPos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var img = go.AddComponent<Image>();
        img.color = bgColor;
        var btn = go.AddComponent<Button>();
        var rt  = go.GetComponent<RectTransform>();
        ApplyAnchor(rt, anchor);
        rt.sizeDelta        = sizeDelta;
        rt.anchoredPosition = anchoredPos;

        // Texto do botão
        var txtGO = new GameObject("Text");
        txtGO.transform.SetParent(go.transform, false);
        var tmp = txtGO.AddComponent<TextMeshProUGUI>();
        tmp.text      = label;
        tmp.fontSize  = 22;
        tmp.color     = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        var txtRT = txtGO.GetComponent<RectTransform>();
        txtRT.anchorMin = Vector2.zero;
        txtRT.anchorMax = Vector2.one;
        txtRT.offsetMin = Vector2.zero;
        txtRT.offsetMax = Vector2.zero;

        btn.targetGraphic = img;
        return btn;
    }

    static GameObject CreateTMPText(GameObject parent, string name, string text,
        float font, Color color, bool bold, AnchorPreset anchor,
        Vector2 sizeDelta, Vector2 anchoredPos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = font;
        tmp.color     = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableWordWrapping = true;
        if (bold) tmp.fontStyle = FontStyles.Bold;
        var rt = go.GetComponent<RectTransform>();
        ApplyAnchor(rt, anchor);
        rt.sizeDelta        = sizeDelta;
        rt.anchoredPosition = anchoredPos;
        return go;
    }

    static void ApplyAnchor(RectTransform rt, AnchorPreset p)
    {
        switch (p)
        {
            case AnchorPreset.MiddleCenter:
                rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot     = new Vector2(0.5f, 0.5f); break;
            case AnchorPreset.TopCenter:
                rt.anchorMin = new Vector2(0.5f, 1f); rt.anchorMax = new Vector2(0.5f, 1f);
                rt.pivot     = new Vector2(0.5f, 1f); break;
            case AnchorPreset.BottomCenter:
                rt.anchorMin = new Vector2(0.5f, 0f); rt.anchorMax = new Vector2(0.5f, 0f);
                rt.pivot     = new Vector2(0.5f, 0f); break;
            case AnchorPreset.StretchAll:
                rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
                rt.pivot     = new Vector2(0.5f, 0.5f);
                rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero; break;
            case AnchorPreset.TopLeft:
                rt.anchorMin = new Vector2(0f, 1f); rt.anchorMax = new Vector2(0f, 1f);
                rt.pivot     = new Vector2(0f, 1f); break;
        }
    }

    // ════════════════════════════════════════════════════════════════════
    // SCENE / GO HELPERS
    // ════════════════════════════════════════════════════════════════════

    static Scene OpenScene(string name) =>
        EditorSceneManager.OpenScene($"{SCENES_PATH}{name}.unity", OpenSceneMode.Single);

    static void SaveScene(Scene scene)
    {
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    static Camera EnsureCamera(Scene scene, Vector3 pos, Vector3 rot)
    {
        foreach (var go in scene.GetRootGameObjects())
        {
            var c = go.GetComponentInChildren<Camera>(true);
            if (c != null) { c.transform.position = pos; return c; }
        }
        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        SceneManager.MoveGameObjectToScene(camGO, scene);
        camGO.transform.position = pos;
        camGO.transform.eulerAngles = rot;
        camGO.AddComponent<AudioListener>();
        return camGO.AddComponent<Camera>();
    }

    static void EnsureDirectionalLight(Scene scene)
    {
        foreach (var go in scene.GetRootGameObjects())
        {
            var light = go.GetComponent<Light>();
            if (light != null && light.type == LightType.Directional) return;
        }

        var lightGO = new GameObject("Directional Light");
        SceneManager.MoveGameObjectToScene(lightGO, scene);
        lightGO.transform.eulerAngles = new Vector3(50f, -30f, 0f);
        var l = lightGO.AddComponent<Light>();
        l.type      = LightType.Directional;
        l.intensity = 1f;
        l.color     = new Color(1f, 0.97f, 0.88f);
    }

    static GameObject EnsureGO(Scene scene, string name)
    {
        foreach (var go in scene.GetRootGameObjects())
            if (go.name == name) return go;

        var newGO = new GameObject(name);
        SceneManager.MoveGameObjectToScene(newGO, scene);
        return newGO;
    }

    static void Prog(string msg, float p) =>
        EditorUtility.DisplayProgressBar("ChronoMundi — Construindo Cenas", msg, p);

    // ════════════════════════════════════════════════════════════════════
    // EXHIBIT DATA STRUCT
    // ════════════════════════════════════════════════════════════════════

    struct ExhibitData
    {
        public string        goName;
        public string        artifactName;
        public string        description;
        public string        subtitle;
        public Vector3       position;
        public Vector3       scale;
        public PrimitiveType shape;
        public string        materialName;
        public Color         glowColor;
        public float         pulseSpeed;
    }
}
#endif