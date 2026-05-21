#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// ╔══════════════════════════════════════════════════════════════════╗
/// ║           PLANTA — FUTURO TECH (laboratório)                    ║
/// ╠══════════════════════════════════════════════════════════════════╣
/// ║                                                                  ║
/// ║   PAREDE NORTE (Z = +8)                                          ║
/// ║   ┌──────────────────────────────────────────┐                  ║
/// ║   │  [Chip]              (console esq.)      │                  ║
/// ║   │  X=-5, Y=1.45, Z=3                       │  PAREDE OESTE    ║
/// ║   │  (console esquerdo, plataforma elevada)   │  (X = -9)        ║
/// ║   │                                          │                  ║
/// ║   │         [Holograma]                      │  PAREDE LESTE    ║
/// ║   │         X=0, Y=0.85, Z=0.5              │  (X = +9)        ║
/// ║   │         (plataforma central, nível baixo) │                  ║
/// ║   │                                          │                  ║
/// ║   │                        [Exo]             │                  ║
/// ║   │                        X=5, Y=1.2, Z=-2.5│                  ║
/// ║   │                        (plataforma direita, fundo)          ║
/// ║   │                                          │                  ║
/// ║   │       ↑ SAÍDA / SPAWN JOGADOR            │                  ║
/// ║   └──────────────────────────────────────────┘                  ║
/// ║   PAREDE SUL (Z = -6)                                            ║
/// ╚══════════════════════════════════════════════════════════════════╝
///
/// SEQUÊNCIA SUGERIDA DE VISITA:
///   1. Holograma (centro, impacto visual imediato)
///   2. Chip (esquerda, sobe na plataforma)
///   3. Exo (direita, maior item do lab)
///
/// Menu: ChronoMundi → 🏛️ Room Builders → 🔬 Construir FuturoTechScene
/// </summary>
public static class FuturoTechRoomBuilder
{
    const string SCENE_PATH = "Assets/Projeto/Assets/Scenes/FuturoTechScene.unity";

    static readonly ArtifactLayoutFT[] ARTIFACTS = new[]
    {
        // ── 1. INTERFACE HOLOGRÁFICA ──────────────────────────────────────
        // Posição: plataforma central, Y baixo — o objeto "flutua" sobre a base
        // Lógica: visível imediatamente ao entrar, atrai o olhar
        // Formato: quase cúbico semi-transparente — simula cubo de dados holográfico
        new ArtifactLayoutFT
        {
            goName      = "Artefato_Holograma",
            artifactID  = "Holograma_FuturoTech",
            position    = new Vector3(0f, 0.85f, 0.5f),
            scale       = new Vector3(0.5f, 0.75f, 0.5f),
            rotation    = Quaternion.Euler(0f, 45f, 0f),    // 45° — visualmente dinâmico
            glowColor   = new Color(0f, 0.9f, 1f),          // ciano neon
            glowSpeed   = 3f,                                // pulso mais rápido (futurista)
            description = "Plataforma central — objeto mais visível da sala, pulso rápido",
        },

        // ── 2. PROCESSADOR QUÂNTICO ───────────────────────────────────────
        // Posição: console esquerdo, ligeiramente elevado (Y=1.45)
        // Lógica: objeto pequeno e precioso — exige aproximação para ver
        // Formato: disco fino (y muito pequeno) — simula chip sobre superfície
        new ArtifactLayoutFT
        {
            goName      = "Artefato_Chip",
            artifactID  = "Chip_FuturoTech",
            position    = new Vector3(-5f, 1.45f, 3f),
            scale       = new Vector3(0.2f, 0.03f, 0.2f),
            rotation    = Quaternion.identity,
            glowColor   = new Color(0.3f, 0.4f, 1f),        // azul elétrico
            glowSpeed   = 4f,                                // pulso mais rápido ainda
            description = "Console esquerdo elevado — pequeno e precioso, exige aproximação",
        },

        // ── 3. EXOESQUELETO NEURAL ────────────────────────────────────────
        // Posição: plataforma direita recuada — objeto maior da sala
        // Lógica: maior impacto visual, visitado por último
        // Formato: alto e largo — simula armadura/traje em exposição
        new ArtifactLayoutFT
        {
            goName      = "Artefato_Exo",
            artifactID  = "Exo_FuturoTech",
            position    = new Vector3(5f, 1.2f, -2.5f),
            scale       = new Vector3(0.35f, 1.1f, 0.2f),
            rotation    = Quaternion.Euler(0f, -30f, 0f),   // levemente voltado para a entrada
            glowColor   = new Color(0.1f, 1f, 0.4f),        // verde neon
            glowSpeed   = 1.6f,                              // pulso calmo — objeto "vivo"
            description = "Plataforma direita recuada — maior item do lab, impacto final",
        },
    };

    // ════════════════════════════════════════════════════════════════════
    [MenuItem("ChronoMundi/🏛️ Room Builders/🔬 Construir FuturoTechScene")]
    public static void Build()
    {
        if (!EditorUtility.DisplayDialog("ChronoMundi — FuturoTechScene",
            "Vai aplicar/criar os artefatos:\n" +
            "Holograma (plataforma central)\n" +
            "Chip (console esquerdo)\n" +
            "Exoesqueleto (plataforma direita)\n\n" +
            "Continuar?", "Construir", "Cancelar"))
            return;

        var scene = EditorSceneManager.OpenScene(SCENE_PATH, OpenSceneMode.Single);

        EditorUtility.DisplayProgressBar("ChronoMundi — RoomBuilder", "Aplicando artefatos...", 0.3f);
        int updated = 0, created = 0;
        var instances = new List<InteracleObject>();

        foreach (var layout in ARTIFACTS)
        {
            var (io, isNew) = ApplyOrCreate(scene, layout);
            if (io != null)
            {
                instances.Add(io);
                if (isNew) created++; else updated++;
            }
        }

        EditorUtility.DisplayProgressBar("ChronoMundi — RoomBuilder", "Reconectando ExhibitManager...", 0.7f);
        ReconnectExhibitManager(scene, instances);

        EditorUtility.DisplayProgressBar("ChronoMundi — RoomBuilder", "Salvando...", 0.95f);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        EditorUtility.ClearProgressBar();

        EditorUtility.DisplayDialog("🔬 FuturoTechScene ✅",
            $"Concluído!\n\n" +
            $"• {updated} artefato(s) atualizado(s)\n" +
            $"• {created} artefato(s) criado(s)\n\n" +
            "ExhibitManager reconectado.", "OK");
    }

    // ════════════════════════════════════════════════════════════════════

    static (InteracleObject io, bool isNew) ApplyOrCreate(
        UnityEngine.SceneManagement.Scene scene, ArtifactLayoutFT layout)
    {
        var data = ArtifactDatabaseBuilder.Load(layout.artifactID);
        if (data == null) return (null, false);

        GameObject go = FindInScene(scene, layout.goName);
        bool isNew = go == null;

        if (isNew)
        {
            go = GameObject.CreatePrimitive(
                layout.artifactID.Contains("Chip") ? PrimitiveType.Cube : PrimitiveType.Cube);
            go.name = layout.goName;
            SceneManager.MoveGameObjectToScene(go, scene);

            int layer = LayerMask.NameToLayer("Interactable");
            if (layer >= 0) go.layer = layer;

            BuildPedestal(scene, layout.goName + "_Pedestal",
                layout.position + new Vector3(0, -layout.scale.y * 0.5f - 0.3f, 0),
                layout.artifactID.Contains("Exo"));  // plataforma maior para o Exo
        }

        go.transform.position   = layout.position;
        go.transform.localScale = layout.scale;
        go.transform.rotation   = layout.rotation;

        var io = go.GetComponent<InteracleObject>();
        if (io == null) io = go.AddComponent<InteracleObject>();
        data.ApplyTo(io);
        io.interactOnlyOnce = true;

        var hc = go.GetComponent<HighlightController>();
        if (hc == null) hc = go.AddComponent<HighlightController>();
        hc.emissionColor  = layout.glowColor;
        hc.pulseSpeed     = layout.glowSpeed;
        hc.minIntensity   = 0.08f;
        hc.maxIntensity   = 2.2f;  // mais intenso para o lab futurista
        hc.stopOnInteract = true;

        UpdateOrCreateLabel(go, data.artifactName, layout.scale, layout.glowColor);

        EditorUtility.SetDirty(go);
        Debug.Log($"[FuturoTechBuilder] {(isNew ? "Criado" : "Atualizado")}: {layout.goName}");
        return (io, isNew);
    }

    static void ReconnectExhibitManager(
        UnityEngine.SceneManagement.Scene scene, List<InteracleObject> exhibits)
    {
        ExhibitManager em  = null;
        TimelineEra    era = null;

        foreach (var root in scene.GetRootGameObjects())
        {
            if (em  == null) em  = root.GetComponentInChildren<ExhibitManager>(true);
            if (era == null) era = root.GetComponentInChildren<TimelineEra>(true);
        }

        if (em != null)
        {
            em.exhibits = exhibits;
            if (era != null) em.eraController = era;
            EditorUtility.SetDirty(em);
        }

        if (era != null)
        {
            era.eraName       = "Futuro Tecnológico";
            era.introSubtitle = "Sistema iniciado.\nBem-vindo ao setor Futuro Tecnológico.";
            EditorUtility.SetDirty(era);
        }
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    static GameObject FindInScene(UnityEngine.SceneManagement.Scene scene, string name)
    {
        foreach (var root in scene.GetRootGameObjects())
        {
            if (root.name == name) return root;
            var t = System.Array.Find(root.GetComponentsInChildren<Transform>(true), x => x.name == name);
            if (t != null) return t.gameObject;
        }
        return null;
    }

    static void BuildPedestal(UnityEngine.SceneManagement.Scene scene,
        string name, Vector3 pos, bool large = false)
    {
        var old = FindInScene(scene, name);
        if (old != null) Object.DestroyImmediate(old);

        var ped = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ped.name = name;
        SceneManager.MoveGameObjectToScene(ped, scene);
        ped.transform.position   = pos;
        ped.transform.localScale = large
            ? new Vector3(0.6f, 0.25f, 0.6f)   // plataforma maior para o Exo
            : new Vector3(0.35f, 0.2f, 0.35f);  // plataforma tech menor
        Object.DestroyImmediate(ped.GetComponent<Collider>());
    }

    static void UpdateOrCreateLabel(GameObject parent, string text,
        Vector3 artScale, Color glowColor)
    {
        var old = parent.transform.Find("Label");
        if (old != null) Object.DestroyImmediate(old.gameObject);

        var go = new GameObject("Label");
        go.transform.SetParent(parent.transform, false);
        go.transform.localPosition = new Vector3(0, artScale.y * 0.5f + 0.3f, 0);
        go.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        go.transform.localScale    = Vector3.one * 0.012f;

        go.AddComponent<Canvas>().renderMode = RenderMode.WorldSpace;
        go.AddComponent<UnityEngine.UI.CanvasScaler>();

        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text         = text;
        tmp.fontSize     = 42;
        tmp.alignment    = TextAlignmentOptions.Center;
        tmp.color        = glowColor;  // cor do artefato para o label (estilo HUD futurista)
        tmp.fontStyle    = FontStyles.Bold;
        tmp.outlineWidth = 0.2f;
        tmp.outlineColor = Color.black;
        go.GetComponent<RectTransform>().sizeDelta = new Vector2(500, 100);
    }
}

class ArtifactLayoutFT
{
    public string     goName;
    public string     artifactID;
    public Vector3    position;
    public Vector3    scale;
    public Quaternion rotation   = Quaternion.identity;
    public Color      glowColor  = Color.cyan;
    public float      glowSpeed  = 2f;
    public string     description;
}
#endif
