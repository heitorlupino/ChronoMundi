#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Corrige a IdadeMediaScene:
///   - Cria os 3 artefatos (Pergaminho, Escudo, Espada) que estão null no ExhibitManager
///   - Adiciona SimpleReturnToMuseum (saída da cena)
///   - Reconecta tudo: ExhibitManager → exhibits[], TimelineEra → returnPoint
///
/// Menu: ChronoMundi → 🔧 Corrigir IdadeMediaScene
/// </summary>
public static class ChronoMundiIdadeMediaFixer
{
    const string SCENE_PATH = "Assets/Projeto/Assets/Scenes/IdadeMediaScene.unity";
    const string MAT_PATH   = "Assets/Projeto/Assets/Materials/Environment/";

    // ── Cores ─────────────────────────────────────────────────────────────
    static readonly Color UI_WHITE = Color.white;
    static readonly Color UI_GOLD  = new Color(1f, 0.85f, 0.2f, 1f);

    // ════════════════════════════════════════════════════════════════════
    [MenuItem("ChronoMundi/🔧 Corrigir IdadeMediaScene (Exhibits + Saída)")]
    public static void Fix()
    {
        if (!EditorUtility.DisplayDialog("ChronoMundi — Corrigir IdadeMediaScene",
            "Isso vai:\n" +
            "  • Criar os 3 artefatos da Idade Média\n" +
            "  • Adicionar a saída (SimpleReturnToMuseum)\n" +
            "  • Reconectar ExhibitManager e TimelineEra\n\n" +
            "Continuar?", "Sim, corrigir", "Cancelar"))
            return;

        EditorUtility.DisplayProgressBar("ChronoMundi", "Abrindo IdadeMediaScene...", 0.1f);
        var scene = EditorSceneManager.OpenScene(SCENE_PATH, OpenSceneMode.Single);

        EditorUtility.DisplayProgressBar("ChronoMundi", "Criando artefatos...", 0.35f);
        var exhibits = RebuildExhibits(scene);

        EditorUtility.DisplayProgressBar("ChronoMundi", "Criando saída...", 0.6f);
        var returnPoint = BuildReturnPoint(scene);

        EditorUtility.DisplayProgressBar("ChronoMundi", "Reconectando componentes...", 0.8f);
        ReconnectComponents(scene, exhibits, returnPoint);

        EditorUtility.DisplayProgressBar("ChronoMundi", "Salvando...", 0.95f);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        AssetDatabase.SaveAssets();
        EditorUtility.ClearProgressBar();

        EditorUtility.DisplayDialog("ChronoMundi ✅",
            "IdadeMediaScene corrigida!\n\n" +
            "✔ 3 artefatos criados\n" +
            "✔ SimpleReturnToMuseum adicionado\n" +
            "✔ ExhibitManager reconectado\n" +
            "✔ TimelineEra reconectado\n\n" +
            "Execute 'ChronoMundi → ✅ Validar Projeto' para confirmar.",
            "OK");
    }

    // ════════════════════════════════════════════════════════════════════
    // RECRIA OS 3 ARTEFATOS
    // ════════════════════════════════════════════════════════════════════

    static List<InteracleObject> RebuildExhibits(Scene scene)
    {
        var list = new List<InteracleObject>();

        // Remove artefatos antigos se existirem (podem estar sem componente)
        string[] exhibitNames = { "Artefato_Pergaminho", "Artefato_Escudo", "Artefato_Espada" };
        foreach (var name in exhibitNames)
            foreach (var go in scene.GetRootGameObjects())
                if (go.name == name) Object.DestroyImmediate(go);

        // ── Pergaminho Iluminado ─────────────────────────────────────────
        list.Add(CreateExhibit(scene,
            goName:      "Artefato_Pergaminho",
            artifactName:"Pergaminho Iluminado",
            description: "Manuscritos produzidos à mão por monges medievais entre os séculos V e XV. " +
                         "Preservaram o conhecimento clássico e eram obras de arte riquíssimas em detalhes.",
            subtitle:    "Os livros da Idade Média — feitos à mão por anos",
            position:    new Vector3(0f, 1.15f, 4.5f),
            scale:       new Vector3(0.38f, 0.03f, 0.28f),
            shape:       PrimitiveType.Cube,
            matName:     "Artifact_Bone",
            glowColor:   new Color(1f, 0.85f, 0.1f),
            pulseSpeed:  1.8f));

        // ── Escudo Heráldico ─────────────────────────────────────────────
        list.Add(CreateExhibit(scene,
            goName:      "Artefato_Escudo",
            artifactName:"Escudo Heráldico",
            description: "Os brasões medievais eram identidades visuais de famílias nobres. " +
                         "Cada símbolo, cor e animal tinha um significado preciso, registrado " +
                         "pelos heráldicos da época.",
            subtitle:    "Identidade e honra representadas em metal",
            position:    new Vector3(-3f, 1.3f, 2f),
            scale:       new Vector3(0.55f, 0.7f, 0.09f),
            shape:       PrimitiveType.Cube,
            matName:     "Artifact_Metal",
            glowColor:   new Color(1f, 0.1f, 0.1f),
            pulseSpeed:  2f));

        // ── Espada de Cavaleiro ──────────────────────────────────────────
        list.Add(CreateExhibit(scene,
            goName:      "Artefato_Espada",
            artifactName:"Espada de Cavaleiro",
            description: "A espada era o símbolo máximo do cavaleiro medieval. Forjada por " +
                         "ferreiros especializados, levava semanas para ser produzida e era " +
                         "passada de geração em geração como relíquia.",
            subtitle:    "Símbolo de poder e nobreza da Idade Média",
            position:    new Vector3(3f, 1.2f, 2f),
            scale:       new Vector3(0.08f, 0.75f, 0.06f),
            shape:       PrimitiveType.Cube,
            matName:     "Artifact_Metal",
            glowColor:   new Color(0.7f, 0.8f, 1f),
            pulseSpeed:  2.4f));

        Debug.Log($"[IdadeMediaFixer] {list.Count} artefatos criados.");
        return list;
    }

    static InteracleObject CreateExhibit(
        Scene scene, string goName, string artifactName, string description,
        string subtitle, Vector3 position, Vector3 scale,
        PrimitiveType shape, string matName, Color glowColor, float pulseSpeed)
    {
        var go = shape switch
        {
            PrimitiveType.Sphere   => GameObject.CreatePrimitive(PrimitiveType.Sphere),
            PrimitiveType.Cylinder => GameObject.CreatePrimitive(PrimitiveType.Cylinder),
            PrimitiveType.Capsule  => GameObject.CreatePrimitive(PrimitiveType.Capsule),
            _                      => GameObject.CreatePrimitive(PrimitiveType.Cube)
        };

        go.name = goName;
        SceneManager.MoveGameObjectToScene(go, scene);
        go.transform.position   = position;
        go.transform.localScale = scale;

        // Layer
        int layer = LayerMask.NameToLayer("Interactable");
        if (layer >= 0) go.layer = layer;

        // Material
        var mat = AssetDatabase.LoadAssetAtPath<Material>($"{MAT_PATH}{matName}.mat");
        if (mat != null) go.GetComponent<Renderer>().sharedMaterial = mat;

        // InteracleObject
        var io = go.AddComponent<InteracleObject>();
        io.artifactName        = artifactName;
        io.artifactDescription = description;
        io.subtitleText        = subtitle;
        io.interactOnlyOnce    = true;

        // HighlightController
        var hc = go.AddComponent<HighlightController>();
        hc.emissionColor  = glowColor;
        hc.pulseSpeed     = pulseSpeed;
        hc.minIntensity   = 0.05f;
        hc.maxIntensity   = 1.8f;
        hc.stopOnInteract = true;

        // Pedestal
        var pedestal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pedestal.name = $"{goName}_Pedestal";
        SceneManager.MoveGameObjectToScene(pedestal, scene);
        pedestal.transform.position   = position + new Vector3(0, -scale.y * 0.5f - 0.3f, 0);
        pedestal.transform.localScale = new Vector3(0.4f, 0.3f, 0.4f);
        Object.DestroyImmediate(pedestal.GetComponent<Collider>());

        var pedestalMat = AssetDatabase.LoadAssetAtPath<Material>($"{MAT_PATH}Pillar_Stone.mat");
        if (pedestalMat != null)
            pedestal.GetComponent<Renderer>().sharedMaterial = pedestalMat;

        // Label flutuante com nome do artefato
        AddWorldLabel(go, artifactName, new Vector3(0, scale.y * 0.5f + 0.25f, 0));

        return io;
    }

    // ════════════════════════════════════════════════════════════════════
    // SIMPLERETURNTOMUSEUM (SAÍDA)
    // ════════════════════════════════════════════════════════════════════

    static SimpleReturnToMuseum BuildReturnPoint(Scene scene)
    {
        // Remove antigo se existir
        foreach (var go in scene.GetRootGameObjects())
            if (go.name == "SaidaMuseu") Object.DestroyImmediate(go);

        var exitGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
        exitGO.name = "SaidaMuseu";
        SceneManager.MoveGameObjectToScene(exitGO, scene);
        exitGO.transform.position   = new Vector3(0, 1.5f, -5.5f);
        exitGO.transform.localScale = new Vector3(1.8f, 3f, 0.25f);

        int layer = LayerMask.NameToLayer("Interactable");
        if (layer >= 0) exitGO.layer = layer;

        var mat = AssetDatabase.LoadAssetAtPath<Material>($"{MAT_PATH}Door_Wood.mat");
        if (mat != null) exitGO.GetComponent<Renderer>().sharedMaterial = mat;

        // SimpleReturnToMuseum
        var srtm = exitGO.AddComponent<SimpleReturnToMuseum>();
        srtm.museumSceneName  = "MuseuHubScene";
        srtm.activationMode   = SimpleReturnToMuseum.ActivationMode.Interactable;
        srtm.exitSubtitleText = "Retornando ao Museu...";
        srtm.askConfirmation  = false;

        // HighlightController (começa desativado — TimelineEra ativa ao completar)
        var hc = exitGO.AddComponent<HighlightController>();
        hc.emissionColor  = new Color(0.2f, 1f, 0.5f);
        hc.pulseSpeed     = 1.5f;
        hc.stopOnInteract = false;
        hc.enabled        = false;

        // Label "SAÍDA"
        AddWorldLabel(exitGO, "[ E ] SAIR", new Vector3(0, 1.8f, 0), 80, new Color(0.8f, 1f, 0.8f));

        Debug.Log("[IdadeMediaFixer] SimpleReturnToMuseum criado.");
        return srtm;
    }

    // ════════════════════════════════════════════════════════════════════
    // RECONECTA EXHIBITMANAGER + TIMELINEERA
    // ════════════════════════════════════════════════════════════════════

    static void ReconnectComponents(Scene scene, List<InteracleObject> exhibits, SimpleReturnToMuseum returnPoint)
    {
        ExhibitManager   em  = null;
        TimelineEra      era = null;

        foreach (var root in scene.GetRootGameObjects())
        {
            if (em  == null) em  = root.GetComponentInChildren<ExhibitManager>(true);
            if (era == null) era = root.GetComponentInChildren<TimelineEra>(true);
        }

        // ── ExhibitManager ─────────────────────────────────────────────
        if (em != null)
        {
            em.exhibits = exhibits;

            // Garante highlight material
            if (em.highlightMaterial == null)
            {
                var hlMat = AssetDatabase.LoadAssetAtPath<Material>(
                    "Assets/Projeto/Assets/Materials/Highlight_Interactable.mat");
                if (hlMat != null) em.highlightMaterial = hlMat;
            }

            // Conecta ao TimelineEra
            if (era != null) em.eraController = era;

            EditorUtility.SetDirty(em);
            Debug.Log($"[IdadeMediaFixer] ExhibitManager reconectado com {exhibits.Count} exhibits.");
        }
        else
        {
            Debug.LogError("[IdadeMediaFixer] ExhibitManager não encontrado na cena!");
        }

        // ── TimelineEra ────────────────────────────────────────────────
        if (era != null)
        {
            era.eraName             = "Idade Média";
            era.introSubtitle       = "Bem-vindo à Idade Média.\nExplore as relíquias deste período histórico.";
            era.delayBeforeNarration = 1.2f;
            era.returnPoint         = returnPoint;

            // Conecta DesktopPlayerController se existir
            foreach (var root in scene.GetRootGameObjects())
            {
                var dpc = root.GetComponentInChildren<DesktopPlayerController>(true);
                if (dpc != null)
                {
                    era.playerController = dpc;
                    break;
                }
            }

            EditorUtility.SetDirty(era);
            Debug.Log("[IdadeMediaFixer] TimelineEra reconectado.");
        }
        else
        {
            Debug.LogWarning("[IdadeMediaFixer] TimelineEra não encontrado. Criando...");
            var eraGO = new GameObject("TimelineEra");
            SceneManager.MoveGameObjectToScene(eraGO, scene);
            era = eraGO.AddComponent<TimelineEra>();
            era.eraName       = "Idade Média";
            era.introSubtitle = "Bem-vindo à Idade Média.\nExplore as relíquias deste período histórico.";
            era.returnPoint   = returnPoint;
            if (em != null) em.eraController = era;
        }

        // ── Marca todos como dirty para o Unity salvar ─────────────────
        if (returnPoint != null) EditorUtility.SetDirty(returnPoint.gameObject);
        foreach (var io in exhibits)   EditorUtility.SetDirty(io.gameObject);
    }

    // ════════════════════════════════════════════════════════════════════
    // HELPER — Label flutuante WorldSpace
    // ════════════════════════════════════════════════════════════════════

    static void AddWorldLabel(GameObject parent, string text,
        Vector3 localPos, float fontSize = 40, Color? color = null)
    {
        var labelGO = new GameObject("Label");
        labelGO.transform.SetParent(parent.transform, false);
        labelGO.transform.localPosition = localPos;
        labelGO.transform.localScale    = Vector3.one * 0.012f;

        var canvas = labelGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        labelGO.AddComponent<CanvasScaler>();

        var tmp = labelGO.AddComponent<TMPro.TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = fontSize;
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.color     = color ?? Color.white;
        tmp.fontStyle = TMPro.FontStyles.Bold;

        var rt = labelGO.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(500, 120);
    }
}
#endif
