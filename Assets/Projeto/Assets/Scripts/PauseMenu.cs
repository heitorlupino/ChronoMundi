using UnityEngine;
using UnityEngine.UI;
using TMPro;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Menu de pausa do ChronoMundi.
/// Abre/fecha com Esc (Desktop) ou botão Menu do controller (VR).
/// Permite: retomar jogo, ajustar volume ou voltar ao menu principal.
///
/// Como usar:
///   1. Adicione este script a um Canvas de pausa (inicialmente inativo).
///   2. Configure os botões no Inspector.
///   3. O pausa pode estar no mesmo prefab do GameManager ou em cada cena.
/// </summary>
public class PauseMenu : MonoBehaviour
{
    public static PauseMenu Instance { get; private set; }
    [Header("Painel de Pausa")]
    public GameObject pausePanel;
    public bool isPaused = false;

    [Header("Botões")]
    public Button btnResume;
    public Button btnMenu;
    public Button btnQuit;

    [Header("Áudio")]
    [Range(0f, 1f)]
    public float pausedTimeScale = 0f; // 0 = pausa total; 0.1 = câmera lenta

    [Header("Cena do Menu")]
    public string menuSceneName = "MenuScene";

    // ──────────────────────────────────────────────────────────────────────
    void Start()
    {
        // Registra instância para acesso global
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[PauseMenu] Outra instância já existe. Esta será destruída.");
            Destroy(this.gameObject);
            return;
        }
        Instance = this;

        SetPaused(false);

        if (btnResume != null) btnResume.onClick.AddListener(Resume);
        if (btnMenu != null) btnMenu.onClick.AddListener(GoToMenu);
        if (btnQuit != null) btnQuit.onClick.AddListener(QuitGame);
    }

    void OnDestroy()
    {
        if (btnResume != null) btnResume.onClick.RemoveListener(Resume);
        if (btnMenu != null) btnMenu.onClick.RemoveListener(GoToMenu);
        if (btnQuit != null) btnQuit.onClick.RemoveListener(QuitGame);

        if (Instance == this) Instance = null;
    }

    void Update()
    {
        if (PauseKeyPressed())
            TogglePause();
    }

    // ── API pública ────────────────────────────────────────────────────────
    public void TogglePause() => SetPaused(!isPaused);
    public void Resume() => SetPaused(false);

    public void SetPaused(bool paused)
    {
        isPaused = paused;

        Time.timeScale = paused ? pausedTimeScale : 1f;

        if (pausePanel != null)
            pausePanel.SetActive(paused);

        // Cursor visível no pause, capturado no jogo (Desktop)
        Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = paused;

        // Para a narração durante a pausa
        if (paused && NarratorSystem.Instance != null)
            NarratorSystem.Instance.StopNarration();
    }

    void GoToMenu()
    {
        SetPaused(false); // restaura timescale antes de carregar
        SceneFader.FadeToScene(menuSceneName);
    }

    void QuitGame()
    {
        Debug.Log("[PauseMenu] Saindo do jogo...");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ── Input ──────────────────────────────────────────────────────────────
    bool PauseKeyPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.Escape);
#endif
    }
}
