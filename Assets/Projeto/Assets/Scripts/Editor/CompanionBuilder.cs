#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Adiciona e configura os personagens guia em todas as cenas históricas.
///
/// Correções v2:
///   - Name tag rotacionada 180° em Y para ficar de frente ao jogador
///   - Artefatos recriados nas posições/escalas reais da cena
///   - Narração preenchida com textos históricos completos
///   - Comentários do guia sincronizados com cada artefato específico
///   - IdadeMedia: artefatos criados do zero pois não existiam
///
/// Menu: ChronoMundi → 🧙 Adicionar Personagem Guia — Todas as Cenas
/// </summary>
public static class CompanionBuilder
{
    const string SCENES_PATH = "Assets/Projeto/Assets/Scenes/";
    const string MAT_PATH    = "Assets/Projeto/Assets/Materials/Environment/";

    // ════════════════════════════════════════════════════════════════════
    // CONFIG DOS GUIAS
    // ════════════════════════════════════════════════════════════════════

    class CompanionConfig
    {
        public string       sceneName;
        public string       companionName;
        public Color        color;
        public string       introLine;
        public string       allDoneLine;
        // Comentários indexados por artefato (mesmo índice que artifacts abaixo)
        public List<string> comments;
        public Vector3      spawnOffset;
    }

    // Dados completos de cada artefato por cena
    class ArtifactData
    {
        public string  goName;
        public string  artifactName;
        public string  subtitle;
        public string  narration;   // texto completo para o NarratorSystem
        public Vector3 position;
        public Vector3 scale;
        public Color   glowColor;
    }

    // ── PRÉ-HISTÓRIA ──────────────────────────────────────────────────────
    static readonly List<ArtifactData> PREHISTORIA_ARTIFACTS = new()
    {
        new ArtifactData
        {
            goName       = "Artefato_Machado",
            artifactName = "Machado de Pedra",
            subtitle     = "Machado de pedra — 2,5 milhões de anos",
            narration    = "O machado de pedra lascada é uma das ferramentas mais antigas já criadas " +
                           "pelo ser humano. Fabricado há aproximadamente 2,5 milhões de anos, era usado " +
                           "para cortar carne, quebrar ossos e trabalhar madeira. Representa o primeiro " +
                           "salto tecnológico da humanidade: transformar um recurso natural bruto em " +
                           "uma ferramenta com propósito definido.",
            position  = new Vector3(2f, 0.9f, 0f),
            scale     = new Vector3(0.28f, 0.06f, 0.16f),
            glowColor = new Color(1f, 0.5f, 0.1f),
        },
        new ArtifactData
        {
            goName       = "Artefato_Osso",
            artifactName = "Osso Numerado",
            subtitle     = "Os primeiros números da humanidade",
            narration    = "O Osso de Ishango, encontrado no atual Congo e datado de cerca de 25.000 anos, " +
                           "é considerado por muitos pesquisadores o primeiro artefato matemático da história. " +
                           "Suas marcas entalhadas podem representar contagens lunares ou sequências " +
                           "numéricas primitivas. Ele evidencia que a capacidade de abstração matemática " +
                           "surgiu muito antes da escrita ou das civilizações formais.",
            position  = new Vector3(0f, 1.0f, 3.5f),
            scale     = new Vector3(0.1f, 0.45f, 0.1f),
            glowColor = new Color(0.9f, 0.8f, 0.5f),
        },
        new ArtifactData
        {
            goName       = "Artefato_Pintura",
            artifactName = "Pintura Rupestre",
            subtitle     = "Arte nas pedras — os primeiros registros humanos",
            narration    = "As pinturas rupestres são expressões artísticas feitas com pigmentos minerais " +
                           "como ocre e carvão nas paredes de cavernas. As mais antigas têm mais de 40.000 anos. " +
                           "Representam animais, caçadas e rituais, sendo o primeiro sistema de comunicação " +
                           "visual da humanidade. Mais do que decoração, eram possivelmente registros " +
                           "sagrados e formas de transmitir conhecimento entre gerações.",
            position  = new Vector3(-2f, 1.1f, 2.5f),
            scale     = new Vector3(0.8f, 0.6f, 0.06f),
            glowColor = new Color(0.6f, 0.3f, 0.1f),
        },
    };

    static readonly CompanionConfig PREHISTORIA_CONFIG = new()
    {
        sceneName     = "PreHistoriaScene",
        companionName = "Uruk",
        color         = new Color(1f, 0.55f, 0.1f),
        introLine     = "Uggh! Bem-vindo à Pré-História! Sou Uruk. " +
                        "Muita coisa aqui para você ver. Toca nos objetos para descobrir!",
        allDoneLine   = "Você viu tudo! Os ancestrais ficariam orgulhosos. " +
                        "Hora de voltar ao museu, amigo.",
        comments      = new List<string>
        {
            // índice 0 → Machado
            "Isso! Com essa pedra cortava carne, madeira, tudo. Simples, mas muito inteligente!",
            // índice 1 → Osso
            "Olha esses entalhes. Cada risco um número. Meus ancestrais já contavam as luas!",
            // índice 2 → Pintura
            "Bonito, não é? Faziam com carvão e pedra colorida. Guardavam histórias na rocha.",
        },
        spawnOffset = new Vector3(2.2f, 0f, -1.5f),
    };

    // ── IDADE MÉDIA ───────────────────────────────────────────────────────
    static readonly List<ArtifactData> IDADEMEDIA_ARTIFACTS = new()
    {
        new ArtifactData
        {
            goName       = "Artefato_Pergaminho",
            artifactName = "Pergaminho Iluminado",
            subtitle     = "Os livros da Idade Média — feitos à mão por anos",
            narration    = "Os manuscritos iluminados eram produzidos à mão por monges escribas " +
                           "em mosteiros medievais. Cada página podia levar semanas de trabalho, " +
                           "adornada com letras capitais decoradas, ilustrações em folha de ouro " +
                           "e pigmentos raros vindos de toda a Europa e do Oriente. " +
                           "Eram os livros mais preciosos da Idade Média, preservando não só " +
                           "textos religiosos, mas também filosofia clássica, medicina e astronomia.",
            position  = new Vector3(0f, 1.15f, 4.5f),
            scale     = new Vector3(0.38f, 0.03f, 0.28f),
            glowColor = new Color(1f, 0.85f, 0.1f),
        },
        new ArtifactData
        {
            goName       = "Artefato_Escudo",
            artifactName = "Escudo Heráldico",
            subtitle     = "Identidade e honra representadas em metal",
            narration    = "Os brasões heráldicos medievais funcionavam como identidades visuais " +
                           "das famílias nobres — equivalentes aos logos modernos. " +
                           "Cada elemento tinha significado preciso: a cor vermelha simbolizava coragem " +
                           "e ousadia; o leão, força e nobreza; as listras, distinção em batalha. " +
                           "O escudo era passado de pai para filho e reconhecido em combate " +
                           "mesmo com a viseira fechada, sendo essencial para identificação no campo de batalha.",
            position  = new Vector3(-3f, 1.3f, 2f),
            scale     = new Vector3(0.55f, 0.7f, 0.09f),
            glowColor = new Color(1f, 0.1f, 0.1f),
        },
        new ArtifactData
        {
            goName       = "Artefato_Espada",
            artifactName = "Espada de Cavaleiro",
            subtitle     = "Símbolo de poder e nobreza da Idade Média",
            narration    = "A espada de cavaleiro era muito mais que uma arma: era um símbolo de status, " +
                           "honra e compromisso com o código da cavalaria. " +
                           "Forjada por ferreiros especializados ao longo de semanas, " +
                           "cada espada era praticamente única. O aço era dobrado e martelado centenas " +
                           "de vezes para obter a dureza e flexibilidade certas. " +
                           "Cavaleiros eram agraciados com suas espadas em cerimônias formais " +
                           "e muitas vezes as recebiam com nomes próprios.",
            position  = new Vector3(3f, 1.2f, 2f),
            scale     = new Vector3(0.08f, 0.75f, 0.06f),
            glowColor = new Color(0.7f, 0.8f, 1f),
        },
    };

    static readonly CompanionConfig IDADEMEDIA_CONFIG = new()
    {
        sceneName     = "IdadeMediaScene",
        companionName = "Sir Aldric",
        color         = new Color(0.85f, 0.72f, 0.15f),
        introLine     = "Salve, viajante! Sou Sir Aldric, cavaleiro guardião deste museu. " +
                        "Permita-me guiá-lo pelos tesouros da Idade Média.",
        allDoneLine   = "Honroso! Você conheceu todas as relíquias desta era. " +
                        "O rei certamente ficaria impressionado. Retorne ao museu.",
        comments      = new List<string>
        {
            // índice 0 → Pergaminho
            "Este manuscrito levou meses para ser produzido. Cada letra, uma obra de arte.",
            // índice 1 → Escudo
            "Este brasão pertenceu a uma família nobre. Cada símbolo conta uma batalha vencida.",
            // índice 2 → Espada
            "Forjada à mão, temperada em fogo e água. Uma espada assim durava gerações.",
        },
        spawnOffset = new Vector3(2.2f, 0f, -1.5f),
    };

    // ── FUTURO TECH ───────────────────────────────────────────────────────
    static readonly List<ArtifactData> FUTURO_ARTIFACTS = new()
    {
        new ArtifactData
        {
            goName       = "Artefato_Holograma",
            artifactName = "Interface Holográfica",
            subtitle     = "O futuro das interfaces humano-máquina",
            narration    = "A interface holográfica projeta informações tridimensionais no ar " +
                           "sem necessidade de superfície física. Utilizando lasers e difração de luz, " +
                           "cria imagens com profundidade real, manipuláveis com gestos das mãos. " +
                           "Esta tecnologia elimina a barreira entre mundo digital e físico, " +
                           "permitindo colaboração imersiva à distância, cirurgias guiadas por projeção " +
                           "e ambientes de trabalho que se adaptam ao usuário em tempo real.",
            position  = new Vector3(0f, 0.85f, 0.5f),
            scale     = new Vector3(0.5f, 0.75f, 0.5f),
            glowColor = new Color(0f, 0.9f, 1f),
        },
        new ArtifactData
        {
            goName       = "Artefato_Chip",
            artifactName = "Processador Quântico",
            subtitle     = "Computação quântica — além dos limites do silício",
            narration    = "O processador quântico utiliza qubits que, ao contrário dos bits clássicos, " +
                           "podem existir em superposição de zero e um simultaneamente. " +
                           "Isso permite que um computador quântico resolva em segundos " +
                           "problemas que levariam milhões de anos para processadores convencionais. " +
                           "Aplicações incluem simulação molecular para descoberta de remédios, " +
                           "quebra de criptografia avançada e otimização de rotas logísticas globais.",
            position  = new Vector3(-5f, 1.45f, 3f),
            scale     = new Vector3(0.2f, 0.03f, 0.2f),
            glowColor = new Color(0.3f, 0.5f, 1f),
        },
        new ArtifactData
        {
            goName       = "Artefato_Exo",
            artifactName = "Exoesqueleto Neural",
            subtitle     = "A fusão entre humano e máquina",
            narration    = "O exoesqueleto neural é controlado diretamente por sinais elétricos do cérebro, " +
                           "capturados por sensores não-invasivos na superfície do crânio. " +
                           "Permite que pessoas com paralisia voltem a andar, amplifica a força " +
                           "humana em até dez vezes para uso industrial e militar, " +
                           "e serve como interface de reabilitação motora para pacientes neurológicos. " +
                           "Representa a convergência entre neurociência, robótica e inteligência artificial.",
            position  = new Vector3(5f, 1.2f, -2.5f),
            scale     = new Vector3(0.35f, 1.1f, 0.2f),
            glowColor = new Color(0.1f, 1f, 0.5f),
        },
    };

    static readonly CompanionConfig FUTURO_CONFIG = new()
    {
        sceneName     = "FuturoTechScene",
        companionName = "ARIA",
        color         = new Color(0.1f, 0.85f, 1f),
        introLine     = "Sistemas iniciados. Olá, sou ARIA, sua assistente de IA. " +
                        "Bem-vindo ao setor Futuro Tecnológico. Aproxime-se dos artefatos para analisá-los.",
        allDoneLine   = "Análise completa. Todos os artefatos do setor foram processados. " +
                        "Retorne ao hub central para prosseguir.",
        comments      = new List<string>
        {
            // índice 0 → Holograma
            "Fascinante. Esta interface elimina completamente a necessidade de telas físicas.",
            // índice 1 → Chip
            "Meus processadores são baseados nesta tecnologia. Bilhões de cálculos por nanosegundo.",
            // índice 2 → Exo
            "Este modelo amplia a força humana em 10x. A fronteira entre biologia e máquina desaparece.",
        },
        spawnOffset = new Vector3(2.2f, 0f, -1.5f),
    };

    // ════════════════════════════════════════════════════════════════════
    // ENTRY POINTS
    // ════════════════════════════════════════════════════════════════════

    [MenuItem("ChronoMundi/🧙 Adicionar Personagem Guia — Todas as Cenas")]
    public static void AddToAllScenes()
    {
        if (!EditorUtility.DisplayDialog("ChronoMundi — Personagem Guia v2",
            "Adiciona guias com artefatos completos e narrações:\n\n" +
            "• Pré-História → Uruk  (laranja)\n" +
            "• Idade Média  → Sir Aldric  (dourado)\n" +
            "• Futuro Tech  → ARIA  (ciano)\n\n" +
            "Artefatos da IdadeMedia serão criados do zero.\n" +
            "Artefatos existentes nas outras cenas terão narração atualizada.",
            "Adicionar tudo", "Cancelar"))
            return;

        float p = 0f;
        Process("Pré-História — Uruk...",    ref p, 0.28f, PREHISTORIA_CONFIG, PREHISTORIA_ARTIFACTS);
        Process("Idade Média — Sir Aldric...", ref p, 0.56f, IDADEMEDIA_CONFIG,   IDADEMEDIA_ARTIFACTS);
        Process("Futuro Tech — ARIA...",       ref p, 0.84f, FUTURO_CONFIG,        FUTURO_ARTIFACTS);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();

        EditorUtility.DisplayDialog("ChronoMundi ✅",
            "Personagens guia adicionados!\n\n" +
            "✔ Uruk na PreHistoriaScene\n" +
            "✔ Sir Aldric na IdadeMediaScene (artefatos criados)\n" +
            "✔ ARIA na FuturoTechScene\n\n" +
            "Para adicionar voz: selecione o guia na cena e arraste\n" +
            "AudioClips nos campos do CompanionCharacter.", "OK");
    }

    [MenuItem("ChronoMundi/🧙 Adicionar Personagem Guia — Cena Atual")]
    public static void AddToCurrentScene()
    {
        var scene = EditorSceneManager.GetActiveScene();
        var (cfg, arts) = scene.name switch
        {
            "PreHistoriaScene" => (PREHISTORIA_CONFIG, PREHISTORIA_ARTIFACTS),
            "IdadeMediaScene"  => (IDADEMEDIA_CONFIG,  IDADEMEDIA_ARTIFACTS),
            "FuturoTechScene"  => (FUTURO_CONFIG,      FUTURO_ARTIFACTS),
            _ => ((CompanionConfig)null, null)
        };

        if (cfg == null)
        {
            EditorUtility.DisplayDialog("ChronoMundi",
                $"Nenhuma configuração para '{scene.name}'.", "OK");
            return;
        }

        BuildAll(scene, cfg, arts);
        EditorUtility.DisplayDialog("ChronoMundi ✅",
            $"{cfg.companionName} adicionado a {scene.name}!", "OK");
    }

    // ════════════════════════════════════════════════════════════════════
    // FLUXO PRINCIPAL
    // ════════════════════════════════════════════════════════════════════

    static void Process(string msg, ref float p, float target,
        CompanionConfig cfg, List<ArtifactData> arts)
    {
        EditorUtility.DisplayProgressBar("ChronoMundi — Guia", msg, p);
        var scene = EditorSceneManager.OpenScene(SCENES_PATH + cfg.sceneName + ".unity", OpenSceneMode.Single);
        BuildAll(scene, cfg, arts);
        p = target;
    }

    static void BuildAll(UnityEngine.SceneManagement.Scene scene, CompanionConfig cfg, List<ArtifactData> arts)
    {
        // 1. Atualiza / cria artefatos
        UpdateArtifacts(scene, arts);

        // 2. Constrói o personagem guia
        BuildCompanion(scene, cfg, arts);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    // ════════════════════════════════════════════════════════════════════
    // ARTEFATOS
    // ════════════════════════════════════════════════════════════════════

    static void UpdateArtifacts(UnityEngine.SceneManagement.Scene scene, List<ArtifactData> arts)
    {
        foreach (var art in arts)
        {
            // Procura se já existe na cena
            GameObject existing = null;
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.name == art.goName) { existing = root; break; }
                var found = System.Array.Find(
                    root.GetComponentsInChildren<Transform>(true),
                    t => t.name == art.goName);
                if (found != null) { existing = found.gameObject; break; }
            }

            if (existing != null)
            {
                // Só atualiza narração e dados do InteracleObject
                var io = existing.GetComponent<InteracleObject>();
                if (io != null)
                {
                    io.artifactName        = art.artifactName;
                    io.artifactDescription = art.narration;
                    io.subtitleText        = art.subtitle;
                    EditorUtility.SetDirty(existing);
                }
                Debug.Log($"[CompanionBuilder] Narração atualizada: {art.goName}");
            }
            else
            {
                // Cria do zero (caso da IdadeMedia)
                CreateArtifact(scene, art);
                Debug.Log($"[CompanionBuilder] Artefato criado: {art.goName}");
            }
        }
    }

    static void CreateArtifact(UnityEngine.SceneManagement.Scene scene, ArtifactData art)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = art.goName;
        SceneManager.MoveGameObjectToScene(go, scene);
        go.transform.position   = art.position;
        go.transform.localScale = art.scale;

        int layer = LayerMask.NameToLayer("Interactable");
        if (layer >= 0) go.layer = layer;

        // Material
        var mat = AssetDatabase.LoadAssetAtPath<Material>(MAT_PATH + "Artifact_Metal.mat");
        if (mat != null) go.GetComponent<Renderer>().sharedMaterial = mat;

        var io = go.AddComponent<InteracleObject>();
        io.artifactName        = art.artifactName;
        io.artifactDescription = art.narration;
        io.subtitleText        = art.subtitle;
        io.interactOnlyOnce    = true;

        var hc = go.AddComponent<HighlightController>();
        hc.emissionColor  = art.glowColor;
        hc.pulseSpeed     = 2f;
        hc.minIntensity   = 0.05f;
        hc.maxIntensity   = 1.8f;
        hc.stopOnInteract = true;

        // Pedestal
        var ped = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ped.name = art.goName + "_Pedestal";
        SceneManager.MoveGameObjectToScene(ped, scene);
        ped.transform.position   = art.position + new Vector3(0, -art.scale.y * 0.5f - 0.35f, 0);
        ped.transform.localScale = new Vector3(0.4f, 0.32f, 0.4f);
        Object.DestroyImmediate(ped.GetComponent<Collider>());

        // Label WorldSpace acima do artefato
        AddWorldLabel(go, art.artifactName,
            new Vector3(0, art.scale.y * 0.5f + 0.3f, 0));

        EditorUtility.SetDirty(go);
    }

    // ════════════════════════════════════════════════════════════════════
    // PERSONAGEM GUIA
    // ════════════════════════════════════════════════════════════════════

    static void BuildCompanion(UnityEngine.SceneManagement.Scene scene,
        CompanionConfig cfg, List<ArtifactData> arts)
    {
        // Remove antigo
        foreach (var go in scene.GetRootGameObjects())
            if (go.name == "Companion_" + cfg.companionName)
                Object.DestroyImmediate(go);

        var root = new GameObject("Companion_" + cfg.companionName);
        SceneManager.MoveGameObjectToScene(root, scene);
        root.transform.position = GetPlayerSpawn(cfg.sceneName) + cfg.spawnOffset;

        // ── Corpo ─────────────────────────────────────────────────────────
        var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Body";
        body.transform.SetParent(root.transform, false);
        body.transform.localPosition = Vector3.zero;
        body.transform.localScale    = new Vector3(0.45f, 0.55f, 0.45f);
        Object.DestroyImmediate(body.GetComponent<Collider>());

        var mat = CreateCompanionMaterial(cfg);
        body.GetComponent<Renderer>().sharedMaterial = mat;

        // ── Cabeça ────────────────────────────────────────────────────────
        var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "Head";
        head.transform.SetParent(root.transform, false);
        head.transform.localPosition = new Vector3(0, 0.85f, 0);
        head.transform.localScale    = new Vector3(0.38f, 0.38f, 0.38f);
        Object.DestroyImmediate(head.GetComponent<Collider>());
        head.GetComponent<Renderer>().sharedMaterial = mat;

        // ── Olhos ─────────────────────────────────────────────────────────
        AddEye(head, new Vector3(-0.12f, 0.06f, 0.35f));
        AddEye(head, new Vector3( 0.12f, 0.06f, 0.35f));

        // ── Aura ──────────────────────────────────────────────────────────
        AddAura(root, cfg.color);

        // ── Name Tag (CORRIGIDO: rotação 180° em Y para ficar legível) ────
        var nameTag = BuildNameTag(root, cfg.companionName, cfg.color);

        // ── AudioSource ───────────────────────────────────────────────────
        var audio = root.AddComponent<AudioSource>();
        audio.spatialBlend = 0.6f;
        audio.minDistance  = 2f;
        audio.maxDistance  = 15f;
        audio.volume       = 0.85f;

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

        EditorUtility.SetDirty(root);
        Debug.Log($"[CompanionBuilder] {cfg.companionName} criado em {cfg.sceneName}.");
    }

    // ════════════════════════════════════════════════════════════════════
    // HELPERS VISUAIS
    // ════════════════════════════════════════════════════════════════════

    static Material CreateCompanionMaterial(CompanionConfig cfg)
    {
        string dir  = "Assets/Projeto/Assets/Materials/Companions/";
        string path = dir + cfg.companionName + "_Mat.mat";
        EnsureDirectory(dir);

        var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing != null)
        {
            existing.color = cfg.color;
            existing.SetColor("_EmissionColor", cfg.color * 0.5f);
            return existing;
        }

        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ??
                               Shader.Find("Standard"));
        mat.color = cfg.color;
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", cfg.color * 0.5f);
        AssetDatabase.CreateAsset(mat, path);
        return mat;
    }

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

        var ps   = aura.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startColor      = new ParticleSystem.MinMaxGradient(color * 0.7f, color);
        main.startSize       = new ParticleSystem.MinMaxCurve(0.04f, 0.12f);
        main.startLifetime   = new ParticleSystem.MinMaxCurve(1f, 2f);
        main.startSpeed      = new ParticleSystem.MinMaxCurve(0.08f, 0.25f);
        main.maxParticles    = 30;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = 10f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius    = 0.35f;
    }

    /// <summary>
    /// Constrói o name tag em WorldSpace.
    /// CORREÇÃO: rotaciona 180° em Y para que o texto fique legível
    /// quando o guia está de frente para o jogador (que fica atrás do guia).
    /// O FacePlayer() do CompanionCharacter faz o guia olhar para o jogador,
    /// então o forward do guia aponta PARA o jogador — sem a rotação o texto
    /// fica do lado errado. Com 180° em Y, o texto aponta para o jogador.
    /// </summary>
    static TextMeshProUGUI BuildNameTag(GameObject root, string name, Color color)
    {
        var tagGO = new GameObject("NameTag");
        tagGO.transform.SetParent(root.transform, false);
        tagGO.transform.localPosition = new Vector3(0, 1.45f, 0);
        // CORREÇÃO DO BUG: 180° em Y para o texto não aparecer espelhado/invertido
        tagGO.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        tagGO.transform.localScale    = Vector3.one * 0.01f;

        var canvas = tagGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        tagGO.AddComponent<UnityEngine.UI.CanvasScaler>();

        var tmp = tagGO.AddComponent<TextMeshProUGUI>();
        tmp.text         = name;
        tmp.fontSize     = 52;
        tmp.color        = color;
        tmp.fontStyle    = FontStyles.Bold;
        tmp.alignment    = TextAlignmentOptions.Center;
        tmp.outlineWidth = 0.2f;
        tmp.outlineColor = Color.black;

        var rt = tagGO.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(600, 120);

        return tmp;
    }

    static void AddWorldLabel(GameObject parent, string text, Vector3 localPos,
        float fontSize = 40, Color? color = null)
    {
        var go = new GameObject("Label");
        go.transform.SetParent(parent.transform, false);
        go.transform.localPosition = localPos;
        // Mesma correção: 180° para labels dos artefatos ficarem legíveis
        go.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        go.transform.localScale    = Vector3.one * 0.012f;

        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        go.AddComponent<UnityEngine.UI.CanvasScaler>();

        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = color ?? Color.white;
        tmp.fontStyle = FontStyles.Bold;

        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(500, 120);
    }

    // ════════════════════════════════════════════════════════════════════
    // UTILS
    // ════════════════════════════════════════════════════════════════════

    static Vector3 GetPlayerSpawn(string sceneName) => sceneName switch
    {
        "PreHistoriaScene" => new Vector3(0, 0, -4f),
        "IdadeMediaScene"  => new Vector3(0, 0, -4f),
        "FuturoTechScene"  => new Vector3(0, 0.3f, -4.5f),
        _                  => Vector3.zero
    };

    static void EnsureDirectory(string path)
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
#endif