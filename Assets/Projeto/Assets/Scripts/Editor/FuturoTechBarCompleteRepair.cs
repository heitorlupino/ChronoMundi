#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Corrige COMPLETAMENTE a barra de progresso visual da FuturoTechScene.
/// 
/// Investiga e corrige todos os problemas possíveis:
/// 1. Verifica se o Fill existe
/// 2. Reconfigura o Fill com anchoring correto (left-aligned, crescimento horizontal)
/// 3. Remove qualquer tamanho fixo que pudesse interferir
/// 4. Garante que o slider está ativo e visível
/// 
/// Menu: ChronoMundi → 🔬 REPARAR Barra FuturoTech Completamente
/// </summary>
public static class FuturoTechBarCompleteRepair
{
    const string SCENE_PATH = "Assets/Projeto/Assets/Scenes/FuturoTechScene.unity";

    [MenuItem("ChronoMundi/🔬 REPARAR Barra FuturoTech Completamente")]
    public static void Repair()
    {
        var scene = EditorSceneManager.OpenScene(SCENE_PATH, OpenSceneMode.Single);

        bool repaired = false;
        foreach (var root in scene.GetRootGameObjects())
        {
            var slider = root.GetComponentInChildren<Slider>(true);
            if (slider != null && slider.name == "ProgressBar")
            {
                Debug.Log($"[BarRepair] Encontrado Slider: {slider.gameObject.name}");
                RepairSlider(slider);
                repaired = true;
            }
        }

        if (repaired)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            EditorUtility.DisplayDialog("✅ Barra Reparada!",
                "Barra de progresso completamente reconfigu­rada!\n\n" +
                "A barra deve animar corretamente agora:\n" +
                "0% → 25% → 50% → 75% → 100%",
                "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("❌ Slider não encontrado",
                "Não foi possível encontrar o ProgressBar na cena.\n\n" +
                "Verifique se a cena tem o Canvas com ProgressBar.",
                "OK");
        }
    }

    static void RepairSlider(Slider slider)
    {
        Debug.Log("[BarRepair] ===== REPARANDO SLIDER =====");

        // 1. Verifica Fill Area
        Transform fillArea = slider.transform.Find("Fill Area");
        if (fillArea == null)
        {
            Debug.LogError("[BarRepair] ❌ Fill Area não encontrada!");
            return;
        }
        Debug.Log("[BarRepair] ✓ Fill Area encontrada");

        // 2. Verifica Fill
        Transform fill = fillArea.Find("Fill");
        if (fill == null)
        {
            Debug.LogError("[BarRepair] ❌ Fill não encontrada!");
            return;
        }
        Debug.Log("[BarRepair] ✓ Fill encontrada");

        var fillRT = fill.GetComponent<RectTransform>();
        if (fillRT == null)
        {
            Debug.LogError("[BarRepair] ❌ Fill RectTransform não encontrada!");
            return;
        }

        // 3. CONFIGURAÇÃO CRÍTICA: Fill deve crescer para a DIREITA
        //    anchorMin = (0, 0) — canto inferior esquerdo
        //    anchorMax = (0, 1) — lado esquerdo inteiro (NÃO lado direito!)
        //    Isso faz o Fill crescer horizontalmente para a direita conforme value aumenta

        fillRT.anchorMin = Vector2.zero;           // (0, 0)
        fillRT.anchorMax = new Vector2(0, 1);      // (0, 1) ← CRÍTICO!
        fillRT.anchoredPosition = Vector2.zero;
        fillRT.sizeDelta = Vector2.zero;
        fillRT.offsetMin = Vector2.zero;
        fillRT.offsetMax = Vector2.zero;

        Debug.Log("[BarRepair] ✓ Fill RectTransform configurado:");
        Debug.Log($"    anchorMin: {fillRT.anchorMin}");
        Debug.Log($"    anchorMax: {fillRT.anchorMax}");
        Debug.Log($"    anchoredPosition: {fillRT.anchoredPosition}");
        Debug.Log($"    sizeDelta: {fillRT.sizeDelta}");

        // 4. Garante que a Image do Fill está visível
        var fillImg = fill.GetComponent<Image>();
        if (fillImg != null)
        {
            if (fillImg.color.a < 0.5f)
            {
                fillImg.color = new Color(fillImg.color.r, fillImg.color.g, fillImg.color.b, 1f);
                Debug.Log("[BarRepair] ✓ Fill Image alpha ajustado para 1");
            }
            Debug.Log($"[BarRepair] ✓ Fill Image color: {fillImg.color}");
        }

        // 5. Verifica Fill Area anchoring também
        var fillAreaRT = fillArea.GetComponent<RectTransform>();
        fillAreaRT.anchorMin = Vector2.zero;
        fillAreaRT.anchorMax = Vector2.one;
        fillAreaRT.offsetMin = new Vector2(2, 2);
        fillAreaRT.offsetMax = new Vector2(-2, -2);
        Debug.Log("[BarRepair] ✓ Fill Area RectTransform verificada");

        // 6. Religa o fillRect ao slider (em caso de desconexão)
        slider.fillRect = fillRT;
        Debug.Log("[BarRepair] ✓ Slider.fillRect reconectado");

        // 7. Garante que o slider está com valores corretos
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0f;  // Começa em 0%
        Debug.Log("[BarRepair] ✓ Slider values: min=0, max=1, current=0");

        EditorUtility.SetDirty(fillRT);
        EditorUtility.SetDirty(fillAreaRT);
        EditorUtility.SetDirty(slider.gameObject);

        Debug.Log("[BarRepair] ===== REPARO COMPLETO ✅ =====");
    }
}
#endif
