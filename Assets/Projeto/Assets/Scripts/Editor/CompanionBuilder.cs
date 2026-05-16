#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Adiciona o personagem guia a todas as cenas históricas.
/// Cada era recebe um guia com nome, cor e diálogos próprios.
///
/// Menu: ChronoMundi → 🧙 Adicionar Personagem Guia — Todas as Cenas
/// </summary>
public static class CompanionBuilder
{
    const string SCENES_PATH = "Assets/Projeto/Assets/Scenes/";

    // ── Configuração por era ───────────────────────────────────────────────

    class EraCompanionConfig
    {
        public string sceneName;
        public string companionName;
        public Color  color;
        public string introLine;
        public string allDoneLine;
        public List<string> comments;
        public Vector3 spawnOffset;
    }

    static readonly List<EraCompanionConfig> CONFIGS = new()
    {
        new EraCompanionConfig
        {
            sceneName     = "PreHistoriaScene",
            companionName = "Uruk",
            color         = new Color(1f, 0.55f, 0.1f), // laranja pedra
            introLine     = "Uggh! Bem-vindo à Pré-História! Sou Uruk. " +
                            "Este lugar tem muita coisa para te mostrar. Vai lá, explora!",
            allDoneLine   = "Você viu tudo! Os ancestrais ficariam orgulhosos. " +
                            "Hora de voltar ao museu, amigo.",
            comments      = new List<string>
            {
                "Isso aí! Nossos ancestrais usavam isso no dia a dia. " +
                "Simples, mas muito inteligente para a época.",
                "Viu? Sem tecnologia nenhuma — só força, criatividade e necessidade.",
                "Cada objeto desses carrega milênios de história nas mãos."
            },
            spawnOffset = new Vector3(2f, 0f, 0f)
        },

        new EraCompanionConfig
        {
            sceneName     = "IdadeMediaScene",
            companionName = "Sir Aldric",
            color         = new Color(0.8f, 0.7f, 0.15f), // dourado medieval
            introLine     = "Salve, viajante! Sou Sir Aldric, cavaleiro e guardião deste museu. " +
                            "Deixe-me guiá-lo pelos tesouros da Idade Média.",
            allDoneLine   = "Honroso! Você conheceu todas as relíquias desta era. " +
                            "O rei certamente ficaria impressionado. Retorne ao museu.",
            comments      = new List<string>
            {
                "Este artefato pertenceu a nobres e guerreiros. " +
                "Cada marca conta uma batalha, cada detalhe uma tradição.",
                "Na Idade Média, objetos assim valiam fortunas. " +
                "Passavam de pai para filho por gerações.",
                "Os artesãos medievais levavam anos para dominar sua arte. " +
                "Veja a precisão deste trabalho."
            },
            spawnOffset = new Vector3(2f, 0f, 0f)
        },

        new EraCompanionConfig
        {
            sceneName     = "FuturoTechScene",
            companionName = "ARIA",
            color         = new Color(0.1f, 0.85f, 1f), // ciano futurista
            introLine     = "Inicializando sistema de guia... Olá! Sou ARIA, " +
                            "sua assistente de inteligência artificial. " +
                            "Bem-vindo ao setor Futuro Tecnológico. Vamos explorar?",
            allDoneLine   = "Análise completa. Você processou todos os artefatos do setor. " +
                            "Retorne ao hub central para prosseguir.",
            comments      = new List<string>
            {
                "Fascinante. Este objeto representa um salto tecnológico " +
                "que mudou a humanidade para sempre.",
                "Meus sensores detectam que você está impressionado — " +
                "e com razão! Esta tecnologia era impensável décadas atrás.",
                "Registro histórico confirmado. Este artefato marcou " +
                "o início de uma nova era para a civilização."
            },
            spawnOffset = new Vector3(2f, 0f, 0f)
        }
    };

    // ════════════════════════════════════════════════════════════════════
    [MenuItem("ChronoMundi/🧙 Adicionar Personagem Guia — Todas as Cenas")]
    public static void AddToAllScenes()
    {
        if (!EditorUtility.DisplayDialog("ChronoMundi — Personagem Guia",
            "Adiciona um guia único a cada cena histórica:\n\n" +
            "• Pré-História → Uruk (laranja)\n" +
            "• Idade Média  → Sir Aldric (dourado)\n" +
            "• Futuro Tech  → ARIA (ciano)\n\n" +
            "Cada guia reage aos artefatos com comentários próprios.",
            "Adicionar", "Cancelar"))
            return;

        float p = 0f;
        foreach (var cfg in CONFIGS)
        {
            p += 0.3f;
            EditorUtility.DisplayProgressBar("ChronoMundi — Guia", $"Adicionando {cfg.companionName}...", p);
            BuildCompanionInScene(cfg);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();

        EditorUtility.DisplayDialog("ChronoMundi ✅",
            "Personagens guia adicionados!\n\n" +
            "✔ Uruk na PreHistoriaScene\n" +
            "✔ Sir Aldric na IdadeMediaScene\n" +
            "✔ ARIA na FuturoTechScene\n\n" +
            "Para adicionar áudio: selecione o guia na cena e\n" +
            "arraste os AudioClips nos campos do CompanionCharacter.",
            "OK");
    }

    [MenuItem("ChronoMundi/🧙 Adicionar Personagem Guia — Cena Atual")]
    public static void AddToCurrentScene()
    {
        var scene = EditorSceneManager.GetActiveScene();
        var cfg   = CONFIGS.Find(c => c.sceneName == scene.name);

        if (cfg == null)
        {
            EditorUtility.DisplayDialog("ChronoMundi",
                $"Nenhuma configuração de guia para '{scene.name}'.\n" +
                "O guia só é configurado para cenas históricas.", "OK");
            return;
        }

        BuildCompanionInScene(cfg);
        EditorUtility.DisplayDialog("ChronoMundi ✅",
            $"{cfg.companionName} adicionado a {scene.name}!", "OK");
    }

    // ════════════════════════════════════════════════════════════════════

    static void BuildCompanionInScene(EraCompanionConfig cfg)
    {
        var scene = EditorSceneManager.OpenScene(
            SCENES_PATH + cfg.sceneName + ".unity", OpenSceneMode.Single);

        // Remove guia antigo se existir
        foreach (var go in scene.GetRootGameObjects())
            if (go.name == "Companion_" + cfg.companionName)
                Object.DestroyImmediate(go);

        // ── Raiz do Guia ──────────────────────────────────────────────────
        var root = new GameObject("Companion_" + cfg.companionName);
        SceneManager.MoveGameObjectToScene(root, scene);
        root.transform.position = GetPlayerSpawn(cfg.sceneName) + cfg.spawnOffset;

        // ── Corpo (cápsula) ───────────────────────────────────────────────
        var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Body";
        body.transform.SetParent(root.transform, false);
        body.transform.localPosition = Vector3.zero;
        body.transform.localScale    = new Vector3(0.45f, 0.55f, 0.45f);
        Object.DestroyImmediate(body.GetComponent<Collider>());

        // Material com cor da era
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ??
                               Shader.Find("Standard"));
        mat.color = cfg.color;
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", cfg.color * 0.5f);
        body.GetComponent<Renderer>().sharedMaterial = mat;

        // Salva material
        string matDir  = "Assets/Projeto/Assets/Materials/Companions/";
        EnsureDirectory(matDir);
        string matPath = matDir + cfg.companionName + "_Mat.mat";
        AssetDatabase.CreateAsset(mat, matPath);

        // ── Cabeça ────────────────────────────────────────────────────────
        var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "Head";
        head.transform.SetParent(root.transform, false);
        head.transform.localPosition = new Vector3(0, 0.85f, 0);
        head.transform.localScale    = new Vector3(0.38f, 0.38f, 0.38f);
        Object.DestroyImmediate(head.GetComponent<Collider>());
        head.GetComponent<Renderer>().sharedMaterial = mat;

        // ── Olhos (visuais) ───────────────────────────────────────────────
        AddEye(head, new Vector3(-0.12f, 0.06f, 0.35f));
        AddEye(head, new Vector3( 0.12f, 0.06f, 0.35f));

        // ── Aura de partículas (esfera de luz pulsante) ───────────────────
        AddAura(root, cfg.color);

        // ── Name Tag ──────────────────────────────────────────────────────
        var nameTag = BuildNameTag(root, cfg.companionName, cfg.color);

        // ── CompanionCharacter ────────────────────────────────────────────
        var companion = root.AddComponent<CompanionCharacter>();
        companion.companionName   = cfg.companionName;
        companion.companionColor  = cfg.color;
        companion.introLine       = cfg.introLine;
        companion.allDoneLines    = cfg.allDoneLine;
        companion.exhibitComments = cfg.comments;
        companion.bodyRenderer    = body.GetComponent<Renderer>();
        companion.nameTagTMP      = nameTag;
        companion.followDistance  = 1.8f;
        companion.moveSpeed       = 4f;
        companion.side            = 1f;

        // ── AudioSource ───────────────────────────────────────────────────
        var audio = root.AddComponent<AudioSource>();
        audio.spatialBlend = 0.6f; // semi-3D
        audio.minDistance  = 2f;
        audio.maxDistance  = 15f;
        audio.volume       = 0.85f;

        EditorUtility.SetDirty(root);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log($"[CompanionBuilder] {cfg.companionName} adicionado à {cfg.sceneName}.");
    }

    // ── Helpers visuais ───────────────────────────────────────────────────

    static void AddEye(GameObject head, Vector3 localPos)
    {
        var eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        eye.name = "Eye";
        eye.transform.SetParent(head.transform, false);
        eye.transform.localPosition = localPos;
        eye.transform.localScale    = Vector3.one * 0.18f;
        Object.DestroyImmediate(eye.GetComponent<Collider>());

        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ??
                               Shader.Find("Standard"));
        mat.color = Color.black;
        eye.GetComponent<Renderer>().sharedMaterial = mat;
    }

    static void AddAura(GameObject root, Color color)
    {
        var aura = new GameObject("Aura");
        aura.transform.SetParent(root.transform, false);
        aura.transform.localPosition = new Vector3(0, 0.5f, 0);

        var ps = aura.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startColor     = new ParticleSystem.MinMaxGradient(color * 0.8f, color);
        main.startSize      = new ParticleSystem.MinMaxCurve(0.04f, 0.12f);
        main.startLifetime  = new ParticleSystem.MinMaxCurve(1f, 2f);
        main.startSpeed     = new ParticleSystem.MinMaxCurve(0.1f, 0.3f);
        main.maxParticles   = 30;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = 10f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius    = 0.35f;
    }

    static TextMeshProUGUI BuildNameTag(GameObject root, string name, Color color)
    {
        var tagGO = new GameObject("NameTag");
        tagGO.transform.SetParent(root.transform, false);
        tagGO.transform.localPosition = new Vector3(0, 1.45f, 0);
        tagGO.transform.localScale    = Vector3.one * 0.01f;

        var canvas = tagGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        tagGO.AddComponent<UnityEngine.UI.CanvasScaler>();

        var tmp = tagGO.AddComponent<TextMeshProUGUI>();
        tmp.text      = name;
        tmp.fontSize  = 52;
        tmp.color     = color;
        tmp.fontStyle = TMPro.FontStyles.Bold;
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.outlineWidth = 0.2f;
        tmp.outlineColor = Color.black;

        var rt = tagGO.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(600, 120);

        return tmp;
    }

    // ── Utils ─────────────────────────────────────────────────────────────

    static Vector3 GetPlayerSpawn(string sceneName) => sceneName switch
    {
        "PreHistoriaScene" => new Vector3(0, 0, -4f),
        "IdadeMediaScene"  => new Vector3(0, 0, -4f),
        "FuturoTechScene"  => new Vector3(0, 0.3f, -4.5f),
        _                  => Vector3.zero
    };

    static void EnsureDirectory(string path)
    {
        if (!UnityEditor.AssetDatabase.IsValidFolder(path.TrimEnd('/')))
        {
            var parts = path.TrimEnd('/').Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
#endif
