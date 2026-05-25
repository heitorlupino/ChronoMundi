#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// DEBUG: Mostra a estrutura exata da barra e identifica o problema.
/// </summary>
public static class FuturoTechBarDebug
{
    const string SCENE_PATH = "Assets/Projeto/Assets/Scenes/FuturoTechScene.unity";

    [MenuItem("ChronoMundi/🔍 DEBUG Barra FuturoTech")]
    public static void DebugBar()
    {
        var scene = EditorSceneManager.OpenScene(SCENE_PATH, OpenSceneMode.Single);

        UnityEngine.Debug.Log("===== DEBUG BARRA FUTUROTECH =====\n");

        // 1. Encontra ExhibitManager
        ExhibitManager em = null;
        foreach (var root in scene.GetRootGameObjects())
        {
            em = root.GetComponentInChildren<ExhibitManager>(true);
            if (em != null) break;
        }

        if (em == null)
        {
            UnityEngine.Debug.LogError("❌ ExhibitManager NÃO ENCONTRADO!");
            return;
        }

        UnityEngine.Debug.Log($"✅ ExhibitManager encontrado");
        UnityEngine.Debug.Log($"   Exhibits count: {em.exhibits.Count}");
        UnityEngine.Debug.Log($"   progressPanel: {(em.progressPanel != null ? em.progressPanel.name : "NULL")}");
        UnityEngine.Debug.Log($"   progressText: {(em.progressText != null ? em.progressText.name : "NULL")}");
        UnityEngine.Debug.Log($"   progressBar: {(em.progressBar != null ? em.progressBar.name : "NULL")}");

        if (em.progressBar == null)
        {
            UnityEngine.Debug.LogError("\n❌ progressBar É NULL! Esse é o problema!");
            UnityEngine.Debug.Log("   Solução: Rode 'RECONSTRUIR Barra FuturoTech'");
            return;
        }

        // 2. Analisa o Slider
        Slider slider = em.progressBar;
        UnityEngine.Debug.Log($"\n✅ Slider encontrado: {slider.gameObject.name}");
        UnityEngine.Debug.Log($"   value: {slider.value}");
        UnityEngine.Debug.Log($"   minValue: {slider.minValue}");
        UnityEngine.Debug.Log($"   maxValue: {slider.maxValue}");
        UnityEngine.Debug.Log($"   fillRect: {(slider.fillRect != null ? slider.fillRect.name : "NULL")}");

        if (slider.fillRect == null)
        {
            UnityEngine.Debug.LogError("\n❌ fillRect É NULL! A barra não consegue animar!");
            return;
        }

        // 3. Analisa o Fill
        RectTransform fillRT = slider.fillRect;
        UnityEngine.Debug.Log($"\n✅ Fill encontrado: {fillRT.gameObject.name}");
        UnityEngine.Debug.Log($"   anchorMin: {fillRT.anchorMin}");
        UnityEngine.Debug.Log($"   anchorMax: {fillRT.anchorMax}");
        UnityEngine.Debug.Log($"   sizeDelta: {fillRT.sizeDelta}");
        UnityEngine.Debug.Log($"   anchoredPosition: {fillRT.anchoredPosition}");

        // 4. Verifica se o Fill tem Image
        Image fillImg = fillRT.GetComponent<Image>();
        if (fillImg == null)
        {
            UnityEngine.Debug.LogError("\n❌ Fill NÃO tem Image component!");
            return;
        }
        UnityEngine.Debug.Log($"\n✅ Fill Image encontrada");
        UnityEngine.Debug.Log($"   color: {fillImg.color}");
        UnityEngine.Debug.Log($"   alpha: {fillImg.color.a}");

        // 5. Test: Simula um clique
        UnityEngine.Debug.Log("\n===== TESTE =====");
        UnityEngine.Debug.Log("Agora vou simular um clique para ver se o slider muda...\n");

        slider.value = 0.25f;
        UnityEngine.Debug.Log($"Slider.value setado para 0.25 (25%)");
        UnityEngine.Debug.Log($"Slider.value agora é: {slider.value}");
        UnityEngine.Debug.Log($"Fill rectTransform width deve ser 25% da área...\n");

        EditorUtility.DisplayDialog("🔍 Debug Completo",
            "Verifique o Console para detalhes completos.",
            "OK");
    }
}
#endif
