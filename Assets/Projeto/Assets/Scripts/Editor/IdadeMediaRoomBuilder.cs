#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// ╔══════════════════════════════════════════════════════════════════╗
/// ║           PLANTA — IDADE MÉDIA (salão de castelo)               ║
/// ╠══════════════════════════════════════════════════════════════════╣
/// ║                                                                  ║
/// ║   PAREDE NORTE (Z = +6)                                          ║
/// ║   ┌──────────────────────────────────────────┐                  ║
/// ║   │                                          │                  ║
/// ║   │         [Pergaminho]                     │                  ║
/// ║   │         X=0, Y=1.15, Z=4.5              │                  ║
/// ║   │         (mesa de madeira central)        │                  ║
/// ║   │                                          │                  ║
/// ║   │  [Escudo]              [Espada]          │                  ║
/// ║   │  X=-3, Y=1.3, Z=2     X=3, Y=1.2, Z=2  │                  ║
/// ║   │  (suporte parede W)   (suporte parede E) │                  ║
/// ║   │                                          │                  ║
/// ║   │                                          │                  ║
/// ║   │       ↑ SAÍDA / SPAWN JOGADOR            │                  ║
/// ║   └──────────────────────────────────────────┘                  ║
/// ║   PAREDE SUL (Z = -5.5)                                          ║
/// ╚══════════════════════════════════════════════════════════════════╝
///
/// SEQUÊNCIA SUGERIDA DE VISITA:
///   1. Escudo (esquerda) → 2. Espada (direita) → 3. Pergaminho (fundo)
///
/// NOTA: Esta cena não tinha artefatos — todos são criados do zero.
///
/// Menu: ChronoMundi → 🏛️ Room Builders → ⚔️ Construir IdadeMediaScene
/// </summary>
public static class IdadeMediaRoomBuilder
{
    const string SCENE_PATH = "Assets/Projeto/Assets/Scenes/IdadeMediaScene.unity";

    // ── Layout explícito dos artefatos ────────────────────────────────────
    static readonly ArtifactLayoutIM[] ARTIFACTS = new[]
    {
        // ── 1. ESCUDO HERÁLDICO ───────────────────────────────────────────
        // Posição: parede oeste (X negativo), altura de exibição em suporte
        // Lógica: primeiro artefato à esquerda, visível ao entrar na sala
        // Formato: plano vertical (z pequeno) — simula escudo na parede
        new ArtifactLayoutIM
        {
            goName      = "Artefato_Escudo",
            artifactID  = "Escudo_IdadeMedia",
            position    = new Vector3(-3f, 1.3f, 2f),
            scale       = new Vector3(0.55f, 0.7f, 0.09f),
            rotation    = Quaternion.Euler(0f, 20f, 0f),    // levemente voltado ao centro
            glowColor   = new Color(1f, 0.15f, 0.15f),      // vermelho heráldico
            glowSpeed   = 2f,
            description = "Parede oeste — suporte de escudo em madeira entalhada",
        },

        // ── 2. ESPADA DE CAVALEIRO ────────────────────────────────────────
        // Posição: parede leste (X positivo), simétrico ao escudo
        // Lógica: par com o escudo — heraldry completa (escudo + espada)
        // Formato: fino e alto (x e z pequenos, y grande) — lâmina vertical
        new ArtifactLayoutIM
        {
            goName      = "Artefato_Espada",
            artifactID  = "Espada_IdadeMedia",
            position    = new Vector3(3f, 1.2f, 2f),
            scale       = new Vector3(0.08f, 0.75f, 0.06f),
            rotation    = Quaternion.Euler(0f, -20f, 0f),   // levemente voltado ao centro
            glowColor   = new Color(0.7f, 0.8f, 1f),        // azul aço
            glowSpeed   = 2.4f,
            description = "Parede leste — suporte de espada em aço forjado",
        },

        // ── 3. PERGAMINHO ILUMINADO ───────────────────────────────────────
        // Posição: mesa central ao fundo — posição de honra
        // Lógica: objeto mais importante no centro da atenção, mais ao fundo
        // Formato: plano deitado (y pequeno) — pergaminho aberto sobre mesa
        new ArtifactLayoutIM
        {
            goName      = "Artefato_Pergaminho",
            artifactID  = "Pergaminho_IdadeMedia",
            position    = new Vector3(0f, 1.15f, 4.5f),
            scale       = new Vector3(0.38f, 0.03f, 0.28f),
            rotation    = Quaternion.identity,
            glowColor   = new Color(1f, 0.85f, 0.1f),       // dourado iluminado
            glowSpeed   = 1.8f,
            description = "Mesa central ao fundo — posição de destaque (manuscrito aberto)",
        },
    };

    // ════════════════════════════════════════════════════════════════════
    [MenuItem("ChronoMundi/🏛️ Room Builders/⚔️ Construir IdadeMediaScene")]
    public static void Build()
    {
        if (!EditorUtility.DisplayDialog("ChronoMundi — IdadeMediaScene",
            "Vai aplicar/criar os artefatos:\n" +
            "Escudo (parede esquerda)\n" +
            "Espada (parede direita)\n" +
            "Pergaminho (mesa central fundo)\n\n" +
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

        EditorUtility.DisplayDialog("⚔️ IdadeMediaScene ✅",
            $"Concluído!\n\n" +
            $"• {updated} artefato(s) atualizado(s)\n" +
            $"• {created} artefato(s) criado(s)\n\n" +
            "ExhibitManager reconectado.", "OK");
    }

    // ════════════════════════════════════════════════════════════════════

    static (InteracleObject io, bool isNew) ApplyOrCreate(
        UnityEngine.SceneManagement.Scene scene, ArtifactLayoutIM layout)
    {
        var data = ArtifactDatabaseBuilder.Load(layout.artifactID);
        if (data == null) return (null, false);

        GameObject go = FindInScene(scene, layout.goName);
        bool isNew = go == null;

        if (isNew)
        {
            go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = layout.goName;
            SceneManager.MoveGameObjectToScene(go, scene);

            int layer = LayerMask.NameToLayer("Interactable");
            if (layer >= 0) go.layer = layer;

            // Pedestal (madeira entalhada medieval)
            BuildPedestal(scene, layout.goName + "_Pedestal",
                layout.position + new Vector3(0, -layout.scale.y * 0.5f - 0.32f, 0));
        }

        go.transform.position   = layout.position;
        go.transform.localScale = layout.scale;
        go.transform.rotation   = layout.rotation;

        // InteracleObject
        var io = go.GetComponent<InteracleObject>();
        if (io == null) io = go.AddComponent<InteracleObject>();
        data.ApplyTo(io);
        io.interactOnlyOnce = true;

        // HighlightController
        var hc = go.GetComponent<HighlightController>();
        if (hc == null) hc = go.AddComponent<HighlightController>();
        hc.emissionColor  = layout.glowColor;
        hc.pulseSpeed     = layout.glowSpeed;
        hc.minIntensity   = 0.05f;
        hc.maxIntensity   = 1.8f;
        hc.stopOnInteract = true;

        // Label
        UpdateOrCreateLabel(go, data.artifactName, layout.scale);

        EditorUtility.SetDirty(go);
        Debug.Log($"[IdadeMediaBuilder] {(isNew ? "Criado" : "Atualizado")}: {layout.goName}");
        return (io, isNew);
    }

    static void ReconnectExhibitManager(
        UnityEngine.SceneManagement.Scene scene, List<InteracleObject> exhibits)
    {
        ExhibitManager em  = null;
        TimelineEra    era = null;
        SimpleReturnToMuseum returnPt = null;

        foreach (var root in scene.GetRootGameObjects())
        {
            if (em       == null) em       = root.GetComponentInChildren<ExhibitManager>(true);
            if (era      == null) era      = root.GetComponentInChildren<TimelineEra>(true);
            if (returnPt == null) returnPt = root.GetComponentInChildren<SimpleReturnToMuseum>(true);
        }

        if (em != null)
        {
            em.exhibits = exhibits;
            if (era != null) em.eraController = era;
            EditorUtility.SetDirty(em);
        }

        if (era != null)
        {
            era.eraName       = "Idade Média";
            era.introSubtitle = "Bem-vindo à Idade Média.\nExplore as relíquias deste período histórico.";
            if (returnPt != null) era.returnPoint = returnPt;
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

    static void BuildPedestal(UnityEngine.SceneManagement.Scene scene, string name, Vector3 pos)
    {
        var old = FindInScene(scene, name);
        if (old != null) Object.DestroyImmediate(old);

        var ped = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ped.name = name;
        SceneManager.MoveGameObjectToScene(ped, scene);
        ped.transform.position   = pos;
        ped.transform.localScale = new Vector3(0.42f, 0.3f, 0.42f);
        Object.DestroyImmediate(ped.GetComponent<Collider>());
    }

    static void UpdateOrCreateLabel(GameObject parent, string text, Vector3 artScale)
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
        tmp.color        = new Color(1f, 0.9f, 0.5f); // dourado para Idade Média
        tmp.fontStyle    = FontStyles.Bold;
        tmp.outlineWidth = 0.15f;
        tmp.outlineColor = Color.black;
        go.GetComponent<RectTransform>().sizeDelta = new Vector2(500, 100);
    }
}

class ArtifactLayoutIM
{
    public string     goName;
    public string     artifactID;
    public Vector3    position;
    public Vector3    scale;
    public Quaternion rotation   = Quaternion.identity;
    public Color      glowColor  = Color.yellow;
    public float      glowSpeed  = 2f;
    public string     description;
}
#endif
