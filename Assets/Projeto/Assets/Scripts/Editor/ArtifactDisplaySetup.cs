#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Varre os artefatos REAIS presentes em cada cena histórica,
/// lê o ArtifactDescription atual de cada um e configura o sistema
/// de exibição para que o texto apareça quando o usuário interagir.
///
/// Não altera nenhum texto existente nos artefatos.
///
/// Menu: ChronoMundi → 🔍 Conectar Artefatos ao Sistema de Exibição
/// </summary>
public static class ArtifactDisplaySetup
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
    [MenuItem("ChronoMundi/🔍 Conectar Artefatos ao Sistema de Exibição")]
    public static void Run()
    {
        if (!EditorUtility.DisplayDialog("ChronoMundi — Conectar Artefatos",
            "Este script vai:\n\n" +
            "1. Varrer TODOS os artefatos de cada cena\n" +
            "2. Ler o ArtifactDescription atual de cada um\n" +
            "3. Criar o painel de texto (ArtifactInfoPanel)\n" +
            "4. Criar o sistema de legenda (NarratorSystem)\n" +
            "5. Conectar ExhibitManager aos artefatos reais\n\n" +
            "Nenhum texto será alterado.",
            "Conectar", "Cancelar"))
            return;

        string report = "";
        float p = 0f;

        foreach (var sceneName in SCENES)
        {
            p += 0.3f;
            Prog($"Varrendo {sceneName}...", p);
            var result = ProcessScene(sceneName);
            report += $"\n{sceneName}: {result} artefato(s)";
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();

        EditorUtility.DisplayDialog("ChronoMundi ✅ Conectado",
            "Sistema de exibição conectado!\n" + report +
            "\n\nAgora ao pressionar [E] em qualquer artefato:\n" +
            "• Legenda aparece embaixo da tela\n" +
            "• Painel com nome + descrição abre no centro\n" +
            "• Pressione [F] para fechar o painel",
            "OK");
    }

    // ════════════════════════════════════════════════════════════════════

    static int ProcessScene(string sceneName)
    {
        var scene = EditorSceneManager.OpenScene(
            SCENES_PATH + sceneName + ".unity", OpenSceneMode.Single);

        var color = ERA_COLORS[sceneName];

        // 1. Encontra todos os artefatos reais da cena
        var artifacts = FindAllArtifacts(scene);
        Debug.Log($"[DisplaySetup] {sceneName}: {artifacts.Count} artefato(s) encontrado(s).");

        // 2. Garante que cada artefato tem subtitleText preenchido
        foreach (var io in artifacts)
        {
            if (string.IsNullOrWhiteSpace(io.subtitleText) &&
                !string.IsNullOrWhiteSpace(io.artifactName))
            {
                io.subtitleText = io.artifactName;
                EditorUtility.SetDirty(io);
            }

            // Garante Collider para o raycast funcionar
            if (io.GetComponent<Collider>() == null)
            {
                io.gameObject.AddComponent<BoxCollider>();
                EditorUtility.SetDirty(io.gameObject);
            }
        }

        // 3. Cria NarratorSystem se não existir
        CreateNarratorSystem(scene);

        // 4. Cria ArtifactInfoPanel se não existir
        CreateArtifactInfoPanel(scene, color);

        // 5. Reconecta ExhibitManager com os artefatos encontrados
        ConnectExhibitManager(scene, artifacts);

        // 6. EventSystem
        EnsureEventSystem(scene);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        return artifacts.Count;
    }

    // ════════════════════════════════════════════════════════════════════
    // VARRE ARTEFATOS
    // ════════════════════════════════════════════════════════════════════

    static List<InteracleObject> FindAllArtifacts(
        UnityEngine.SceneManagement.Scene scene)
    {
        var list = new List<InteracleObject>();
        foreach (var root in scene.GetRootGameObjects())
            foreach (var io in root.GetComponentsInChildren<InteracleObject>(true))
                if (!string.IsNullOrWhiteSpace(io.artifactName))
                    list.Add(io);
        return list;
    }

    // ════════════════════════════════════════════════════════════════════
    // NARRATOR SYSTEM — exibe legenda (subtitleText) na parte inferior
    // ════════════════════════════════════════════════════════════════════

    static void CreateNarratorSystem(UnityEngine.SceneManagement.Scene scene)
    {
        foreach (var root in scene.GetRootGameObjects())
            if (root.GetComponentInChildren<NarratorSystem>(true) != null)
            {
                Debug.Log($"[DisplaySetup] NarratorSystem já existe em {scene.name}.");
                return;
            }

        var go = new GameObject("NarratorCanvas");
        SceneManager.MoveGameObjectToScene(go, scene);

        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20;
        go.AddComponent<CanvasScaler>();

        // Painel de legenda (fundo escuro, parte inferior)
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

        var narrator          = go.AddComponent<NarratorSystem>();
        narrator.subtitleText  = tmp;
        narrator.subtitlePanel = panel;
        narrator.textOnlyDuration = 5f;
        narrator.audioSource  = go.AddComponent<AudioSource>();

        panel.SetActive(false);
        EditorUtility.SetDirty(go);
        Debug.Log($"[DisplaySetup] NarratorSystem criado em {scene.name}.");
    }

    // ════════════════════════════════════════════════════════════════════
    // ARTIFACT INFO PANEL — exibe nome + artifactDescription ao interagir
    // ════════════════════════════════════════════════════════════════════

    static void CreateArtifactInfoPanel(
        UnityEngine.SceneManagement.Scene scene, Color accent)
    {
        foreach (var root in scene.GetRootGameObjects())
            if (root.GetComponentInChildren<ArtifactInfoPanel>(true) != null)
            {
                Debug.Log($"[DisplaySetup] ArtifactInfoPanel já existe em {scene.name}.");
                return;
            }

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

        // Barra colorida no topo
        CreateRect(panel, "TopBar", new Color(accent.r, accent.g, accent.b),
            new Vector2(0f,1f), new Vector2(1f,1f),
            new Vector2(0f, 6f), new Vector2(0f, -3f));

        // Nome do artefato (título)
        var titleGO = CreateRect(panel, "Title",
            new Vector2(0f,1f), new Vector2(1f,1f),
            new Vector2(-32f, 52f), new Vector2(16f, -44f));
        var titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
        titleTMP.fontSize  = 26f;
        titleTMP.color     = accent;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.alignment = TextAlignmentOptions.Left;

        // Linha separadora
        CreateRect(panel, "Separator", new Color(1f,1f,1f,0.1f),
            new Vector2(0f,1f), new Vector2(1f,1f),
            new Vector2(-32f, 2f), new Vector2(0f, -74f));

        // ScrollView com a descrição
        var scrollGO = CreateRect(panel, "ScrollView",
            new Vector2(0f,0f), new Vector2(1f,1f),
            new Vector2(16f, 48f), new Vector2(-16f, -82f));
        var scroll = scrollGO.AddComponent<ScrollRect>();
        scroll.horizontal = false;

        var viewport = CreateRect(scrollGO, "Viewport",
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        viewport.AddComponent<Image>().color = Color.clear;
        viewport.AddComponent<Mask>().showMaskGraphic = false;
        var vRT = viewport.GetComponent<RectTransform>();
        vRT.anchorMin = Vector2.zero;
        vRT.anchorMax = Vector2.one;

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
        dRT.offsetMin = new Vector2(4f, 0f);
        dRT.offsetMax = new Vector2(-4f, 0f);
        var descTMP = descGO.AddComponent<TextMeshProUGUI>();
        descTMP.fontSize           = 18f;
        descTMP.color              = new Color(0.92f, 0.92f, 0.92f);
        descTMP.alignment          = TextAlignmentOptions.TopLeft;
        descTMP.enableWordWrapping = true;

        scroll.viewport = vRT;
        scroll.content  = cRT;

        // Dica [F] fechar
        var hint = CreateRect(panel, "CloseHint",
            new Vector2(0f,0f), new Vector2(1f,0f),
            new Vector2(-32f, 30f), new Vector2(-8f, 16f));
        var hintTMP = hint.AddComponent<TextMeshProUGUI>();
        hintTMP.text      = "[F]  Fechar";
        hintTMP.fontSize  = 15f;
        hintTMP.color     = new Color(1f,1f,1f,0.4f);
        hintTMP.fontStyle = FontStyles.Italic;
        hintTMP.alignment = TextAlignmentOptions.Right;

        // Componente ArtifactInfoPanel
        var aip             = go.AddComponent<ArtifactInfoPanel>();
        aip.panel           = panel;
        aip.titleText       = titleTMP;
        aip.descriptionText = descTMP;
        aip.closeKey        = KeyCode.F;

        panel.SetActive(false);
        EditorUtility.SetDirty(go);
        Debug.Log($"[DisplaySetup] ArtifactInfoPanel criado em {scene.name}.");
    }

    // ════════════════════════════════════════════════════════════════════
    // EXHIBIT MANAGER — conecta com os artefatos encontrados
    // ════════════════════════════════════════════════════════════════════

    static void ConnectExhibitManager(
        UnityEngine.SceneManagement.Scene scene,
        List<InteracleObject> artifacts)
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

        // Conecta barra de progresso existente se houver
        foreach (var root in scene.GetRootGameObjects())
        {
            var slider = root.GetComponentInChildren<Slider>(true);
            if (slider != null) { em.progressBar = slider; break; }
        }
        foreach (var root in scene.GetRootGameObjects())
        {
            var texts = root.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var t in texts)
                if (t.name == "ProgressText") { em.progressText = t; break; }
        }

        EditorUtility.SetDirty(em);
        Debug.Log($"[DisplaySetup] ExhibitManager conectado com {artifacts.Count} artefato(s).");
    }

    // ════════════════════════════════════════════════════════════════════
    // HELPERS
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

    // Cria um RectTransform com cor de fundo
    static GameObject CreateRect(GameObject parent, string name, Color color,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 sizeDelta, Vector2 anchoredPos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        go.AddComponent<RectTransform>();
        go.AddComponent<Image>().color = color;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin        = anchorMin;
        rt.anchorMax        = anchorMax;
        rt.sizeDelta        = sizeDelta;
        rt.anchoredPosition = anchoredPos;
        return go;
    }

    // Cria um RectTransform sem imagem (container)
    static GameObject CreateRect(GameObject parent, string name,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        go.AddComponent<RectTransform>();
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = offsetMin;
        rt.offsetMax = offsetMax;
        return go;
    }

    static void Prog(string msg, float p) =>
        EditorUtility.DisplayProgressBar("ChronoMundi — Conectando Artefatos", msg, p);
}
#endif
