#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

/// <summary>
/// Adiciona chão de teleporte VR em todas as cenas históricas.
/// Executado automaticamente pelo ChronoMundiXRBuilder, mas também
/// pode ser chamado manualmente.
///
/// Menu: ChronoMundi → 🥽 Adicionar Chão de Teleporte VR
/// </summary>
public static class XRFloorBuilder
{
    const string SCENES_PATH = "Assets/Projeto/Assets/Scenes/";

    // Tamanhos dos chãos por cena (ajuste conforme o ambiente)
    // (centerX, centerZ, sizeX, sizeZ)
    static readonly System.Collections.Generic.Dictionary<string, (float cx, float cz, float sx, float sz)> FLOOR_CONFIGS
        = new()
        {
            { "MuseuHubScene",    (0f, 0f, 20f, 20f) },
            { "PreHistoriaScene", (0f, 0f, 16f, 16f) },
            { "IdadeMediaScene",  (0f, 0f, 16f, 16f) },
            { "FuturoTechScene",  (0f, 0f, 18f, 18f) }
        };

    [MenuItem("ChronoMundi/🥽 Adicionar Chão de Teleporte VR — Todas as Cenas")]
    public static void AddTeleportFloorToAllScenes()
    {
        foreach (var kvp in FLOOR_CONFIGS)
        {
            var scene = EditorSceneManager.OpenScene(
                $"{SCENES_PATH}{kvp.Key}.unity", OpenSceneMode.Single);

            AddTeleportFloor(scene, kvp.Key, kvp.Value.cx, kvp.Value.cz, kvp.Value.sx, kvp.Value.sz);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("ChronoMundi VR",
            "Chões de teleporte adicionados a todas as cenas!\n\n" +
            "O jogador VR pode agora apontar e teleportar pelo museu.",
            "OK");
    }

    [MenuItem("ChronoMundi/🥽 Adicionar Chão de Teleporte VR — Cena Atual")]
    public static void AddTeleportFloorToCurrentScene()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (!FLOOR_CONFIGS.TryGetValue(scene.name, out var cfg))
            cfg = (0f, 0f, 16f, 16f); // fallback

        AddTeleportFloor(scene, scene.name, cfg.cx, cfg.cz, cfg.sx, cfg.sz);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    static void AddTeleportFloor(Scene scene, string sceneName, float cx, float cz, float sx, float sz)
    {
        // Remove chão antigo se existir
        foreach (var go in scene.GetRootGameObjects())
            if (go.name == "TeleportFloor_VR") Object.DestroyImmediate(go);

        var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "TeleportFloor_VR";
        SceneManager.MoveGameObjectToScene(floor, scene);

        floor.transform.position   = new Vector3(cx, -0.01f, cz);
        floor.transform.localScale = new Vector3(sx * 0.1f, 1f, sz * 0.1f);

        // Configura material semi-transparente azul
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ??
                               Shader.Find("Standard"));
        if (mat != null)
        {
            mat.color = new Color(0.1f, 0.5f, 1f, 0.12f);
            floor.GetComponent<Renderer>().sharedMaterial = mat;
        }

        // Adiciona XRTeleportZone
        floor.AddComponent<XRTeleportZone>();

        // Tag correto para XR Interaction Toolkit
        // A TeleportationArea precisa do componente do XRI
        // Verificamos se o script existe antes de adicionar
        var teleportAreaType = System.Type.GetType(
            "UnityEngine.XR.Interaction.Toolkit.TeleportationArea, Unity.XR.Interaction.Toolkit");

        if (teleportAreaType != null)
        {
            floor.AddComponent(teleportAreaType);
            Debug.Log($"[XRFloorBuilder] TeleportationArea adicionado ao chão em {sceneName}.");
        }
        else
        {
            Debug.LogWarning($"[XRFloorBuilder] TeleportationArea não encontrado. " +
                             $"Verifique se o XR Interaction Toolkit está instalado.");
        }

        Debug.Log($"[XRFloorBuilder] Chão de teleporte criado em {sceneName} ({sx}x{sz}m).");
    }
}
#endif
