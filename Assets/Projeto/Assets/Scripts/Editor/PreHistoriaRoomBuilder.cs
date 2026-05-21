#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// ╔══════════════════════════════════════════════════════════════════╗
/// ║           PLANTA — PRÉ-HISTÓRIA (caverna)                       ║
/// ╠══════════════════════════════════════════════════════════════════╣
/// ║                                                                  ║
/// ║   PAREDE NORTE (Z = +5)                                          ║
/// ║   ┌──────────────────────────────────────────┐                  ║
/// ║   │                                          │                  ║
/// ║   │  [Pintura]         [Osso]                │  PAREDE OESTE    ║
/// ║   │  X=-2, Y=1.1, Z=2.5  X=0, Y=1.0, Z=3.5  │  (X = -5)        ║
/// ║   │  (na parede W)    (pedestal centro)      │                  ║
/// ║   │                                          │                  ║
/// ║   │         ÁREA DE CIRCULAÇÃO               │  PAREDE LESTE    ║
/// ║   │                                          │  (X = +5)        ║
/// ║   │  [Machado]                               │                  ║
/// ║   │  X=2, Y=0.9, Z=0                        │                  ║
/// ║   │  (pedestal entrada direita)              │                  ║
/// ║   │                                          │                  ║
/// ║   │         ↑ SPAWN JOGADOR / SAÍDA           │                  ║
/// ║   └──────────────────────────────────────────┘                  ║
/// ║   PAREDE SUL (Z = -5)                                            ║
/// ╚══════════════════════════════════════════════════════════════════╝
///
/// SEQUÊNCIA SUGERIDA DE VISITA:
///   1. Machado (entrada) → 2. Osso (centro) → 3. Pintura (parede)
///
/// Menu: ChronoMundi → 🏛️ Room Builders → 🪨 Construir PreHistoriaScene
/// </summary>
public static class PreHistoriaRoomBuilder
{
    const string SCENE_PATH = "Assets/Projeto/Assets/Scenes/PreHistoriaScene.unity";

    // ── Layout explícito dos artefatos ────────────────────────────────────
    // Cada entrada: goName, artifactID (busca no ArtifactDatabaseBuilder),
    //               posição, escala, cor do glow, rotação
    static readonly ArtifactLayout[] ARTIFACTS = new[]
    {
        // ── 1. MACHADO DE PEDRA ───────────────────────────────────────────
        // Posição: entrada da caverna, lado direito
        // Lógica: primeiro objeto que o jogador vê ao entrar
        // Formato: chato (y pequeno) — simula machado deitado no pedestal
        new ArtifactLayout
        {
            goName      = "Artefato_Machado",
            artifactID  = "Machado_PreHistoria",
            position    = new Vector3(2f, 0.9f, 0f),
            scale       = new Vector3(0.28f, 0.06f, 0.16f),
            rotation    = Quaternion.Euler(0f, 30f, 0f),    // leve ângulo
            glowColor   = new Color(1f, 0.5f, 0.1f),        // laranja fogo
            glowSpeed   = 2f,
            description = "Pedestal à direita da entrada — primeiro artefato visível",
        },

        // ── 2. OSSO NUMERADO ──────────────────────────────────────────────
        // Posição: centro da caverna, mais ao fundo
        // Lógica: posição de destaque central, atrai jogador para explorar
        // Formato: cilindro vertical — simula osso em pé no pedestal
        new ArtifactLayout
        {
            goName      = "Artefato_Osso",
            artifactID  = "Osso_PreHistoria",
            position    = new Vector3(0f, 1.0f, 3.5f),
            scale       = new Vector3(0.1f, 0.45f, 0.1f),
            rotation    = Quaternion.identity,
            glowColor   = new Color(0.9f, 0.8f, 0.5f),      // bege/marfim
            glowSpeed   = 1.8f,
            description = "Pedestal central no fundo — foco visual da caverna",
        },

        // ── 3. PINTURA RUPESTRE ───────────────────────────────────────────
        // Posição: parede oeste (X negativo), inclinada contra a parede
        // Lógica: última descoberta — jogador explora o lado esquerdo
        // Formato: plano horizontal (z pequeno) — simula placa de rocha
        new ArtifactLayout
        {
            goName      = "Artefato_Pintura",
            artifactID  = "Pintura_PreHistoria",
            position    = new Vector3(-2f, 1.1f, 2.5f),
            scale       = new Vector3(0.8f, 0.6f, 0.06f),
            rotation    = Quaternion.Euler(0f, 15f, 0f),     // levemente voltado ao jogador
            glowColor   = new Color(0.6f, 0.3f, 0.1f),      // marrom terra
            glowSpeed   = 2.2f,
            description = "Na parede oeste — simula rocha com pintura encostada",
        },
    };

    // ════════════════════════════════════════════════════════════════════
    [MenuItem("ChronoMundi/🏛️ Room Builders/🪨 Construir PreHistoriaScene")]
    public static void Build()
    {
        if (!ConfirmDialog("PreHistoriaScene",
            "Machado (entrada direita)\nOsso (centro)\nPintura (parede oeste)"))
            return;

        var scene = EditorSceneManager.OpenScene(SCENE_PATH, OpenSceneMode.Single);

        Prog("Aplicando artefatos...", 0.3f);
        int updated = 0, created = 0;
        var instances = new List<InteracleObject>();

        foreach (var layout in ARTIFACTS)
        {
            var (io, isNew) = ApplyOrCreateArtifact(scene, layout);
            if (io != null)
            {
                instances.Add(io);
                if (isNew) created++; else updated++;
            }
        }

        Prog("Reconectando ExhibitManager...", 0.7f);
        ReconnectExhibitManager(scene, instances);

        Prog("Salvando...", 0.95f);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        EditorUtility.ClearProgressBar();

        EditorUtility.DisplayDialog("🪨 PreHistoriaScene ✅",
            $"Concluído!\n\n" +
            $"• {updated} artefato(s) atualizado(s)\n" +
            $"• {created} artefato(s) criado(s)\n\n" +
            "ExhibitManager reconectado.", "OK");
    }

    // ════════════════════════════════════════════════════════════════════
    // CORE
    // ════════════════════════════════════════════════════════════════════

    static (InteracleObject io, bool isNew) ApplyOrCreateArtifact(
        UnityEngine.SceneManagement.Scene scene, ArtifactLayout layout)
    {
        // Carrega dados do SO (backend)
        var data = ArtifactDatabaseBuilder.Load(layout.artifactID);
        if (data == null) return (null, false);

        // Verifica se o artefato já existe na cena
        GameObject go = FindInScene(scene, layout.goName);
        bool isNew = go == null;

        if (isNew)
        {
            go = BuildArtifactGO(scene, layout);
            Debug.Log($"[PreHistoriaBuilder] Criado: {layout.goName}");
        }
        else
        {
            // Atualiza posição/escala/rotação
            go.transform.position   = layout.position;
            go.transform.localScale = layout.scale;
            go.transform.rotation   = layout.rotation;
            Debug.Log($"[PreHistoriaBuilder] Atualizado: {layout.goName}");
        }

        // Aplica dados do SO ao InteracleObject
        var io = go.GetComponent<InteracleObject>();
        if (io == null) io = go.AddComponent<InteracleObject>();
        data.ApplyTo(io);
        io.interactOnlyOnce = true;

        // Atualiza HighlightController
        var hc = go.GetComponent<HighlightController>();
        if (hc == null) hc = go.AddComponent<HighlightController>();
        hc.emissionColor  = layout.glowColor;
        hc.pulseSpeed     = layout.glowSpeed;
        hc.minIntensity   = 0.05f;
        hc.maxIntensity   = 1.8f;
        hc.stopOnInteract = true;

        // Label com nome
        UpdateOrCreateLabel(go, data.artifactName, layout.scale);

        EditorUtility.SetDirty(go);
        return (io, isNew);
    }

    static GameObject BuildArtifactGO(
        UnityEngine.SceneManagement.Scene scene, ArtifactLayout layout)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = layout.goName;
        SceneManager.MoveGameObjectToScene(go, scene);
        go.transform.position   = layout.position;
        go.transform.localScale = layout.scale;
        go.transform.rotation   = layout.rotation;

        int layer = LayerMask.NameToLayer("Interactable");
        if (layer >= 0) go.layer = layer;

        // Pedestal de pedra
        BuildPedestal(scene, layout.goName + "_Pedestal",
            layout.position + new Vector3(0, -layout.scale.y * 0.5f - 0.32f, 0),
            new Vector3(0.42f, 0.3f, 0.42f));

        return go;
    }

    static void ReconnectExhibitManager(
        UnityEngine.SceneManagement.Scene scene, List<InteracleObject> exhibits)
    {
        ExhibitManager em = null;
        TimelineEra   era = null;

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
    }

    // ════════════════════════════════════════════════════════════════════
    // SHARED HELPERS (replicados por sala para evitar dependência circular)
    // ════════════════════════════════════════════════════════════════════

    static GameObject FindInScene(UnityEngine.SceneManagement.Scene scene, string name)
    {
        foreach (var root in scene.GetRootGameObjects())
        {
            if (root.name == name) return root;
            var found = System.Array.Find(
                root.GetComponentsInChildren<Transform>(true), t => t.name == name);
            if (found != null) return found.gameObject;
        }
        return null;
    }

    static void BuildPedestal(UnityEngine.SceneManagement.Scene scene,
        string name, Vector3 pos, Vector3 scale)
    {
        var old = FindInScene(scene, name);
        if (old != null) Object.DestroyImmediate(old);

        var ped = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ped.name = name;
        SceneManager.MoveGameObjectToScene(ped, scene);
        ped.transform.position   = pos;
        ped.transform.localScale = scale;
        Object.DestroyImmediate(ped.GetComponent<Collider>());
    }

    static void UpdateOrCreateLabel(GameObject parent, string text, Vector3 artScale)
    {
        // Remove label antiga
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
        tmp.text      = text;
        tmp.fontSize  = 42;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = Color.white;
        tmp.fontStyle = FontStyles.Bold;
        tmp.outlineWidth = 0.15f;
        tmp.outlineColor = Color.black;

        go.GetComponent<RectTransform>().sizeDelta = new Vector2(500, 100);
    }

    static bool ConfirmDialog(string sceneName, string artifacts)
    {
        return EditorUtility.DisplayDialog(
            $"ChronoMundi — {sceneName}",
            $"Vai aplicar/criar os artefatos:\n{artifacts}\n\nContinuar?",
            "Construir", "Cancelar");
    }

    static void Prog(string msg, float p) =>
        EditorUtility.DisplayProgressBar("ChronoMundi — RoomBuilder", msg, p);
}

// Estrutura de layout por artefato — interna ao builder
class ArtifactLayout
{
    public string     goName;
    public string     artifactID;
    public Vector3    position;
    public Vector3    scale;
    public Quaternion rotation   = Quaternion.identity;
    public Color      glowColor  = Color.cyan;
    public float      glowSpeed  = 2f;
    public string     description; // só documentação, não vai para a cena
}
#endif
