#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Corrige a barra de progresso visual da FuturoTechScene.
/// 
/// Problema: O Fill do Slider tinha anchoring errado (anchorMax = Vector2.one)
///           Isso fazia o Fill ocupar sempre 100% em vez de crescer com o slider.
/// 
/// Solução: Reconfigura o Fill para ter anchorMax = new Vector2(0, 1)
///         Assim a barra cresce/encolhe corretamente com o progresso.
/// 
/// Menu: ChronoMundi → 🔬 CORRIGIR Slider FuturoTech
/// </summary>
public static class FuturoTechSliderFix
{
    const string SCENE_PATH = "Assets/Projeto/Assets/Scenes/FuturoTechScene.unity";

    [MenuItem("ChronoMundi/🔬 CORRIGIR Slider FuturoTech")]
    public static void Fix()
    {
        var scene = EditorSceneManager.OpenScene(SCENE_PATH, OpenSceneMode.Single);

        bool fixed_slider = false;
        foreach (var root in scene.GetRootGameObjects())
        {
            var slider = root.GetComponentInChildren<Slider>(true);
            if (slider != null && slider.name == "ProgressBar")
            {
                FixSliderFill(slider);
                fixed_slider = true;
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
        }

        if (fixed_slider)
        {
            EditorUtility.DisplayDialog("✅ Slider Corrigido",
                "Barra de progresso visual corrigida!\n\n" +
                "O Fill agora vai animar corretamente de 0% a 100%.",
                "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("❌ Slider não encontrado",
                "Não foi possível encontrar o ProgressBar na cena.",
                "OK");
        }
    }

    static void FixSliderFill(Slider slider)
    {
        if (slider.fillRect == null)
        {
            Debug.LogError("[SliderFix] fillRect é null!");
            return;
        }

        var fillRT = slider.fillRect.GetComponent<RectTransform>();
        if (fillRT == null)
        {
            Debug.LogError("[SliderFix] fillRT é null!");
            return;
        }

        // CORREÇÃO: O Fill deve ter apenas a altura preenchida
        // A largura deve ser controlada pelo slider dinamicamente
        fillRT.anchorMin = Vector2.zero;           // Canto inferior esquerdo
        fillRT.anchorMax = new Vector2(0, 1);      // AQUI! Só a altura, width = 0
        fillRT.offsetMin = Vector2.zero;
        fillRT.offsetMax = Vector2.zero;

        Debug.Log("[SliderFix] ✅ Slider Fill reconfigurado!");
        Debug.Log($"  anchorMin: {fillRT.anchorMin}");
        Debug.Log($"  anchorMax: {fillRT.anchorMax}");
        Debug.Log($"  offsetMin: {fillRT.offsetMin}");
        Debug.Log($"  offsetMax: {fillRT.offsetMax}");

        EditorUtility.SetDirty(fillRT);
    }
}
#endif
