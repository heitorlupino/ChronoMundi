#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Configura o projeto ChronoMundi para Google Cardboard (celular normal).
///
/// O que faz:
///   1. Adiciona o Google Cardboard XDK ao Packages/manifest.json
///   2. Configura Android: minSDK 24, orientação Landscape, instala Cardboard
///   3. Remove XR Origin (não usado no Cardboard — câmera simples)
///   4. Cria prefab de reticle (mira circular) no Canvas de cada cena
///   5. Adiciona GazeInteraction na Main Camera de cada cena
///   6. Configura ProjectSettings para split-screen stereo
///
/// Diferença Cardboard vs Quest:
///   Quest   → XR Origin + Ray Interactor + controller físico
///   Cardboard → Camera normal + Raycast central + gaze timer + toque na tela
///
/// Menu: ChronoMundi → 📱 Configurar Google Cardboard (celular)
/// </summary>
public static class CardboardSetup
{
    const string SCENES_PATH = "Assets/Projeto/Assets/Scenes/";
    const string MANIFEST    = "Packages/manifest.json";

    // Cardboard XDK — pacote oficial Google para Unity
    // https://developers.google.com/cardboard/develop/unity/quickstart
    const string CARDBOARD_PACKAGE   = "com.google.xr.cardboard";
    const string CARDBOARD_VERSION   = "1.22.0";
    const string CARDBOARD_REGISTRY  = "https://package.openupm.com";

    static readonly string[] SCENE_NAMES =
        { "MuseuHubScene", "PreHistoriaScene", "IdadeMediaScene", "FuturoTechScene" };

    // ════════════════════════════════════════════════════════════════════
    [MenuItem("ChronoMundi/📱 Configurar Google Cardboard (celular)")]
    public static void Setup()
    {
        if (!EditorUtility.DisplayDialog("ChronoMundi — Google Cardboard",
            "Configura o projeto para celular com Google Cardboard:\n\n" +
            "1. Adiciona Cardboard XDK ao manifest.json\n" +
            "2. Configura Android (minSDK 24, Landscape)\n" +
            "3. Remove XR Origin das cenas (não necessário)\n" +
            "4. Adiciona GazeInteraction + mira circular\n" +
            "5. Ativa split-screen stereo do Cardboard\n\n" +
            "⚠ Isso substitui a configuração OpenXR/Quest.\n" +
            "Para reverter: ChronoMundi → ↩ Desfazer Setup VR",
            "Configurar Cardboard", "Cancelar"))
            return;

        Prog("Adicionando Cardboard ao manifest...", 0.1f);
        bool manifestOK = AddCardboardToManifest();

        Prog("Configurando Android...", 0.25f);
        ConfigureAndroidSettings();

        Prog("Desativando OpenXR Loader...", 0.35f);
        DisableOpenXRLoader();

        int scenes = 0;
        float p = 0.4f;
        foreach (var name in SCENE_NAMES)
        {
            Prog($"Configurando gaze em {name}...", p += 0.13f);
            if (ConfigureSceneForCardboard(name)) scenes++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();

        EditorUtility.DisplayDialog("ChronoMundi ✅ Cardboard",
            $"Configuração concluída!\n\n" +
            $"✔ Cardboard XDK adicionado ao manifest\n" +
            $"✔ Android configurado (minSDK 24, Landscape)\n" +
            $"✔ OpenXR desativado\n" +
            $"✔ GazeInteraction em {scenes}/4 cenas\n\n" +
            "Próximos passos:\n" +
            "1. Unity vai baixar o pacote Cardboard\n" +
            "   (aguarde reimport automático)\n" +
            "2. Edit → Project Settings → XR Plug-in Management\n" +
            "   → Android → ✔ Cardboard XR Plugin\n" +
            "3. File → Build Settings → Android → Build\n" +
            "4. Instale o APK no celular e use com o Cardboard",
            "OK");
    }

    // ════════════════════════════════════════════════════════════════════
    // 1. MANIFEST — adiciona Cardboard SDK + scoped registry OpenUPM
    // ════════════════════════════════════════════════════════════════════

    static bool AddCardboardToManifest()
    {
        string raw = File.ReadAllText(MANIFEST);

        // Já tem o pacote?
        if (raw.Contains(CARDBOARD_PACKAGE))
        {
            Debug.Log("[Cardboard] Pacote já está no manifest.");
            return true;
        }

        // Adiciona scoped registry OpenUPM se não existir
        if (!raw.Contains("package.openupm.com"))
        {
            string scopedRegistry =
                "  \"scopedRegistries\": [\n" +
                "    {\n" +
                $"      \"name\": \"OpenUPM\",\n" +
                $"      \"url\": \"{CARDBOARD_REGISTRY}\",\n" +
                "      \"scopes\": [\n" +
                "        \"com.google\"\n" +
                "      ]\n" +
                "    }\n" +
                "  ],\n";

            // Insere antes de "dependencies"
            raw = raw.Replace("\"dependencies\":", scopedRegistry + "  \"dependencies\":");
            Debug.Log("[Cardboard] Scoped registry OpenUPM adicionado.");
        }

        // Adiciona a dependência
        raw = raw.Replace(
            "\"dependencies\": {",
            $"\"dependencies\": {{\n    \"{CARDBOARD_PACKAGE}\": \"{CARDBOARD_VERSION}\",");

        File.WriteAllText(MANIFEST, raw);
        Debug.Log($"[Cardboard] {CARDBOARD_PACKAGE} {CARDBOARD_VERSION} adicionado ao manifest.");
        return true;
    }

    // ════════════════════════════════════════════════════════════════════
    // 2. ANDROID SETTINGS
    // ════════════════════════════════════════════════════════════════════

    static void ConfigureAndroidSettings()
    {
        // Min SDK 24 (Android 7.0) — requisito mínimo do Cardboard
        if (PlayerSettings.Android.minSdkVersion < AndroidSdkVersions.AndroidApiLevel24)
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;

        // Orientação Landscape (obrigatório para o split-screen Cardboard)
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;

        // Fullscreen
        PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;

        // Desativa multi-threaded rendering (evita artefatos no Cardboard)
        PlayerSettings.MTRendering = false;

        EditorUtility.SetDirty(AssetDatabase.LoadAssetAtPath<Object>("ProjectSettings/ProjectSettings.asset"));
        Debug.Log("[Cardboard] Android settings configurados.");
    }

    // ════════════════════════════════════════════════════════════════════
    // 3. DESATIVA OPENXR (não compatível com Cardboard)
    // ════════════════════════════════════════════════════════════════════

    static void DisableOpenXRLoader()
    {
        foreach (var group in new[] { BuildTargetGroup.Standalone, BuildTargetGroup.Android })
        {
            var settings = UnityEditor.XR.Management.XRGeneralSettingsPerBuildTarget
                .XRGeneralSettingsForBuildTarget(group);
            if (settings == null) continue;

            var so = new SerializedObject(settings);
            var al = so.FindProperty("m_AutomaticLoading");
            var ar = so.FindProperty("m_AutomaticRunning");
            if (al != null) al.boolValue = false;
            if (ar != null) ar.boolValue = false;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(settings);
        }
        Debug.Log("[Cardboard] OpenXR loader desativado.");
    }

    // ════════════════════════════════════════════════════════════════════
    // 4 + 5. CENAS — remove XR Origin, adiciona gaze
    // ════════════════════════════════════════════════════════════════════

    static bool ConfigureSceneForCardboard(string sceneName)
    {
        string path = SCENES_PATH + sceneName + ".unity";
        if (!File.Exists(path)) return false;

        var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

        // Remove XR Origin se existir
        foreach (var root in scene.GetRootGameObjects())
        {
            if (root.name.StartsWith("XR Origin") ||
                root.name.StartsWith("Complete XR Origin") ||
                root.name == "XR Rig" ||
                root.name == "VRModeManager")
            {
                Object.DestroyImmediate(root);
                Debug.Log($"[Cardboard] '{root?.name}' removido de {sceneName}.");
            }
        }

        // Encontra a Main Camera
        Camera mainCam = null;
        foreach (var root in scene.GetRootGameObjects())
        {
            mainCam = root.GetComponentInChildren<Camera>(true);
            if (mainCam != null) break;
        }

        if (mainCam == null)
        {
            Debug.LogWarning($"[Cardboard] Main Camera não encontrada em {sceneName}!");
            return false;
        }

        // Adiciona GazeInteraction na câmera
        AddGazeInteraction(scene, mainCam);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        return true;
    }

    static void AddGazeInteraction(UnityEngine.SceneManagement.Scene scene, Camera cam)
    {
        // Remove gaze antigo
        var old = cam.GetComponent<GazeInteraction>();
        if (old != null) Object.DestroyImmediate(old);

        // Cria o canvas de reticle (mira) em Screen Space Overlay
        var reticleCanvas = BuildReticleCanvas(scene, cam);
        var reticleImage  = reticleCanvas.GetComponentInChildren<Image>();

        // Adiciona GazeInteraction
        var gaze = cam.gameObject.AddComponent<GazeInteraction>();
        gaze.gazeTime         = 2f;
        gaze.gazeDistance     = 5f;
        gaze.interactableLayer = ~0;
        gaze.reticleRoot      = reticleCanvas.gameObject;
        gaze.reticleProgress  = reticleImage;
        gaze.reticleIdleColor   = new Color(1f, 1f, 1f, 0.55f);
        gaze.reticleActiveColor = new Color(0.3f, 1f, 0.5f, 1f);

        EditorUtility.SetDirty(cam.gameObject);
        Debug.Log($"[Cardboard] GazeInteraction adicionado à câmera em {scene.name}.");
    }

    /// <summary>
    /// Constrói o canvas da mira circular (reticle):
    ///   - Fundo: círculo branco pequeno (centro da tela)
    ///   - Fill: Image com Radial360 que avança conforme o gaze timer
    /// </summary>
    static Canvas BuildReticleCanvas(
        UnityEngine.SceneManagement.Scene scene, Camera cam)
    {
        // Remove antigo
        foreach (var root in scene.GetRootGameObjects())
            if (root.name == "GazeReticleCanvas")
                Object.DestroyImmediate(root);

        // Canvas raiz
        var canvasGO = new GameObject("GazeReticleCanvas");
        SceneManager.MoveGameObjectToScene(canvasGO, scene);
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // Âncora central
        var anchor = new GameObject("ReticleAnchor");
        anchor.transform.SetParent(canvasGO.transform, false);
        var anchorRT = anchor.AddComponent<RectTransform>();
        anchorRT.anchorMin  = new Vector2(0.5f, 0.5f);
        anchorRT.anchorMax  = new Vector2(0.5f, 0.5f);
        anchorRT.sizeDelta  = Vector2.zero;
        anchorRT.anchoredPosition = Vector2.zero;

        // Fundo do reticle (ponto branco fixo)
        var bg = new GameObject("ReticleBg");
        bg.transform.SetParent(anchor.transform, false);
        var bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(1f, 1f, 1f, 0.35f);
        bgImg.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Knob.psd");
        var bgRT = bg.GetComponent<RectTransform>();
        bgRT.sizeDelta = new Vector2(28f, 28f);
        bgRT.anchoredPosition = Vector2.zero;

        // Fill radial (progresso do gaze)
        var fill = new GameObject("ReticleFill");
        fill.transform.SetParent(anchor.transform, false);
        var fillImg = fill.AddComponent<Image>();
        fillImg.sprite    = Resources.GetBuiltinResource<Sprite>("UI/Skin/Knob.psd");
        fillImg.type      = Image.Type.Filled;
        fillImg.fillMethod = Image.FillMethod.Radial360;
        fillImg.fillOrigin = (int)Image.Origin360.Top;
        fillImg.fillClockwise = true;
        fillImg.fillAmount = 0f;
        fillImg.color = new Color(0.3f, 1f, 0.5f, 1f);
        var fillRT = fill.GetComponent<RectTransform>();
        fillRT.sizeDelta = new Vector2(36f, 36f);
        fillRT.anchoredPosition = Vector2.zero;

        // Texto de dica (opcional — "Olhe para interagir")
        var hint = new GameObject("GazeHint");
        hint.transform.SetParent(anchor.transform, false);
        var hintTMP = hint.AddComponent<TextMeshProUGUI>();
        hintTMP.text      = "Olhe para interagir";
        hintTMP.fontSize  = 18;
        hintTMP.color     = new Color(1f, 1f, 1f, 0.7f);
        hintTMP.alignment = TextAlignmentOptions.Center;
        var hintRT = hint.GetComponent<RectTransform>();
        hintRT.sizeDelta        = new Vector2(280f, 40f);
        hintRT.anchoredPosition = new Vector2(0f, -36f);

        // Começa desativado — ativa quando há alvo
        fill.SetActive(true);
        EditorUtility.SetDirty(canvasGO);
        return canvas;
    }

    static void Prog(string msg, float p) =>
        EditorUtility.DisplayProgressBar("ChronoMundi — Cardboard", msg, p);
}
#endif