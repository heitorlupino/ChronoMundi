using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Menu de pausa do ChronoMundi.
///
/// CORREÇÕES v2:
///   - GoToMenu: restaura timeScale ANTES do fade para que as coroutines
///     do SceneFader não travem (Time.timeScale = 0 congela coroutines normais)
///   - Fallback direto para SceneManager.LoadScene se SceneFader não existir
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

    [Header("Cena do Menu")]
    public string menuSceneName = "MenuScene";

    [Header("Áudio")]
    [Range(0f, 1f)]
    public float pausedTimeScale = 0f;

    // ─────────────────────────────────────────────────────────────────────

    void Start()
    {
        if (Instance == null) Instance = this;

        SetPaused(false);
        if (btnResume != null) btnResume.onClick.AddListener(Resume);
        if (btnMenu   != null) btnMenu.onClick.AddListener(GoToMenu);
        if (btnQuit   != null) btnQuit.onClick.AddListener(QuitGame);
    }

    void OnDestroy()
    {
        if (btnResume != null) btnResume.onClick.RemoveListener(Resume);
        if (btnMenu   != null) btnMenu.onClick.RemoveListener(GoToMenu);
        if (btnQuit   != null) btnQuit.onClick.RemoveListener(QuitGame);
    }

    void Update()
    {
        if (PauseKeyPressed()) TogglePause();
    }

    // ── API ───────────────────────────────────────────────────────────────

    public void TogglePause() => SetPaused(!isPaused);
    public void Resume()      => SetPaused(false);

    public void SetPaused(bool paused)
    {
        isPaused       = paused;
        Time.timeScale = paused ? pausedTimeScale : 1f;

        if (pausePanel != null) pausePanel.SetActive(paused);

        Cursor.lockState = paused ? CursorLockMode.None   : CursorLockMode.Locked;
        Cursor.visible   = paused;

        if (paused && NarratorSystem.Instance != null)
            NarratorSystem.Instance.StopNarration();
    }

    void GoToMenu()
    {
        // Restaura estado antes de qualquer carregamento
        Time.timeScale   = 1f;
        isPaused         = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        if (pausePanel != null) pausePanel.SetActive(false);
        if (NarratorSystem.Instance != null) NarratorSystem.Instance.StopNarration();

        // Carrega direto — sem depender de SceneFader ou LoadingScreen
        UnityEngine.SceneManagement.SceneManager.LoadScene(menuSceneName);
    }

    void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    bool PauseKeyPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.Escape);
#endif
    }
}