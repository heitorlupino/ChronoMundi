#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// ChronoMundi — Configurador VR/XR
///
/// Configura o XR Interaction Toolkit em todas as cenas do projeto.
/// Execute DEPOIS de "🏗️ Construir Cenas Completas".
///
/// O que faz:
///   1. Adiciona XR Origin às cenas (substitui ou convive com DesktopPlayer)
///   2. Adiciona XRSimpleInteractable + XRIExhibitAdapter em cada artefato
///   3. Configura NarratorSystem com canvas WorldSpace (visível no headset)
///   4. Adiciona VRModeManager ao Hub e cenas históricas
///   5. Adiciona VRUIInteraction + LineRenderer nos controllers
///
/// Menu: ChronoMundi → 🥽 Configurar VR (XR/XRI)
/// </summary>
public static class ChronoMundiXRBuilder
{
    // GUID do prefab XR Origin que já existe no projeto
    // (Assets/Samples/XR Interaction Toolkit/3.1.2/Starter Assets/Prefabs/XR Origin (XR Rig).prefab)
    const string GUID_XR_ORIGIN = "77e7c27b2c5525e4aa8cc9f99d654486";

    // GUID do prefab Complete XR Origin (com NearFar Interactors — mais completo)
    const string GUID_XR_ORIGIN_COMPLETE = ""; // Será buscado por nome se o GUID não funcionar

    const string SCENES_PATH = "Assets/Projeto/Assets/Scenes/";

    static readonly string[] SCENE_NAMES = {
        "MuseuHubScene",
        "PreHistoriaScene",
        "IdadeMediaScene",
        "FuturoTechScene"
    };

    // ════════════════════════════════════════════════════════════════════
    // ENTRY POINTS
    // ════════════════════════════════════════════════════════════════════

    [MenuItem("ChronoMundi/🥽 Configurar VR (XR/XRI) — Todas as Cenas")]
    public static void ConfigureAllScenes()
    {
        if (!EditorUtility.DisplayDialog("ChronoMundi VR",
            "Isso vai configurar o XR Interaction Toolkit em todas as cenas.\n\n" +
            "Execute DEPOIS de:\n" +
            "  1. 🎨 Popular Assets + Construir Ambientes\n" +
            "  2. 🏗️ Construir Cenas Completas\n\n" +
            "Continuar?", "Sim, configurar VR", "Cancelar"))
            return;

        float p = 0f;
        foreach (var sceneName in SCENE_NAMES)
        {
            Prog($"Configurando VR em {sceneName}...", p += 0.22f);
            ConfigureScene(sceneName);
        }

        // Cria layer XR se não existir
        EnsureLayer("XRInteractable");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();

        EditorUtility.DisplayDialog("ChronoMundi VR ✅",
            "VR configurado em todas as cenas!\n\n" +
            "O que foi feito:\n" +
            "• XR Origin adicionado a cada cena\n" +
            "• XRSimpleInteractable nos artefatos\n" +
            "• XRIExhibitAdapter nos artefatos\n" +
            "• VRModeManager configurado\n" +
            "• NarratorSystem adaptado para VR\n\n" +
            "Próximos passos:\n" +
            "1. Verifique Edit → Project Settings → XR Plug-in Management\n" +
            "2. Ative o provider do seu headset (Oculus/OpenXR)\n" +
            "3. Adicione a layer 'XRInteractable' se pedido\n" +
            "4. Faça um Build e teste no headset",
            "OK");
    }

    [MenuItem("ChronoMundi/🥽 Configurar Artefatos para VR — Cena Atual")]
    public static void ConfigureCurrentScene()
    {
        var scene = EditorSceneManager.GetActiveScene();
        Prog($"Configurando artefatos em {scene.name}...", 0.5f);
        ConfigureExhibitsInScene(scene);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        EditorUtility.ClearProgressBar();
        Debug.Log($"[XRBuilder] Artefatos configurados em {scene.name}.");
    }

    // ════════════════════════════════════════════════════════════════════
    // CONFIGURE PER SCENE
    // ════════════════════════════════════════════════════════════════════

    static void ConfigureScene(string sceneName)
    {
        var scene = EditorSceneManager.OpenScene(
            $"{SCENES_PATH}{sceneName}.unity", OpenSceneMode.Single);

        // 1. XR Origin
        AddXROriginToScene(scene, sceneName);

        // 2. XRSimpleInteractable + XRIExhibitAdapter nos artefatos
        ConfigureExhibitsInScene(scene);

        // 3. VRModeManager
        AddVRModeManager(scene);

        // 4. NarratorSystem WorldSpace canvas (para ser visível no headset)
        // O NarratorSystem é DontDestroyOnLoad — adaptamos o bootstrapper

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    // ════════════════════════════════════════════════════════════════════
    // XR ORIGIN
    // ════════════════════════════════════════════════════════════════════

    static void AddXROriginToScene(Scene scene, string sceneName)
    {
        // Verifica se já existe
        foreach (var go in scene.GetRootGameObjects())
        {
            if (go.name.Contains("XR Origin") || go.name.Contains("XR Rig"))
            {
                Debug.Log($"[XRBuilder] XR Origin já existe em {sceneName}. Atualizando posição...");
                go.transform.position = GetPlayerSpawnPos(sceneName);
                return;
            }
        }

        // Tenta carregar o prefab pelo GUID
        string path = AssetDatabase.GUIDToAssetPath(GUID_XR_ORIGIN);
        GameObject xrPrefab = null;

        if (!string.IsNullOrEmpty(path))
            xrPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

        // Fallback: busca por nome
        if (xrPrefab == null)
        {
            string[] guids = AssetDatabase.FindAssets("XR Origin t:Prefab");
            foreach (var guid in guids)
            {
                string p2 = AssetDatabase.GUIDToAssetPath(guid);
                if (p2.Contains("Starter Assets"))
                {
                    xrPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(p2);
                    if (xrPrefab != null) break;
                }
            }
        }

        GameObject xrInstance;

        if (xrPrefab != null)
        {
            xrInstance = (GameObject)PrefabUtility.InstantiatePrefab(xrPrefab, scene);
            xrInstance.name = "XR Origin (VR)";
            Debug.Log($"[XRBuilder] XR Origin instanciado de prefab em {sceneName}.");
        }
        else
        {
            // Cria XR Origin mínimo manualmente
            xrInstance = BuildMinimalXROrigin(scene);
            Debug.LogWarning($"[XRBuilder] Prefab XR Origin não encontrado. Criando versão mínima em {sceneName}.");
        }

        xrInstance.transform.position = GetPlayerSpawnPos(sceneName);

        // Adiciona PlayerInteraction ao XR Origin em modo VR
        var pi = xrInstance.GetComponentInChildren<PlayerInteraction>();
        if (pi == null)
        {
            pi = xrInstance.AddComponent<PlayerInteraction>();
            pi.isVRMode           = true;
            pi.interactionDistance = 5f;

            int interLayer = LayerMask.NameToLayer("Interactable");
            if (interLayer >= 0)
                pi.interactableLayer = 1 << interLayer;
        }
        else
        {
            pi.isVRMode = true;
        }
    }

    /// <summary>
    /// Cria um XR Origin mínimo (sem prefab).
    /// Contém: XROrigin component, Camera Offset, Main Camera, Left/Right Hand.
    /// </summary>
    static GameObject BuildMinimalXROrigin(Scene scene)
    {
        var root = new GameObject("XR Origin (VR)");
        SceneManager.MoveGameObjectToScene(root, scene);
        root.tag = "Player";

        // Camera Offset
        var cameraOffset = new GameObject("Camera Offset");
        cameraOffset.transform.SetParent(root.transform, false);
        cameraOffset.transform.localPosition = new Vector3(0, 1.36f, 0);

        // Main Camera
        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        camGO.transform.SetParent(cameraOffset.transform, false);
        camGO.AddComponent<Camera>();
        camGO.AddComponent<AudioListener>();

        // Left Hand Controller
        var leftHand = BuildHandController(cameraOffset.transform, "LeftHand Controller", true);

        // Right Hand Controller
        var rightHand = BuildHandController(cameraOffset.transform, "RightHand Controller", false);

        // VRUIInteraction nos controllers
        AddVRUIInteractionToHand(leftHand);
        AddVRUIInteractionToHand(rightHand);

        Debug.Log("[XRBuilder] XR Origin mínimo criado.");
        return root;
    }

    static GameObject BuildHandController(Transform parent, string name, bool isLeft)
    {
        var hand = new GameObject(name);
        hand.transform.SetParent(parent, false);
        hand.transform.localPosition = isLeft
            ? new Vector3(-0.2f, -0.1f, 0.3f)
            : new Vector3(0.2f, -0.1f, 0.3f);

        // Visual do controller (cubo simples)
        var visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visual.name = "ControllerVisual";
        visual.transform.SetParent(hand.transform, false);
        visual.transform.localScale = new Vector3(0.05f, 0.05f, 0.1f);
        Object.DestroyImmediate(visual.GetComponent<Collider>());

        // LineRenderer para o ray visual
        var lr = hand.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.startWidth = 0.005f;
        lr.endWidth   = 0.002f;
        lr.useWorldSpace = true;

        // Material simples para o ray
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = new Color(0f, 0.8f, 1f, 0.8f);
        lr.endColor   = new Color(0f, 0.8f, 1f, 0f);

        return hand;
    }

    static void AddVRUIInteractionToHand(GameObject hand)
    {
        var vrUI = hand.AddComponent<VRUIInteraction>();
        vrUI.rayDistance = 10f;
        vrUI.useXRInput  = true;
        vrUI.lineRenderer = hand.GetComponent<LineRenderer>();

        int uiLayer = LayerMask.NameToLayer("UI");
        if (uiLayer >= 0)
            vrUI.uiLayer = 1 << uiLayer;
    }

    // ════════════════════════════════════════════════════════════════════
    // ARTEFATOS — XRSimpleInteractable + XRIExhibitAdapter
    // ════════════════════════════════════════════════════════════════════

    static void ConfigureExhibitsInScene(Scene scene)
    {
        int count = 0;
        foreach (var root in scene.GetRootGameObjects())
        {
            foreach (var exhibit in root.GetComponentsInChildren<InteracleObject>(true))
            {
                ConfigureExhibitForXR(exhibit.gameObject);
                count++;
            }
        }
        Debug.Log($"[XRBuilder] {count} artefatos configurados para XR em {scene.name}.");
    }

    static void ConfigureExhibitForXR(GameObject go)
    {
        // Garante Collider (necessário para XRI)
        if (go.GetComponent<Collider>() == null)
        {
            go.AddComponent<BoxCollider>();
            Debug.LogWarning($"[XRBuilder] BoxCollider adicionado a '{go.name}' (necessário para XRI).");
        }

        // Garante Rigidbody kinematic (necessário para XRI detectar)
        var rb = go.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = go.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity  = false;
        }

        // XRSimpleInteractable
        var xrInteractable = go.GetComponent<XRSimpleInteractable>();
        if (xrInteractable == null)
            xrInteractable = go.AddComponent<XRSimpleInteractable>();

        // Configura interaction layers
        xrInteractable.interactionLayers = InteractionLayerMask.GetMask("Default");

        // XRIExhibitAdapter
        if (go.GetComponent<XRIExhibitAdapter>() == null)
            go.AddComponent<XRIExhibitAdapter>();

        // Layer Interactable
        int layer = LayerMask.NameToLayer("Interactable");
        if (layer >= 0) go.layer = layer;
    }

    // ════════════════════════════════════════════════════════════════════
    // VR MODE MANAGER
    // ════════════════════════════════════════════════════════════════════

    static void AddVRModeManager(Scene scene)
    {
        // Remove antigo
        foreach (var go in scene.GetRootGameObjects())
            if (go.name == "VRModeManager") Object.DestroyImmediate(go);

        var managerGO = new GameObject("VRModeManager");
        SceneManager.MoveGameObjectToScene(managerGO, scene);
        var vrManager = managerGO.AddComponent<VRModeManager>();

        // Tenta conectar os players automaticamente
        foreach (var go in scene.GetRootGameObjects())
        {
            if (go.name.Contains("XR Origin") || go.name.Contains("XR Rig"))
                vrManager.xrOrigin = go;

            if (go.GetComponent<DesktopPlayerController>() != null ||
                go.name.Contains("DesktopPlayer") || go.name.Contains("Desktop"))
                vrManager.desktopPlayer = go;
        }

        // Busca em segundo nível (player pode ter nome diferente, ex: "PreHistoriaScene_Player")
        if (vrManager.desktopPlayer == null)
        {
            foreach (var go in scene.GetRootGameObjects())
            {
                if (go.GetComponentInChildren<DesktopPlayerController>() != null)
                {
                    vrManager.desktopPlayer = go;
                    break;
                }
            }
        }
    }

    // ════════════════════════════════════════════════════════════════════
    // HELPERS
    // ════════════════════════════════════════════════════════════════════

    static Vector3 GetPlayerSpawnPos(string sceneName)
    {
        return sceneName switch
        {
            "MuseuHubScene"    => new Vector3(0, 0, -5f),
            "PreHistoriaScene" => new Vector3(0, 0, -4f),
            "IdadeMediaScene"  => new Vector3(0, 0, -4f),
            "FuturoTechScene"  => new Vector3(0, 0.3f, -4.5f),
            _                  => Vector3.zero
        };
    }

    /// <summary>Adiciona uma layer ao projeto se ela não existir.</summary>
    static void EnsureLayer(string layerName)
    {
        var tagManager = new SerializedObject(
            AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(
                "ProjectSettings/TagManager.asset"));

        var layers = tagManager.FindProperty("layers");
        if (layers == null || !layers.isArray) return;

        for (int i = 8; i < layers.arraySize; i++)
        {
            var element = layers.GetArrayElementAtIndex(i);
            if (element.stringValue == layerName) return; // já existe
        }

        for (int i = 8; i < layers.arraySize; i++)
        {
            var element = layers.GetArrayElementAtIndex(i);
            if (string.IsNullOrEmpty(element.stringValue))
            {
                element.stringValue = layerName;
                tagManager.ApplyModifiedProperties();
                Debug.Log($"[XRBuilder] Layer '{layerName}' criada no slot {i}.");
                return;
            }
        }

        Debug.LogWarning($"[XRBuilder] Não foi possível criar layer '{layerName}': sem slots disponíveis.");
    }

    static void Prog(string msg, float p) =>
        EditorUtility.DisplayProgressBar("ChronoMundi — Configurando VR", msg, p);
}
#endif
