#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// RECONSTRÓI completamente a barra de progresso da FuturoTechScene.
/// 
/// Em vez de tentar consertar a existente, apaga e reconstrói do zero.
/// Garante que todos os componentes estão corretos e conectados.
/// 
/// Menu: ChronoMundi → 🔬 RECONSTRUIR Barra FuturoTech
/// </summary>
public static class FuturoTechBarRebuild
{
    const string SCENE_PATH = "Assets/Projeto/Assets/Scenes/FuturoTechScene.unity";

    [MenuItem("ChronoMundi/🔬 RECONSTRUIR Barra FuturoTech")]
    public static void Rebuild()
    {
        if (!EditorUtility.DisplayDialog("Reconstruir Barra",
            "Vai APAGAR a barra de progresso atual\n" +
            "e RECONSTRUIR do zero?\n\n" +
            "Continuar?", "Sim", "Cancelar"))
            return;

        var scene = EditorSceneManager.OpenScene(SCENE_PATH, OpenSceneMode.Single);

        // 1. Remove canvas antigo
        foreach (var root in scene.GetRootGameObjects())
        {
            if (root.name == "ProgressCanvas")
                Object.DestroyImmediate(root);
        }

        // 2. Reconstrói
        BuildProgressBar(scene);

        // 3. Reconecta ao ExhibitManager
        ReconnectProgressBar(scene);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        EditorUtility.DisplayDialog("✅ Barra Reconstruída",
            "Barra de progresso reconstruída com sucesso!\n\n" +
            "Teste clicando em um artefato.",
            "OK");
    }

    static void BuildProgressBar(UnityEngine.SceneManagement.Scene scene)
    {
        // === CANVAS ===
        var canvasGO = new GameObject("ProgressCanvas");
        SceneManager.MoveGameObjectToScene(canvasGO, scene);
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // === PAINEL ===
        var panelGO = new GameObject("ProgressPanel");
        panelGO.transform.SetParent(canvasGO.transform, false);
        var panelImg = panelGO.AddComponent<Image>();
        panelImg.color = new Color(0f, 0f, 0f, 0.7f);
        
        var panelRT = panelGO.GetComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0, 1);
        panelRT.anchorMax = new Vector2(0, 1);
        panelRT.pivot = new Vector2(0, 1);
        panelRT.anchoredPosition = new Vector2(20, -20);
        panelRT.sizeDelta = new Vector2(300, 80);

        // === TEXTO ===
        var textGO = new GameObject("ProgressText");
        textGO.transform.SetParent(panelGO.transform, false);
        var textTMP = textGO.AddComponent<TextMeshProUGUI>();
        textTMP.text = "0 / 4 artefatos explorados";
        textTMP.fontSize = 18;
        textTMP.color = Color.white;
        textTMP.alignment = TextAlignmentOptions.TopLeft;
        
        var textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = new Vector2(10, -10);
        textRT.offsetMax = new Vector2(-10, -40);

        // === SLIDER (Barra) ===
        var sliderGO = new GameObject("ProgressBar");
        sliderGO.transform.SetParent(panelGO.transform, false);
        var slider = sliderGO.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0f;
        slider.wholeNumbers = false;
        
        var sliderRT = sliderGO.GetComponent<RectTransform>();
        sliderRT.anchorMin = Vector2.zero;
        sliderRT.anchorMax = new Vector2(1, 0);
        sliderRT.pivot = new Vector2(0, 0);
        sliderRT.anchoredPosition = new Vector2(0, 5);
        sliderRT.sizeDelta = new Vector2(-20, 30);

        // === BACKGROUND DO SLIDER ===
        var bgGO = new GameObject("Background");
        bgGO.transform.SetParent(sliderGO.transform, false);
        var bgImg = bgGO.AddComponent<Image>();
        bgImg.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        
        var bgRT = bgGO.GetComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = Vector2.zero;
        bgRT.offsetMax = Vector2.zero;

        slider.targetGraphic = bgImg;

        // === FILL AREA ===
        var fillAreaGO = new GameObject("Fill Area");
        fillAreaGO.transform.SetParent(sliderGO.transform, false);
        
        var fillAreaRT = fillAreaGO.GetComponent<RectTransform>();
        if (fillAreaRT == null) fillAreaRT = fillAreaGO.AddComponent<RectTransform>();
        fillAreaRT.anchorMin = Vector2.zero;
        fillAreaRT.anchorMax = Vector2.one;
        fillAreaRT.offsetMin = Vector2.zero;
        fillAreaRT.offsetMax = Vector2.zero;

        // === FILL (A barra que cresce) ===
        var fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(fillAreaGO.transform, false);
        var fillImg = fillGO.AddComponent<Image>();
        fillImg.color = new Color(0.2f, 0.8f, 0.4f, 1f);  // Verde
        
        var fillRT = fillGO.GetComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = new Vector2(0, 1);  // ← CRÍTICO: Cresce para direita
        fillRT.offsetMin = Vector2.zero;
        fillRT.offsetMax = Vector2.zero;

        slider.fillRect = fillRT;

        // === MARCAR COMO DIRTY ===
        EditorUtility.SetDirty(canvasGO);
        EditorUtility.SetDirty(panelGO);
        EditorUtility.SetDirty(sliderGO);
        EditorUtility.SetDirty(fillGO);

        Debug.Log("[BarRebuild] ✅ Barra reconstruída:");
        Debug.Log($"  Slider: {slider.name}");
        Debug.Log($"  fillRect: {fillRT.gameObject.name}");
        Debug.Log($"  Fill anchorMax: {fillRT.anchorMax}");
    }

    static void ReconnectProgressBar(UnityEngine.SceneManagement.Scene scene)
    {
        // Encontra ExhibitManager
        ExhibitManager em = null;
        foreach (var root in scene.GetRootGameObjects())
        {
            em = root.GetComponentInChildren<ExhibitManager>(true);
            if (em != null) break;
        }

        if (em == null)
        {
            UnityEngine.Debug.LogWarning("[BarRebuild] ExhibitManager não encontrado!");
            return;
        }

        // Encontra componentes da barra
        Canvas canvas = null;
        GameObject progressPanel = null;
        Slider slider = null;
        TextMeshProUGUI textTMP = null;

        foreach (var root in scene.GetRootGameObjects())
        {
            canvas = root.GetComponentInChildren<Canvas>(true);
            if (canvas != null && canvas.name == "ProgressCanvas")
            {
                // Encontra o ProgressPanel
                progressPanel = canvas.transform.Find("ProgressPanel")?.gameObject;
                slider = canvas.GetComponentInChildren<Slider>(true);
                textTMP = canvas.GetComponentInChildren<TextMeshProUGUI>(true);
                break;
            }
        }

        // Conecta ao ExhibitManager
        if (canvas != null && progressPanel != null && slider != null)
        {
            em.progressPanel = progressPanel;
            em.progressBar = slider;
            em.progressText = textTMP;
            EditorUtility.SetDirty(em);

            UnityEngine.Debug.Log("[BarRebuild] ✅ ExhibitManager reconectado!");
            UnityEngine.Debug.Log($"  progressPanel: {em.progressPanel.name}");
            UnityEngine.Debug.Log($"  progressBar: {em.progressBar.name}");
            UnityEngine.Debug.Log($"  progressText: {(em.progressText != null ? em.progressText.name : "NULL")}");
        }
        else
        {
            UnityEngine.Debug.LogError("[BarRebuild] ❌ Falha ao reconectar! Canvas ou componentes não encontrados.");
            UnityEngine.Debug.Log($"  canvas: {canvas}");
            UnityEngine.Debug.Log($"  progressPanel: {progressPanel}");
            UnityEngine.Debug.Log($"  slider: {slider}");
        }
    }
}
#endif
