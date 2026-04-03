#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gera prefabs base do projeto com um clique.
/// Cria em Assets/Projeto/Assets/Prefabs e também em Assets/Resources
/// (para funcionar com SceneBootstrapper automaticamente).
/// </summary>
public static class ChronoMundiPrefabBuilder
{
    private const string PrefabsFolder = "Assets/Projeto/Assets/Prefabs";
    private const string ResourcesFolder = "Assets/Resources";

    [MenuItem("ChronoMundi/Prefabs/Recriar Prefabs Base")]
    public static void RebuildBasePrefabs()
    {
        EnsureFolder(PrefabsFolder);
        EnsureFolder(ResourcesFolder);

        BuildGameManagerPrefab();
        BuildNarratorSystemPrefab();
        BuildLoadingScreenPrefab();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "ChronoMundi Prefabs",
            "Prefabs base atualizados em:\n- Assets/Projeto/Assets/Prefabs\n- Assets/Resources",
            "OK");
    }

    private static void BuildGameManagerPrefab()
    {
        var root = new GameObject("GameManager_Prefab");
        root.AddComponent<GameManager>();
        root.AddComponent<AudioManager>();

        SavePrefab(root, "GameManager_Prefab.prefab");
    }

    private static void BuildNarratorSystemPrefab()
    {
        var root = new GameObject("NarratorSystem_Prefab");
        var narrator = root.AddComponent<NarratorSystem>();
        narrator.audioSource = root.AddComponent<AudioSource>();

        var canvas = CreateCanvas("NarratorCanvas", root.transform, 950);
        var subtitlePanel = CreatePanel("SubtitlePanel", canvas.transform, new Color(0f, 0f, 0f, 0.65f));
        subtitlePanel.SetActive(false);

        var subtitle = CreateTMPText(
            "SubtitleText",
            subtitlePanel.transform,
            "",
            30,
            TextAlignmentOptions.Center);

        narrator.subtitlePanel = subtitlePanel;
        narrator.subtitleText = subtitle;

        SavePrefab(root, "NarratorSystem_Prefab.prefab");
    }

    private static void BuildLoadingScreenPrefab()
    {
        var root = new GameObject("LoadingScreen_Prefab");
        var loading = root.AddComponent<LoadingScreen>();
        var fader = root.AddComponent<SceneFader>();

        var canvas = CreateCanvas("LoadingCanvas", root.transform, 1000);
        var panel = CreatePanel("LoadingPanel", canvas.transform, new Color(0f, 0f, 0f, 0.85f));

        var progressText = CreateTMPText(
            "ProgressText",
            panel.transform,
            "0%",
            34,
            TextAlignmentOptions.Center);
        SetAnchors(progressText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 70), new Vector2(400, 60));

        var tipText = CreateTMPText(
            "TipText",
            panel.transform,
            "Dica...",
            24,
            TextAlignmentOptions.Center);
        SetAnchors(tipText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -80), new Vector2(1000, 100));

        var sliderGO = new GameObject("ProgressBar", typeof(RectTransform), typeof(Slider));
        sliderGO.transform.SetParent(panel.transform, false);
        var sliderRect = sliderGO.GetComponent<RectTransform>();
        SetAnchors(sliderRect, new Vector2(0.2f, 0.5f), new Vector2(0.8f, 0.5f), new Vector2(0, 0), new Vector2(0, 30));

        var slider = sliderGO.GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0f;

        // Fill
        var fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(sliderGO.transform, false);
        var fillAreaRect = fillArea.GetComponent<RectTransform>();
        fillAreaRect.anchorMin = new Vector2(0f, 0f);
        fillAreaRect.anchorMax = new Vector2(1f, 1f);
        fillAreaRect.offsetMin = Vector2.zero;
        fillAreaRect.offsetMax = Vector2.zero;

        var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fill.transform.SetParent(fillArea.transform, false);
        var fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        var fillImage = fill.GetComponent<Image>();
        fillImage.color = new Color(0.2f, 0.8f, 1f, 1f);

        // Background
        var bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bg.transform.SetParent(sliderGO.transform, false);
        bg.transform.SetAsFirstSibling();
        var bgRect = bg.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        var bgImage = bg.GetComponent<Image>();
        bgImage.color = new Color(1f, 1f, 1f, 0.15f);

        slider.fillRect = fillRect;
        slider.targetGraphic = fillImage;

        loading.loadingPanel = panel;
        loading.progressBar = slider;
        loading.progressText = progressText;
        loading.tipText = tipText;

        var fadeCanvas = CreateCanvas("FadeCanvas", root.transform, 1100);
        var fadeImageGO = CreatePanel("FadeImage", fadeCanvas.transform, Color.black);
        var fadeImage = fadeImageGO.GetComponent<Image>();
        fadeImage.color = new Color(0f, 0f, 0f, 0f);
        fader.fadeImage = fadeImage;

        SavePrefab(root, "LoadingScreen_Prefab.prefab");
    }

    private static Canvas CreateCanvas(string name, Transform parent, int sortOrder)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        go.transform.SetParent(parent, false);

        var canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortOrder;

        var scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        return canvas;
    }

    private static GameObject CreatePanel(string name, Transform parent, Color color)
    {
        var panel = new GameObject(name, typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);
        var rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        panel.GetComponent<Image>().color = color;
        return panel;
    }

    private static TextMeshProUGUI CreateTMPText(
        string name,
        Transform parent,
        string text,
        int fontSize,
        TextAlignmentOptions alignment)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);

        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = alignment;
        tmp.color = Color.white;

        var rect = tmp.rectTransform;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(800, 100);

        return tmp;
    }

    private static void SetAnchors(RectTransform rect, Vector2 min, Vector2 max, Vector2 anchoredPos, Vector2 size)
    {
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = size;
    }

    private static void SavePrefab(GameObject root, string fileName)
    {
        var prefabPath = $"{PrefabsFolder}/{fileName}";
        var resourcePath = $"{ResourcesFolder}/{fileName}";

        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        PrefabUtility.SaveAsPrefabAsset(root, resourcePath);

        Object.DestroyImmediate(root);
    }

    private static void EnsureFolder(string folderPath)
    {
        var parts = folderPath.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = $"{current}/{parts[i]}";
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }
}
#endif
