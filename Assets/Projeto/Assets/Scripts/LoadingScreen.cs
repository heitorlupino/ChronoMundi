using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Carregamento assíncrono de cenas com tela de loading.
/// Use LoadingScreen.LoadScene("NomeDaCena") a partir de qualquer script.
/// Adicione este script em um Canvas com DontDestroyOnLoad, ou crie uma
/// cena intermediária "LoadingScene" com este script.
/// </summary>
public class LoadingScreen : MonoBehaviour
{
    public static LoadingScreen Instance { get; private set; }

    // Indica se já existe um carregamento em andamento para evitar reentradas
    bool _isLoading = false;

    public static bool IsLoading => Instance != null && Instance._isLoading;

    [Header("UI de Loading")]
    public GameObject loadingPanel;
    public Slider     progressBar;
    public TextMeshProUGUI progressText;
    public TextMeshProUGUI tipText;

    [Header("Dicas que aparecem durante o loading")]
    [TextArea]
    public string[] loadingTips = new string[]
    {
        "Explore todos os artefatos para descobrir a história completa.",
        "Cada período histórico tem segredos escondidos!",
        "Pressione E para interagir com os objetos do museu.",
        "A narrativa muda de acordo com os itens que você explora."
    };

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

        if (loadingPanel != null)
            loadingPanel.SetActive(false);
    }

    // ── API estática — use isso de qualquer script ────────────────────────
    public static void LoadScene(string sceneName)
    {
        if (IsLoading)
        {
            Debug.Log($"[LoadingScreen] Já há um carregamento em andamento. Ignorando '{sceneName}'.");
            return;
        }

        if (Instance != null)
        {
            Instance._isLoading = true;
            Instance.StartCoroutine(Instance.LoadAsync(sceneName));
        }
        else
        {
            SceneManager.LoadScene(sceneName); // fallback sem loading screen
        }
    }

    // ── Coroutine de carregamento ──────────────────────────────────────────
    IEnumerator LoadAsync(string sceneName)
    {
        // Mostra painel
        if (loadingPanel != null) loadingPanel.SetActive(true);

        // Exibe dica aleatória
        if (tipText != null && loadingTips.Length > 0)
            tipText.text = loadingTips[Random.Range(0, loadingTips.Length)];

        // Reseta barra
        if (progressBar  != null) progressBar.value = 0f;
        if (progressText != null) progressText.text  = "0%";

        yield return null; // espera um frame para a UI atualizar

        // Inicia carregamento assíncrono
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false; // segura a cena para exibir 100%

        while (!op.isDone)
        {
            // Unity vai de 0 a 0.9 enquanto carrega; normaliza para 0-1
            float progress = Mathf.Clamp01(op.progress / 0.9f);

            if (progressBar  != null) progressBar.value = progress;
            if (progressText != null) progressText.text  = $"{Mathf.RoundToInt(progress * 100)}%";

            // Quando chegar a 90% (cena pronta), ativa
            if (op.progress >= 0.9f)
            {
                if (progressBar  != null) progressBar.value = 1f;
                if (progressText != null) progressText.text  = "100%";

                yield return new WaitForSeconds(0.3f); // pequena pausa para o jogador ler

                op.allowSceneActivation = true;
            }

            yield return null;
        }

        // Esconde painel
        if (loadingPanel != null) loadingPanel.SetActive(false);
        // Libera flag de carregamento para permitir novos loads
        _isLoading = false;
        }

    }