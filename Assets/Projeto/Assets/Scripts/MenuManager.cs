using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Gerencia a MenuScene: iniciar jogo, sair e exibir informações.
/// Attach em um GameObject vazio na MenuScene.
/// </summary>
public class MenuManager : MonoBehaviour
{
    [Header("Botões")]
    public Button btnPlay;
    public Button btnQuit;

    [Header("Cenas")]
    public string hubSceneName = "MuseuHubScene";

    [Header("UI Opcional")]
    public TextMeshProUGUI titleText;
    public GameObject loadingIndicator;

    void Start()
    {
        // Garante que o cursor está visível no menu
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        if (loadingIndicator != null)
            loadingIndicator.SetActive(false);

        // Registra os listeners nos botões
        if (btnPlay != null) btnPlay.onClick.AddListener(OnPlayClicked);
        if (btnQuit != null) btnQuit.onClick.AddListener(OnQuitClicked);

        // Reseta progresso ao voltar para o menu
        if (GameManager.Instance != null)
            GameManager.Instance.ResetProgress();
    }

    void OnPlayClicked()
    {
        Debug.Log("[MenuManager] Iniciando jogo...");

        if (btnPlay != null) btnPlay.interactable = false;
        if (loadingIndicator != null) loadingIndicator.SetActive(true);

        LoadingScreen.LoadScene(hubSceneName);
    }

    void OnQuitClicked()
    {
        Debug.Log("[MenuManager] Saindo do jogo...");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void OnDestroy()
    {
        if (btnPlay != null) btnPlay.onClick.RemoveListener(OnPlayClicked);
        if (btnQuit != null) btnQuit.onClick.RemoveListener(OnQuitClicked);
    }
}