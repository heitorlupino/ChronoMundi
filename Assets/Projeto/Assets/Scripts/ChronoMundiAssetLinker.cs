#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.IO;

/// <summary>
/// Popula todas as pastas vazias do ChronoMundi e constrói os ambientes das cenas
/// reaproveitando os assets do VR Template que já estão no projeto.
///
/// Menu Unity → ChronoMundi → 🎨 Popular Assets + Construir Ambientes
/// </summary>
public static class ChronoMundiAssetLinker
{
    // GUIDs dos assets do VR Template (já existem no projeto)
    const string GUID_MAT_CONCRETE      = "00dc025bc6aa88645ad0114c7607fb6f";
    const string GUID_MAT_WALL          = "dcac969f335985b47a7dd104e05496d9";
    const string GUID_MAT_GLASS         = "9c52a7e85c986c2429f4638c6756501c";
    const string GUID_MAT_DARK_GREEN    = "d2fd0fd7f230ce14d89e18fb555b7469";
    const string GUID_MAT_GREY          = "e1b4debff657aa94293a2e4e5f15b8a0";
    const string GUID_MAT_CHROME        = "5d46bb438eef41d449bb7588f4a77e06";
    const string GUID_MAT_CONCRETE_BLUE = "895b038bf03adba4c951968630c6032d";
    const string GUID_MAT_CONCRETE_GREY = "7d02519ee5c8cb745b09b59fc3d367fe";
    const string GUID_MAT_INTERACT      = "377a24320b3adeb438edc10508f4ced8";
    const string GUID_MAT_INTERACT2     = "b2dbb5f1f2068794eba2d105124bf21e";
    const string GUID_MAT_INTERACT3     = "cf6ddc7949dbe3c4992fd95474f486f6";
    const string GUID_MAT_INTERACT4     = "f5cd77343d6ca1949b1d2d7511848832";
    const string GUID_MAT_INTERACT5     = "83eabfe673263a445972586e5d8b56ee";
    const string GUID_MAT_GREEN         = "c4fad0f5843a8fc4f8488ab522a78684";
    const string GUID_FONT_INTER        = "f675742eaf9c20a4f909d78ee7a14fed";
    const string GUID_XR_ORIGIN         = "77e7c27b2c5525e4aa8cc9f99d654486";
    const string GUID_SPRITE_ROUND      = "65d31ca9600c4654e886aa7bab36e94b";

    const string PROJ = "Assets/Projeto/Assets";

    // ── Entry point ────────────────────────────────────────────────────────
    [MenuItem("ChronoMundi/🎨 Popular Assets + Construir Ambientes")]
    public static void Run()
    {
        float p = 0f;
        Progress("Criando pastas...", p += 0.05f);
        EnsureFolders();

        Progress("Criando materiais do projeto...", p += 0.1f);
        CreateProjectMaterials();

        Progress("Criando material de highlight...", p += 0.05f);
        CreateHighlightMaterial();

        Progress("Criando prefabs de UI...", p += 0.1f);
        CreateUIPrefabs();

        Progress("Construindo MenuScene...", p += 0.15f);
        BuildMenuScene();

        Progress("Construindo MuseuHubScene...", p += 0.15f);
        BuildHubScene();

        Progress("Construindo PreHistoriaScene...", p += 0.15f);
        BuildHistoricalScene("PreHistoriaScene",  EraStyle.PreHistoria);

        Progress("Construindo IdadeMediaScene...", p += 0.15f);
        BuildHistoricalScene("IdadeMediaScene",   EraStyle.IdadeMedia);

        Progress("Construindo FuturoTechScene...", p += 0.15f);
        BuildHistoricalScene("FuturoTechScene",   EraStyle.FuturoTech);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();

        EditorUtility.DisplayDialog("ChronoMundi",
            "✅ Assets populados e ambientes construídos!\n\n" +
            "• Materiais criados em Projeto/Assets/Materials/\n" +
            "• Prefabs de UI em Projeto/Assets/UI/\n" +
            "• Todas as 5 cenas têm ambiente 3D\n\n" +
            "Próximo: adicione áudio nos InteracleObjects e ajuste a iluminação.",
            "OK");
    }

    // ── Pastas ─────────────────────────────────────────────────────────────
    static void EnsureFolders()
    {
        string[] paths = {
            $"{PROJ}/Materials",
            $"{PROJ}/Materials/Environment",
            $"{PROJ}/Materials/UI",
            $"{PROJ}/Prefabs",
            $"{PROJ}/Prefabs/Resources",
            $"{PROJ}/Prefabs/Environment",
            $"{PROJ}/Prefabs/UI",
            $"{PROJ}/UI",
            $"{PROJ}/Audio",
            $"{PROJ}/Enviroments",
            $"{PROJ}/Enviroments/PreHistoria",
            $"{PROJ}/Enviroments/IdadeMedia",
            $"{PROJ}/Enviroments/FuturoTech",
            $"{PROJ}/Enviroments/Hub",
        };
        foreach (var path in paths)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                var parts  = path.Split('/');
                var parent = string.Join("/", parts, 0, parts.Length - 1);
                AssetDatabase.CreateFolder(parent, parts[parts.Length - 1]);
            }
        }
        AssetDatabase.Refresh();
    }

    // ── Materiais ──────────────────────────────────────────────────────────
    static void CreateProjectMaterials()
    {
        // Cria materiais no projeto que referenciam os do VR Template via GUID
        // (sem duplicar texturas — apenas alias materials com cor base definida)

        CreateMat("Floor_Stone",    GUID_MAT_CONCRETE,      new Color(0.45f, 0.42f, 0.38f));
        CreateMat("Floor_Dirt",     GUID_MAT_DARK_GREEN,    new Color(0.35f, 0.28f, 0.18f));
        CreateMat("Floor_Tech",     GUID_MAT_CHROME,        new Color(0.18f, 0.22f, 0.28f));
        CreateMat("Wall_Stone",     GUID_MAT_WALL,          new Color(0.50f, 0.48f, 0.44f));
        CreateMat("Wall_Cave",      GUID_MAT_CONCRETE_GREY, new Color(0.30f, 0.28f, 0.25f));
        CreateMat("Wall_Tech",      GUID_MAT_CONCRETE_BLUE, new Color(0.12f, 0.18f, 0.30f));
        CreateMat("Door_Wood",      GUID_MAT_INTERACT2,     new Color(0.55f, 0.35f, 0.15f));
        CreateMat("Door_Metal",     GUID_MAT_CHROME,        new Color(0.60f, 0.62f, 0.65f));
        CreateMat("Artifact_Bone",  GUID_MAT_GREY,          new Color(0.90f, 0.88f, 0.82f));
        CreateMat("Artifact_Stone", GUID_MAT_CONCRETE_GREY, new Color(0.55f, 0.52f, 0.48f));
        CreateMat("Artifact_Metal", GUID_MAT_CHROME,        new Color(0.72f, 0.75f, 0.80f));
        CreateMat("Artifact_Glow",  GUID_MAT_INTERACT3,     new Color(0.20f, 0.85f, 0.95f));
        CreateMat("Ceiling_Stone",  GUID_MAT_CONCRETE,      new Color(0.38f, 0.36f, 0.34f));
        CreateMat("Ceiling_Cave",   GUID_MAT_CONCRETE_GREY, new Color(0.22f, 0.20f, 0.18f));
        CreateMat("Pillar_Stone",   GUID_MAT_WALL,          new Color(0.52f, 0.50f, 0.46f));
        CreateMat("Glass_Panel",    GUID_MAT_GLASS,         new Color(0.60f, 0.85f, 1.00f, 0.3f));
    }

    static void CreateMat(string name, string baseGuid, Color color)
    {
        string path = $"{PROJ}/Materials/Environment/{name}.mat";
        if (File.Exists(path)) return;

        // Carrega o material base do VR Template para clonar
        string basePath = AssetDatabase.GUIDToAssetPath(baseGuid);
        var baseMat = AssetDatabase.LoadAssetAtPath<Material>(basePath);
        if (baseMat == null) { Debug.LogWarning($"[AssetLinker] Base mat not found: {basePath}"); return; }

        var mat = new Material(baseMat);
        mat.name = name;
        mat.SetColor("_BaseColor", color);
        AssetDatabase.CreateAsset(mat, path);
    }

    // ── Highlight Material ─────────────────────────────────────────────────
    static void CreateHighlightMaterial()
    {
        string path = $"{PROJ}/Materials/Highlight_Interactable.mat";
        if (File.Exists(path)) return;

        string basePath = AssetDatabase.GUIDToAssetPath(GUID_MAT_INTERACT);
        var baseMat = AssetDatabase.LoadAssetAtPath<Material>(basePath);
        if (baseMat == null) return;

        var mat = new Material(baseMat);
        mat.name = "Highlight_Interactable";
        mat.SetColor("_BaseColor", new Color(1f, 0.85f, 0.1f, 0.8f));
        mat.SetColor("_EmissionColor", new Color(1f, 0.7f, 0f) * 1.5f);
        mat.EnableKeyword("_EMISSION");
        AssetDatabase.CreateAsset(mat, path);
    }

    // ── UI Prefabs ─────────────────────────────────────────────────────────
    static void CreateUIPrefabs()
    {
        CreateDoorLabelPrefab();
        CreateArtifactHighlightRingPrefab();
        CreateWorldSpaceInfoPrefab();
    }

    static void CreateDoorLabelPrefab()
    {
        string path = $"{PROJ}/Prefabs/UI/DoorLabel.prefab";
        if (File.Exists(path)) return;

        var root = new GameObject("DoorLabel");
        root.transform.localScale = Vector3.one * 0.01f;

        var canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        root.AddComponent<CanvasScaler>();

        var bg = new GameObject("Background");
        bg.transform.SetParent(root.transform, false);
        var bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0f, 0f, 0f, 0.7f);
        var bgRT = bg.GetComponent<RectTransform>();
        bgRT.sizeDelta = new Vector2(300, 60);

        var txt = new GameObject("Label");
        txt.transform.SetParent(bg.transform, false);
        var tmp = txt.AddComponent<TextMeshProUGUI>();
        tmp.text = "Era";
        tmp.fontSize = 36;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        var tRT = txt.GetComponent<RectTransform>();
        tRT.anchorMin = Vector2.zero;
        tRT.anchorMax = Vector2.one;
        tRT.offsetMin = tRT.offsetMax = Vector2.zero;

        SavePrefabAsset(root, path);
        Object.DestroyImmediate(root);
    }

    static void CreateArtifactHighlightRingPrefab()
    {
        string path = $"{PROJ}/Prefabs/Environment/HighlightRing.prefab";
        if (File.Exists(path)) return;

        var root = new GameObject("HighlightRing");
        var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ring.name = "Ring";
        ring.transform.SetParent(root.transform, false);
        ring.transform.localScale = new Vector3(1.2f, 0.02f, 1.2f);
        ring.transform.localPosition = new Vector3(0, -0.5f, 0);
        Object.DestroyImmediate(ring.GetComponent<Collider>());

        string matPath = $"{PROJ}/Materials/Highlight_Interactable.mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (mat != null)
            ring.GetComponent<Renderer>().sharedMaterial = mat;

        ring.AddComponent<HighlightController>();

        SavePrefabAsset(root, path);
        Object.DestroyImmediate(root);
    }

    static void CreateWorldSpaceInfoPrefab()
    {
        string path = $"{PROJ}/Prefabs/UI/WorldSpaceInfo.prefab";
        if (File.Exists(path)) return;

        var root = new GameObject("WorldSpaceInfo");
        root.transform.localScale = Vector3.one * 0.005f;

        var canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        root.AddComponent<CanvasScaler>();

        var panel = new GameObject("Panel");
        panel.transform.SetParent(root.transform, false);
        var panelImg = panel.AddComponent<Image>();
        panelImg.color = new Color(0.05f, 0.05f, 0.1f, 0.9f);
        var panelRT = panel.GetComponent<RectTransform>();
        panelRT.sizeDelta = new Vector2(500, 250);

        var title = new GameObject("Title");
        title.transform.SetParent(panel.transform, false);
        var titleTMP = title.AddComponent<TextMeshProUGUI>();
        titleTMP.text = "Nome do Artefato";
        titleTMP.fontSize = 38;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.color = Color.white;
        titleTMP.alignment = TextAlignmentOptions.TopLeft;
        var tRT = title.GetComponent<RectTransform>();
        tRT.anchorMin = new Vector2(0, 0.6f);
        tRT.anchorMax = Vector2.one;
        tRT.offsetMin = new Vector2(20, 10);
        tRT.offsetMax = new Vector2(-20, -10);

        var desc = new GameObject("Description");
        desc.transform.SetParent(panel.transform, false);
        var descTMP = desc.AddComponent<TextMeshProUGUI>();
        descTMP.text = "Descrição do artefato histórico.";
        descTMP.fontSize = 28;
        descTMP.color = new Color(0.85f, 0.85f, 0.85f);
        descTMP.alignment = TextAlignmentOptions.TopLeft;
        descTMP.enableWordWrapping = true;
        var dRT = desc.GetComponent<RectTransform>();
        dRT.anchorMin = Vector2.zero;
        dRT.anchorMax = new Vector2(1, 0.58f);
        dRT.offsetMin = new Vector2(20, 10);
        dRT.offsetMax = new Vector2(-20, -5);

        SavePrefabAsset(root, path);
        Object.DestroyImmediate(root);
    }

    // ══════════════════════════════════════════════════════════════════════
    // SCENE BUILDERS
    // ══════════════════════════════════════════════════════════════════════

    enum EraStyle { PreHistoria, IdadeMedia, FuturoTech }

    // ── MenuScene ──────────────────────────────────────────────────────────
    static void BuildMenuScene()
    {
        var scene = EditorSceneManager.OpenScene(
            "Assets/Projeto/Assets/Scenes/MenuScene.unity", OpenSceneMode.Single);

        // Chão e paredes simples
        var floor = MakePlane("Floor", 20f, 20f, LoadMat("Floor_Stone"), Vector3.zero);
        MoveToScene(floor, scene);

        var backWall  = MakeWall("Wall_Back",  new Vector3(0, 3, 6),   new Vector3(20, 6, 0.3f), LoadMat("Wall_Stone"));
        var leftWall  = MakeWall("Wall_Left",  new Vector3(-10, 3, 0), new Vector3(0.3f, 6, 20), LoadMat("Wall_Stone"));
        var rightWall = MakeWall("Wall_Right", new Vector3(10, 3, 0),  new Vector3(0.3f, 6, 20), LoadMat("Wall_Stone"));
        var ceiling   = MakeWall("Ceiling",    new Vector3(0, 6, 0),   new Vector3(20, 0.2f, 20), LoadMat("Ceiling_Stone"));
        foreach (var w in new[]{backWall, leftWall, rightWall, ceiling}) MoveToScene(w, scene);

        // Colunas decorativas
        for (int i = -1; i <= 1; i += 2)
        {
            var col = MakeBox($"Pillar_{i}", new Vector3(i * 4f, 1.5f, 2f),
                               new Vector3(0.4f, 3f, 0.4f), LoadMat("Pillar_Stone"));
            MoveToScene(col, scene);
        }

        // Iluminação da cena de menu
        SetupLighting(scene, new Color(0.95f, 0.90f, 0.80f), 1.2f,
                             new Color(0.12f, 0.14f, 0.18f));

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    // ── MuseuHubScene ──────────────────────────────────────────────────────
    static void BuildHubScene()
    {
        var scene = EditorSceneManager.OpenScene(
            "Assets/Projeto/Assets/Scenes/MuseuHubScene.unity", OpenSceneMode.Single);

        // Salão principal do museu — estilo galeria
        MoveToScene(MakePlane("Floor_Hub", 24f, 16f, LoadMat("Floor_Stone"), Vector3.zero), scene);
        MoveToScene(MakeWall("Ceiling_Hub", new Vector3(0, 5, 0), new Vector3(24, 0.2f, 16), LoadMat("Ceiling_Stone")), scene);

        // Paredes
        MoveToScene(MakeWall("Wall_Back",   new Vector3(0, 2.5f, 8),  new Vector3(24, 5, 0.3f), LoadMat("Wall_Stone")), scene);
        MoveToScene(MakeWall("Wall_Front",  new Vector3(0, 2.5f, -8), new Vector3(24, 5, 0.3f), LoadMat("Wall_Stone")), scene);
        MoveToScene(MakeWall("Wall_Left",   new Vector3(-12, 2.5f, 0),new Vector3(0.3f, 5, 16), LoadMat("Wall_Stone")), scene);
        MoveToScene(MakeWall("Wall_Right",  new Vector3(12, 2.5f, 0), new Vector3(0.3f, 5, 16), LoadMat("Wall_Stone")), scene);

        // Colunas do museu extras
        Vector3[] colPos = {
            new Vector3(-5, 2f, 4), new Vector3(5, 2f, 4),
            new Vector3(-5, 2f,-4), new Vector3(5, 2f,-4),
            new Vector3(-8, 2f, 0), new Vector3(8, 2f, 0)
        };
        for (int i = 0; i < colPos.Length; i++)
            MoveToScene(MakeBox($"Pillar_{i}", colPos[i], new Vector3(0.5f, 4f, 0.5f), LoadMat("Pillar_Stone")), scene);

        // Vitrines laterais
        MoveToScene(MakeBox("Vitrine_L", new Vector3(-8, 1f, 5), new Vector3(1.5f, 2f, 1.5f), LoadMat("Glass_Panel")), scene);
        MoveToScene(MakeBox("Vitrine_R", new Vector3(8, 1f, 5), new Vector3(1.5f, 2f, 1.5f), LoadMat("Glass_Panel")), scene);

        // Estátuas decorativas
        MoveToScene(MakeBox("Statue_L", new Vector3(-10, 1.5f, -5), new Vector3(0.8f, 3f, 0.8f), LoadMat("Pillar_Stone")), scene);
        MoveToScene(MakeBox("Statue_R", new Vector3(10, 1.5f, -5), new Vector3(0.8f, 3f, 0.8f), LoadMat("Pillar_Stone")), scene);

        // Plantas decorativas
        MoveToScene(MakeBox("Plant_L", new Vector3(-11, 0.5f, 6), new Vector3(0.5f, 1f, 0.5f), LoadMat("Artifact_Bone")), scene);
        MoveToScene(MakeBox("Plant_R", new Vector3(11, 0.5f, 6), new Vector3(0.5f, 1f, 0.5f), LoadMat("Artifact_Bone")), scene);

        // 3 portais — arcos das eras
        BuildDoorPortal(scene, "Portal_PreHistoria",  new Vector3(-7f, 0, 5f), LoadMat("Wall_Cave"),  "Pré-História");
        BuildDoorPortal(scene, "Portal_IdadeMedia",   new Vector3( 0f, 0, 5f), LoadMat("Wall_Stone"), "Idade Média");
        BuildDoorPortal(scene, "Portal_FuturoTech",   new Vector3( 7f, 0, 5f), LoadMat("Wall_Tech"),  "Futuro Tech");

        // Vitrine central decorativa
        MoveToScene(MakeBox("Pedestal_Central", new Vector3(0, 0.5f, 0), new Vector3(1.5f, 1f, 1.5f), LoadMat("Pillar_Stone")), scene);
        MoveToScene(MakeBox("Vitrine_Glass",    new Vector3(0, 1.5f, 0), new Vector3(1.2f, 1f, 1.2f), LoadMat("Glass_Panel")), scene);

        SetupLighting(scene, new Color(1f, 0.97f, 0.88f), 1.4f, new Color(0.15f, 0.14f, 0.18f));

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    static void BuildDoorPortal(Scene scene, string name, Vector3 pos, Material mat, string label)
    {
        var parent = new GameObject(name);
        MoveToScene(parent, scene);
        parent.transform.position = pos;

        // Moldura da porta: 2 pilares + arco
        var pL  = MakeBox("PillarL", new Vector3(-0.85f, 1.5f, 0), new Vector3(0.3f, 3f, 0.3f), mat); pL.transform.SetParent(parent.transform);
        var pR  = MakeBox("PillarR", new Vector3( 0.85f, 1.5f, 0), new Vector3(0.3f, 3f, 0.3f), mat); pR.transform.SetParent(parent.transform);
        var arc = MakeBox("Arc",     new Vector3( 0,     3.15f,0), new Vector3(2.0f, 0.3f, 0.3f), mat); arc.transform.SetParent(parent.transform);

        // Portal escuro (passagem)
        var portal = MakeBox("Portal", new Vector3(0, 1.5f, 0), new Vector3(1.4f, 3f, 0.1f), LoadMat("Wall_Cave"));
        portal.transform.SetParent(parent.transform);
        var portalCollider = portal.GetComponent<Collider>();
        if (portalCollider != null)
            portalCollider.isTrigger = true;

        // MuseumDoor no portal
        int interLayer = LayerMask.NameToLayer("Interactable");
        portal.layer = interLayer < 0 ? 0 : interLayer;

        string destScene = label.Replace(" ", "").Replace("-", "") + "Scene";
        if (destScene == "Pré-HistóriaScene") destScene = "PreHistoriaScene";
        if (destScene == "IdadeMediaScene")   destScene = "IdadeMediaScene";
        if (destScene == "FuturoTechScene")   destScene = "FuturoTechScene";

        var door = portal.AddComponent<MuseumDoor>();
        door.nomeScene = destScene;

        // Label flutuante
        var labelGO = new GameObject("Label");
        labelGO.transform.SetParent(parent.transform);
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

    // ── Historical Scenes ──────────────────────────────────────────────────
    static void BuildHistoricalScene(string sceneName, EraStyle style)
    {
        var scene = EditorSceneManager.OpenScene(
            $"Assets/Projeto/Assets/Scenes/{sceneName}.unity", OpenSceneMode.Single);

        switch (style)
        {
            case EraStyle.PreHistoria: BuildCaveEnvironment(scene); break;
            case EraStyle.IdadeMedia:  BuildCastleEnvironment(scene); break;
            case EraStyle.FuturoTech:  BuildTechEnvironment(scene); break;
        }

        // Atualiza ExhibitManager já existente com o highlight material
        var em = FindInScene<ExhibitManager>(scene);
        if (em != null)
        {
            string hlPath = $"{PROJ}/Materials/Highlight_Interactable.mat";
            em.highlightMaterial = AssetDatabase.LoadAssetAtPath<Material>(hlPath);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    static void BuildCaveEnvironment(Scene scene)
    {
        // Caverna irregular simulada com planos inclinados
        MoveToScene(MakePlane("Floor_Cave", 28f, 22f, LoadMat("Floor_Dirt"), Vector3.zero), scene);
        MoveToScene(MakeWall("Ceiling_Cave", new Vector3(0, 4.5f, 0), new Vector3(28, 0.5f, 22), LoadMat("Ceiling_Cave")), scene);

        // Paredes de caverna
        MoveToScene(MakeWall("Wall_N",  new Vector3(0, 2.5f, 11),  new Vector3(28, 5, 0.5f), LoadMat("Wall_Cave")), scene);
        MoveToScene(MakeWall("Wall_S",  new Vector3(0, 2.5f,-11),  new Vector3(28, 5, 0.5f), LoadMat("Wall_Cave")), scene);
        MoveToScene(MakeWall("Wall_W",  new Vector3(-14,2.5f, 0),  new Vector3(0.5f, 5, 22), LoadMat("Wall_Cave")), scene);
        MoveToScene(MakeWall("Wall_E",  new Vector3( 14,2.5f, 0),  new Vector3(0.5f, 5, 22), LoadMat("Wall_Cave")), scene);

        // Rochas decorativas extras
        Vector3[] rocks = { new Vector3(-8,0.4f,5), new Vector3(8,0.6f,-5),
                            new Vector3(4,0.3f,7),  new Vector3(-6,0.5f,-4),
                            new Vector3(-10,0.8f,3), new Vector3(10,0.7f,-3),
                            new Vector3(0,0.5f,8), new Vector3(0,0.6f,-8) };
        float[] sizes = { 1.2f, 1.4f, 0.8f, 1.1f, 1.0f, 1.3f, 0.9f, 1.2f };
        for (int i = 0; i < rocks.Length; i++)
        {
            var rock = MakeBox($"Rock_{i}", rocks[i], Vector3.one * sizes[i], LoadMat("Wall_Cave"));
            rock.transform.rotation = Quaternion.Euler(Random.value * 25f, Random.value * 360f, Random.value * 20f);
            Object.DestroyImmediate(rock.GetComponent<Collider>());
            MoveToScene(rock, scene);
        }

        // Estalactites no teto
        for (int i = 0; i < 6; i++)
        {
            float x = (i - 2.5f) * 4f;
            var stalactite = MakeBox($"Stalactite_{i}", new Vector3(x, 4f, Random.Range(-8f, 8f)), new Vector3(0.3f, Random.Range(0.5f, 1.5f), 0.3f), LoadMat("Wall_Cave"));
            Object.DestroyImmediate(stalactite.GetComponent<Collider>());
            MoveToScene(stalactite, scene);
        }

        // Fogueira central (pedestal para o artefato do meio)
        MoveToScene(MakeBox("Pedestal_Fire", new Vector3(0, 0.3f, 0), new Vector3(0.8f, 0.6f, 0.8f), LoadMat("Artifact_Stone")), scene);

        // Cercas de pedra ao redor da fogueira
        for (int i = 0; i < 8; i++)
        {
            float angle = i * 45f * Mathf.Deg2Rad;
            var stone = MakeBox($"Stone_{i}", new Vector3(Mathf.Cos(angle) * 1.5f, 0.2f, Mathf.Sin(angle) * 1.5f), new Vector3(0.2f, 0.4f, 0.2f), LoadMat("Artifact_Stone"));
            Object.DestroyImmediate(stone.GetComponent<Collider>());
            MoveToScene(stone, scene);
        }

        // Pontos de luz vermelha/laranja (caverna) — mais luzes
        SetupLighting(scene, new Color(1f, 0.65f, 0.3f), 0.8f, new Color(0.05f, 0.03f, 0.02f));
        AddPointLight(scene, "Fire_Light", new Vector3(0, 1.2f, 0), new Color(1f, 0.4f, 0.1f), 8f, 3.5f);
        AddPointLight(scene, "Cave_Light1", new Vector3(-5f, 3f, 3f), new Color(0.9f, 0.8f, 0.6f), 4f, 2f);
        AddPointLight(scene, "Cave_Light2", new Vector3(5f, 3f, -3f), new Color(0.9f, 0.8f, 0.6f), 4f, 2f);
        AddPointLight(scene, "Cave_Light3", new Vector3(0, 3f, 8f), new Color(0.9f, 0.8f, 0.6f), 4f, 2f);
    }

    static void BuildCastleEnvironment(Scene scene)
    {
        // Salão de castelo medieval
        MoveToScene(MakePlane("Floor_Castle", 30f, 20f, LoadMat("Floor_Stone"), Vector3.zero), scene);
        MoveToScene(MakeWall("Ceiling_Castle", new Vector3(0, 6, 0), new Vector3(30, 0.3f, 20), LoadMat("Ceiling_Stone")), scene);

        MoveToScene(MakeWall("Wall_N", new Vector3(0, 3, 10),   new Vector3(30, 6, 0.4f), LoadMat("Wall_Stone")), scene);
        MoveToScene(MakeWall("Wall_S", new Vector3(0, 3, -10),  new Vector3(30, 6, 0.4f), LoadMat("Wall_Stone")), scene);
        MoveToScene(MakeWall("Wall_W", new Vector3(-15, 3, 0), new Vector3(0.4f, 6, 20), LoadMat("Wall_Stone")), scene);
        MoveToScene(MakeWall("Wall_E", new Vector3( 15, 3, 0), new Vector3(0.4f, 6, 20), LoadMat("Wall_Stone")), scene);

        // Colunas góticas extras
        Vector3[] cols = { new Vector3(-9,2.5f,5), new Vector3(9,2.5f,5),
                           new Vector3(-9,2.5f,-5), new Vector3(9,2.5f,-5),
                           new Vector3(-12,2.5f,0), new Vector3(12,2.5f,0) };
        foreach (var c in cols)
            MoveToScene(MakeBox($"Column_{c.x}_{c.z}", c, new Vector3(0.6f, 5f, 0.6f), LoadMat("Pillar_Stone")), scene);

        // Arcos entre colunas
        MoveToScene(MakeWall("Arch_L", new Vector3(-9, 5.3f, 0), new Vector3(0.5f, 0.5f, 10), LoadMat("Pillar_Stone")), scene);
        MoveToScene(MakeWall("Arch_R", new Vector3( 9, 5.3f, 0), new Vector3(0.5f, 0.5f, 10), LoadMat("Pillar_Stone")), scene);

        // Tapeçarias nas paredes
        MoveToScene(MakeBox("Tapestry_L", new Vector3(-14.8f, 3f, 0), new Vector3(0.05f, 4f, 8f), LoadMat("Artifact_Bone")), scene);
        MoveToScene(MakeBox("Tapestry_R", new Vector3(14.8f, 3f, 0), new Vector3(0.05f, 4f, 8f), LoadMat("Artifact_Bone")), scene);

        // Escudos decorativos nas paredes
        for (int i = 0; i < 4; i++)
        {
            float z = (i - 1.5f) * 3f;
            MoveToScene(MakeBox($"Shield_W_{i}", new Vector3(-14.9f, 3f, z), new Vector3(0.1f, 1f, 0.8f), LoadMat("Artifact_Metal")), scene);
            MoveToScene(MakeBox($"Shield_E_{i}", new Vector3(14.9f, 3f, z), new Vector3(0.1f, 1f, 0.8f), LoadMat("Artifact_Metal")), scene);
        }

        // Pedestal central com vitrine (trono)
        MoveToScene(MakeBox("Throne_Base",   new Vector3(0, 0.4f, 5), new Vector3(2.5f, 0.8f, 1.5f), LoadMat("Pillar_Stone")), scene);
        MoveToScene(MakeBox("Throne_Seat",   new Vector3(0, 1.2f, 5), new Vector3(1.5f, 0.5f, 1f), LoadMat("Artifact_Bone")), scene);

        // Mesas laterais
        MoveToScene(MakeBox("Table_L", new Vector3(-10, 0.8f, 7), new Vector3(2f, 0.2f, 1f), LoadMat("Artifact_Bone")), scene);
        MoveToScene(MakeBox("Table_R", new Vector3(10, 0.8f, 7), new Vector3(2f, 0.2f, 1f), LoadMat("Artifact_Bone")), scene);

        // Iluminação de castelo — velas extras
        SetupLighting(scene, new Color(0.95f, 0.88f, 0.70f), 0.9f, new Color(0.04f, 0.04f, 0.06f));
        AddPointLight(scene, "Candle_L", new Vector3(-6f, 2.5f, 0), new Color(1f, 0.7f, 0.3f), 6f, 2.5f);
        AddPointLight(scene, "Candle_R", new Vector3( 6f, 2.5f, 0), new Color(1f, 0.7f, 0.3f), 6f, 2.5f);
        AddPointLight(scene, "Torch_N", new Vector3(0, 4f, 8), new Color(1f, 0.7f, 0.3f), 5f, 2f);
        AddPointLight(scene, "Torch_S", new Vector3(0, 4f, -8), new Color(1f, 0.7f, 0.3f), 5f, 2f);
        AddPointLight(scene, "Window",   new Vector3(0, 4.5f, -6),  new Color(0.7f, 0.85f, 1f), 5f, 1.5f);
    }

    static void BuildTechEnvironment(Scene scene)
    {
        // Laboratório futurista
        MoveToScene(MakePlane("Floor_Tech", 30f, 20f, LoadMat("Floor_Tech"), Vector3.zero), scene);
        MoveToScene(MakeWall("Ceiling_Tech", new Vector3(0, 5, 0), new Vector3(30, 0.2f, 20), LoadMat("Wall_Tech")), scene);

        MoveToScene(MakeWall("Wall_N", new Vector3(0, 2.5f, 10),  new Vector3(30, 5, 0.3f), LoadMat("Wall_Tech")), scene);
        MoveToScene(MakeWall("Wall_S", new Vector3(0, 2.5f,-10),  new Vector3(30, 5, 0.3f), LoadMat("Wall_Tech")), scene);
        MoveToScene(MakeWall("Wall_W", new Vector3(-15,2.5f,0),  new Vector3(0.3f, 5, 20), LoadMat("Wall_Tech")), scene);
        MoveToScene(MakeWall("Wall_E", new Vector3( 15,2.5f,0),  new Vector3(0.3f, 5, 20), LoadMat("Wall_Tech")), scene);

        // Painéis de vidro nas paredes extras
        MoveToScene(MakeWall("Glass_N1", new Vector3(-6, 2.5f, 9.8f), new Vector3(4, 3.5f, 0.05f), LoadMat("Glass_Panel")), scene);
        MoveToScene(MakeWall("Glass_N2", new Vector3( 6, 2.5f, 9.8f), new Vector3(4, 3.5f, 0.05f), LoadMat("Glass_Panel")), scene);
        MoveToScene(MakeWall("Glass_S1", new Vector3(-6, 2.5f, -9.8f), new Vector3(4, 3.5f, 0.05f), LoadMat("Glass_Panel")), scene);
        MoveToScene(MakeWall("Glass_S2", new Vector3( 6, 2.5f, -9.8f), new Vector3(4, 3.5f, 0.05f), LoadMat("Glass_Panel")), scene);

        // Consoles/mesas de trabalho extras
        Vector3[] desks = { new Vector3(-8, 0.5f, 4), new Vector3(8, 0.5f, 4),
                            new Vector3(-8, 0.5f,-4), new Vector3(8, 0.5f,-4),
                            new Vector3(0, 0.5f, 6), new Vector3(0, 0.5f, -6) };
        foreach (var d in desks)
            MoveToScene(MakeBox($"Console_{d.x}_{d.z}", d, new Vector3(2.5f, 1f, 1f), LoadMat("Artifact_Metal")), scene);

        // Plataforma central elevada
        MoveToScene(MakeBox("Platform", new Vector3(0, 0.15f, 0), new Vector3(4f, 0.3f, 4f), LoadMat("Artifact_Metal")), scene);

        // Hologramas flutuantes
        for (int i = 0; i < 3; i++)
        {
            var hologram = MakeBox($"Hologram_{i}", new Vector3((i-1)*4f, 2f, 0), new Vector3(0.5f, 1f, 0.5f), LoadMat("Artifact_Glow"));
            Object.DestroyImmediate(hologram.GetComponent<Collider>());
            MoveToScene(hologram, scene);
        }

        // Luzes neon nas paredes
        MoveToScene(MakeBox("Neon_L", new Vector3(-14.9f, 2.5f, 0), new Vector3(0.1f, 4f, 18f), LoadMat("Artifact_Glow")), scene);
        MoveToScene(MakeBox("Neon_R", new Vector3(14.9f, 2.5f, 0), new Vector3(0.1f, 4f, 18f), LoadMat("Artifact_Glow")), scene);

        // Iluminação tech — fria e azulada, mais luzes
        SetupLighting(scene, new Color(0.75f, 0.88f, 1f), 0.7f, new Color(0.02f, 0.04f, 0.08f));
        AddPointLight(scene, "Panel_L",   new Vector3(-8f, 4f, 0), new Color(0.4f, 0.7f, 1f), 8f, 1.5f);
        AddPointLight(scene, "Panel_R",   new Vector3( 8f, 4f, 0), new Color(0.4f, 0.7f, 1f), 8f, 1.5f);
        AddPointLight(scene, "HologramGlow", new Vector3(0, 2f, 0), new Color(0.1f, 1f, 0.8f), 5f, 3f);
        AddPointLight(scene, "Ceiling_Light", new Vector3(0, 4.5f, 0), new Color(0.8f, 0.9f, 1f), 10f, 2f);
    }

    // ══════════════════════════════════════════════════════════════════════
    // PRIMITIVES HELPERS
    // ══════════════════════════════════════════════════════════════════════

    static GameObject MakePlane(string name, float w, float d, Material mat, Vector3 pos)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Plane);
        go.name = name;
        go.transform.position = pos;
        go.transform.localScale = new Vector3(w * 0.1f, 1, d * 0.1f);
        if (mat != null) go.GetComponent<Renderer>().sharedMaterial = mat;
        return go;
    }

    static GameObject MakeWall(string name, Vector3 pos, Vector3 scale, Material mat)
    {
        return MakeBox(name, pos, scale, mat);
    }

    static GameObject MakeBox(string name, Vector3 pos, Vector3 scale, Material mat)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.position = pos;
        go.transform.localScale = scale;
        if (mat != null) go.GetComponent<Renderer>().sharedMaterial = mat;
        return go;
    }

    static void MoveToScene(GameObject go, Scene scene)
    {
        SceneManager.MoveGameObjectToScene(go, scene);
    }

    static Material LoadMat(string name)
    {
        string path = $"{PROJ}/Materials/Environment/{name}.mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            // Fallback para o material default do VR template
            mat = AssetDatabase.LoadAssetAtPath<Material>(
                AssetDatabase.GUIDToAssetPath(GUID_MAT_CONCRETE));
        }
        return mat;
    }

    static void SetupLighting(Scene scene, Color sunColor, float intensity, Color ambientColor)
    {
        foreach (var go in scene.GetRootGameObjects())
        {
            var light = go.GetComponent<Light>();
            if (light != null && light.type == LightType.Directional)
            {
                light.color     = sunColor;
                light.intensity = intensity;
                break;
            }
        }
        RenderSettings.ambientLight = ambientColor;
        RenderSettings.ambientMode  = UnityEngine.Rendering.AmbientMode.Flat;
    }

    static void AddPointLight(Scene scene, string name, Vector3 pos, Color color, float range, float intensity)
    {
        var go    = new GameObject(name);
        var light = go.AddComponent<Light>();
        light.type      = LightType.Point;
        light.color     = color;
        light.range     = range;
        light.intensity = intensity;
        go.transform.position = pos;
        MoveToScene(go, scene);
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

    static void SavePrefabAsset(GameObject go, string path)
    {
        string dir = System.IO.Path.GetDirectoryName(path);
        if (!System.IO.Directory.Exists(dir))
            System.IO.Directory.CreateDirectory(dir);
        PrefabUtility.SaveAsPrefabAsset(go, path);
    }

    static void Progress(string msg, float p) =>
        EditorUtility.DisplayProgressBar("ChronoMundi Assets", msg, p);
}
#endif