using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Fade preto suave entre cenas.
/// Integra-se automaticamente com o LoadingScreen.
///
/// Como usar:
///   - Adicione ao prefab LoadingScreen_Prefab (junto com LoadingScreen.cs).
///   - Chame SceneFader.FadeToScene("NomeDaCena") em vez de LoadingScreen.LoadScene().
///   - Ou use apenas FadeIn() / FadeOut() manualmente.
///
/// Requer:
///   - Um Canvas com um Image de tela cheia (cor preta) atribuído a 'fadeImage'.
///   - O Canvas deve ter Sort Order alto para ficar na frente de tudo.
/// </summary>
public class SceneFader : MonoBehaviour
{
    public static SceneFader Instance { get; private set; }

    [Header("UI")]
    [Tooltip("Image de tela cheia para o fade (cor preta, alpha 0 no início).")]
    public Image fadeImage;

    [Header("Configuração")]
    public float fadeDuration = 0.4f;

    // ──────────────────────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Começa transparente
        SetAlpha(0f);
    }

    void Start()
    {
        // Fade in automático ao carregar qualquer cena
        FadeIn();
    }

    // ── API pública ────────────────────────────────────────────────────────

    /// <summary>Fade para preto → carrega cena → fade saindo do preto.</summary>
    public static void FadeToScene(string sceneName)
    {
        if (Instance != null)
            Instance.StartCoroutine(Instance.FadeAndLoad(sceneName));
        else
            LoadingScreen.LoadScene(sceneName); // fallback sem fader
    }

    /// <summary>Fade de preto para transparente (entrada na cena).</summary>
    public void FadeIn() => StartCoroutine(Fade(1f, 0f));

    /// <summary>Fade de transparente para preto (saída da cena).</summary>
    public void FadeOut() => StartCoroutine(Fade(0f, 1f));

    // ──────────────────────────────────────────────────────────────────────
    IEnumerator FadeAndLoad(string sceneName)
    {
        yield return Fade(0f, 1f);          // escurece
        LoadingScreen.LoadScene(sceneName); // carrega (LoadingScreen cuida do resto)
        // O fade de entrada é disparado automaticamente no Start() da próxima cena
    }

    IEnumerator Fade(float from, float to)
    {
        if (fadeImage == null) yield break;

        float elapsed = 0f;
        SetAlpha(from);

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            SetAlpha(Mathf.Lerp(from, to, elapsed / fadeDuration));
            yield return null;
        }

        SetAlpha(to);
    }

    void SetAlpha(float alpha)
    {
        if (fadeImage == null) return;
        Color c = fadeImage.color;
        c.a = alpha;
        fadeImage.color = c;
        // Desativa o raycast quando invisível para não bloquear input
        fadeImage.raycastTarget = alpha > 0.01f;
    }
}
