#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.XR.Management;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features.Interactions;
using System.Linq;

/// <summary>
/// Ativa o OpenXR loader no XR Plug-in Management e injeta o
/// XR Origin completo (com Ray + Teleport Interactors) em todas as cenas.
///
/// O que faz:
///   1. Habilita m_AutomaticLoading e m_AutomaticRunning no XRGeneralSettings
///      para Standalone (PC) e Android (Quest)
///   2. Garante que o OpenXR Loader está na lista de loaders de ambas as plataformas
///   3. Injeta o prefab "Complete XR Origin Set Up Variant" nas 4 cenas do jogo
///   4. Conecta VRModeManager ao XR Origin e ao DesktopPlayer de cada cena
///
/// Pré-requisito: copie os scripts VR (VRModeManager.cs, XRIExhibitAdapter.cs,
///   XRNarratorAdapter.cs) para Assets/Projeto/Assets/Scripts/ antes de rodar.
///
/// Menu: ChronoMundi → 🥽 ATIVAR VR (OpenXR + XR Origin) — Setup Completo
/// </summary>
public static class ChronoMundiXRSetup
{
    // Prefab completo que vem com o VR Template — tem Ray, Teleport e Poke Interactors
    const string XR_ORIGIN_PREFAB_PATH =
        "Assets/VRTemplateAssets/Prefabs/Setup/Complete XR Origin Set Up Variant.prefab";

    // Fallback: prefab dos Starter Assets
    const string XR_ORIGIN_FALLBACK_PATH =
        "Assets/Samples/XR Interaction Toolkit/3.1.2/Starter Assets/Prefabs/XR Origin (XR Rig).prefab";

    const string SCENES_PATH = "Assets/Projeto/Assets/Scenes/";

    static readonly string[] SCENE_NAMES =
        { "MuseuHubScene", "PreHistoriaScene", "IdadeMediaScene", "FuturoTechScene" };

    static readonly Dictionary<string, Vector3> SPAWN_POSITIONS = new()
    {
        { "MuseuHubScene",    new Vector3(0f,  0f,   -5f) },
        { "PreHistoriaScene", new Vector3(0f,  0f,   -4f) },
        { "IdadeMediaScene",  new Vector3(0f,  0f,   -4f) },
        { "FuturoTechScene",  new Vector3(0f,  0.3f, -4.5f) },
    };

    // ════════════════════════════════════════════════════════════════════
    [MenuItem("ChronoMundi/🥽 ATIVAR VR (OpenXR + XR Origin) — Setup Completo")]
    public static void FullSetup()
    {
        if (!EditorUtility.DisplayDialog("ChronoMundi — Ativar VR",
            "Isso vai:\n\n" +
            "1. Ativar OpenXR Loader (PC + Android/Quest)\n" +
            "2. Injetar XR Origin completo nas 4 cenas\n" +
            "3. Conectar VRModeManager em cada cena\n\n" +
            "Pré-requisito: scripts VR já copiados para Assets/\n\n" +
            "Continuar?", "Ativar VR", "Cancelar"))
            return;

        // ── Passo 1: OpenXR Loader ────────────────────────────────────────
        Prog("Ativando OpenXR Loader...", 0.1f);
        bool loaderOK = ActivateOpenXRLoader();

        // ── Passo 2: XR Origin nas cenas ──────────────────────────────────
        var prefab = LoadXROriginPrefab();
        if (prefab == null)
        {
            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("Erro",
                "Prefab XR Origin não encontrado.\n\n" +
                "Esperado em:\n" + XR_ORIGIN_PREFAB_PATH +
                "\n\nOu:\n" + XR_ORIGIN_FALLBACK_PATH, "OK");
            return;
        }

        int injected = 0;
        float p = 0.2f;
        foreach (var sceneName in SCENE_NAMES)
        {
            Prog($"Injetando XR Origin em {sceneName}...", p += 0.18f);
            if (InjectXROriginInScene(sceneName, prefab))
                injected++;
        }

        // ── Passo 3: Salva settings ───────────────────────────────────────
        Prog("Salvando configurações...", 0.98f);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();

        // ── Relatório ─────────────────────────────────────────────────────
        EditorUtility.DisplayDialog("ChronoMundi ✅ VR Ativado",
            $"Setup concluído!\n\n" +
            $"✔ OpenXR Loader: {(loaderOK ? "ativado" : "já estava ativo")}\n" +
            $"✔ XR Origin injetado em {injected}/{SCENE_NAMES.Length} cenas\n\n" +
            "Próximos passos:\n" +
            "1. Edit → Project Settings → XR Plug-in Management\n" +
            "   Confirme ✔ OpenXR em PC e Android\n" +
            "2. Na aba OpenXR → adicione 'Meta Quest Support'\n" +
            "   e 'Oculus Touch Controller Profile'\n" +
            "3. Execute o restante dos builders:\n" +
            "   🔧 CORRIGIR TUDO\n" +
            "   🏛️ Room Builders\n" +
            "   🧙 Adicionar Personagem Guia\n" +
            "4. File → Build Settings → Android → Build",
            "OK");
    }

    // ════════════════════════════════════════════════════════════════════
    // PASSO 1 — Ativa OpenXR Loader via XRGeneralSettings API
    // ════════════════════════════════════════════════════════════════════

    static bool ActivateOpenXRLoader()
    {
        bool changed = false;

        // API do XR Management 4.x: XRGeneralSettingsPerBuildTarget
        var buildTargetSettings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(
            BuildTargetGroup.Standalone);
        changed |= EnableLoader(buildTargetSettings, "Standalone (PC)");

        var androidSettings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(
            BuildTargetGroup.Android);
        changed |= EnableLoader(androidSettings, "Android (Quest)");

        // Persiste as mudanças
        AssetDatabase.SaveAssets();
        return changed;
    }

    static bool EnableLoader(XRGeneralSettings settings, string platform)
    {
        if (settings == null)
        {
            Debug.LogWarning($"[XRSetup] XRGeneralSettings para {platform} não encontrado. " +
                             "Abra Edit → Project Settings → XR Plug-in Management uma vez " +
                             "para que o Unity crie os assets necessários.");
            return false;
        }

        var manager = settings.Manager;
        if (manager == null)
        {
            Debug.LogWarning($"[XRSetup] XRManagerSettings para {platform} é null.");
            return false;
        }

        // Verifica se OpenXR já está na lista
        bool alreadyHasOpenXR = manager.activeLoaders.Any(l => l is OpenXRLoader);
        if (alreadyHasOpenXR)
        {
            Debug.Log($"[XRSetup] OpenXR já está ativo em {platform}.");
            return false;
        }

        // Procura o OpenXR Loader asset
        var openXRLoaderGUIDs = AssetDatabase.FindAssets("t:OpenXRLoader");
        if (openXRLoaderGUIDs.Length == 0)
        {
            // Tenta pelo path direto
            openXRLoaderGUIDs = new[] {
                AssetDatabase.AssetPathToGUID("Assets/XR/Loaders/Open XR Loader.asset")
            };
        }

        foreach (var guid in openXRLoaderGUIDs)
        {
            string path   = AssetDatabase.GUIDToAssetPath(guid);
            var    loader = AssetDatabase.LoadAssetAtPath<XRLoader>(path);
            if (loader == null) continue;

            // Adiciona via API serializada para garantir persistência
            var serializedManager = new SerializedObject(manager);
            var loadersList       = serializedManager.FindProperty("m_Loaders");

            if (loadersList != null)
            {
                bool found = false;
                for (int i = 0; i < loadersList.arraySize; i++)
                    if (loadersList.GetArrayElementAtIndex(i).objectReferenceValue == loader)
                    { found = true; break; }

                if (!found)
                {
                    loadersList.arraySize++;
                    loadersList.GetArrayElementAtIndex(loadersList.arraySize - 1)
                        .objectReferenceValue = loader;
                    serializedManager.ApplyModifiedProperties();
                    EditorUtility.SetDirty(manager);
                    Debug.Log($"[XRSetup] OpenXR Loader adicionado a {platform}.");
                }
            }

            // Ativa AutomaticLoading e AutomaticRunning
            var serializedSettings = new SerializedObject(settings);
            var autoLoad = serializedSettings.FindProperty("m_AutomaticLoading");
            var autoRun  = serializedSettings.FindProperty("m_AutomaticRunning");
            if (autoLoad != null) autoLoad.boolValue = true;
            if (autoRun  != null) autoRun.boolValue  = true;
            serializedSettings.ApplyModifiedProperties();
            EditorUtility.SetDirty(settings);

            Debug.Log($"[XRSetup] AutomaticLoading + AutomaticRunning ativados para {platform}.");
            return true;
        }

        Debug.LogWarning($"[XRSetup] Não foi possível localizar o OpenXR Loader asset para {platform}.");
        return false;
    }

    // ════════════════════════════════════════════════════════════════════
    // PASSO 2 — Injeta XR Origin nas cenas
    // ════════════════════════════════════════════════════════════════════

    static GameObject LoadXROriginPrefab()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(XR_ORIGIN_PREFAB_PATH);
        if (prefab != null) return prefab;

        prefab = AssetDatabase.LoadAssetAtPath<GameObject>(XR_ORIGIN_FALLBACK_PATH);
        if (prefab != null)
        {
            Debug.Log("[XRSetup] Usando XR Origin fallback (Starter Assets).");
            return prefab;
        }

        // Busca qualquer prefab com "XR Origin" no nome
        var guids = AssetDatabase.FindAssets("XR Origin t:Prefab");
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                Debug.Log($"[XRSetup] XR Origin encontrado via busca: {path}");
                return prefab;
            }
        }

        return null;
    }

    static bool InjectXROriginInScene(string sceneName, GameObject xrPrefab)
    {
        string path = SCENES_PATH + sceneName + ".unity";
        if (!System.IO.File.Exists(path))
        {
            Debug.LogWarning($"[XRSetup] Cena não encontrada: {path}");
            return false;
        }

        var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

        // Remove XR Origin antigo se existir
        foreach (var root in scene.GetRootGameObjects())
        {
            if (root.name.StartsWith("XR Origin") ||
                root.name.StartsWith("Complete XR Origin") ||
                root.name == "XR Rig")
            {
                Object.DestroyImmediate(root);
                Debug.Log($"[XRSetup] XR Origin antigo removido de {sceneName}.");
                break;
            }
        }

        // Instancia o prefab
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(xrPrefab, scene);
        instance.name = "XR Origin (VR)";
        instance.transform.position = SPAWN_POSITIONS.TryGetValue(sceneName, out var pos)
            ? pos : Vector3.zero;
        instance.tag = "Player";

        // Adiciona PlayerInteraction ao XR Origin
        EnsurePlayerInteraction(instance, sceneName);

        // Conecta VRModeManager
        ConnectVRModeManager(scene, instance);

        EditorUtility.SetDirty(instance);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log($"[XRSetup] XR Origin injetado em {sceneName}.");
        return true;
    }

    static void EnsurePlayerInteraction(GameObject xrOrigin, string sceneName)
    {
        // PlayerInteraction já existe no XR Origin?
        if (xrOrigin.GetComponentInChildren<PlayerInteraction>() != null) return;

        var pi = xrOrigin.AddComponent<PlayerInteraction>();
        pi.isVRMode            = true;
        pi.interactionDistance = sceneName == "MuseuHubScene" ? 4f : 3f;
        pi.interactableLayer   = ~0;

        // playerCamera é [SerializeField] private — usa SerializedObject para atribuir
        var cam = xrOrigin.GetComponentInChildren<Camera>();
        if (cam != null)
        {
            var so   = new SerializedObject(pi);
            var prop = so.FindProperty("playerCamera");
            if (prop != null)
            {
                prop.objectReferenceValue = cam;
                so.ApplyModifiedProperties();
            }
        }
    }

    static void ConnectVRModeManager(UnityEngine.SceneManagement.Scene scene,
        GameObject xrOrigin)
    {
        // Busca VRModeManager existente na cena
        VRModeManager vmm = null;
        foreach (var root in scene.GetRootGameObjects())
        {
            vmm = root.GetComponentInChildren<VRModeManager>(true);
            if (vmm != null) break;
        }

        // Cria se não existir
        if (vmm == null)
        {
            var go = new GameObject("VRModeManager");
            SceneManager.MoveGameObjectToScene(go, scene);
            vmm = go.AddComponent<VRModeManager>();
        }

        vmm.xrOrigin = xrOrigin;

        // Tenta conectar DesktopPlayer
        foreach (var root in scene.GetRootGameObjects())
        {
            if (root == xrOrigin) continue;
            var dpc = root.GetComponentInChildren<DesktopPlayerController>(true);
            if (dpc != null)
            {
                vmm.desktopPlayer = dpc.gameObject;
                break;
            }
        }

        EditorUtility.SetDirty(vmm.gameObject);
    }

    // ════════════════════════════════════════════════════════════════════

    static void Prog(string msg, float p) =>
        EditorUtility.DisplayProgressBar("ChronoMundi — Ativar VR", msg, p);
}
#endif