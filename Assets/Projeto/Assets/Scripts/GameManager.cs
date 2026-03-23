using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controle central do jogo. Persiste entre cenas via DontDestroyOnLoad.
/// Guarda progresso do jogador e expõe eventos para outros sistemas.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Cenas do Projeto")]
    public string menuScene       = "MenuScene";
    public string hubScene        = "MuseuHubScene";
    public string preHistoriaScene = "PreHistoriaScene";
    public string idadeMediaScene  = "IdadeMediaScene";
    public string futuroTechScene  = "FuturoTechScene";

    // ── Progresso ──────────────────────────────────────────────────────────
    private HashSet<string> _visitedScenes    = new HashSet<string>();
    private Dictionary<string, int> _exhibitsInteracted = new Dictionary<string, int>();
    private Dictionary<string, int> _exhibitsTotal      = new Dictionary<string, int>();

    // ── Eventos (outros scripts podem escutar) ─────────────────────────────
    public System.Action<string> OnSceneVisited;
    public System.Action<string, int, int> OnExhibitProgress; // cena, atual, total

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
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    // ── Chamado automaticamente ao carregar qualquer cena ──────────────────
    void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!_visitedScenes.Contains(scene.name))
        {
            _visitedScenes.Add(scene.name);
            OnSceneVisited?.Invoke(scene.name);
            Debug.Log($"[GameManager] Nova cena visitada: {scene.name}");
        }
    }

    // ── API pública ────────────────────────────────────────────────────────

    /// <summary>Registra total de exhibits de uma cena (chamado pelo ExhibitManager).</summary>
    public void RegisterExhibitTotal(string sceneName, int total)
    {
        _exhibitsTotal[sceneName] = total;
        if (!_exhibitsInteracted.ContainsKey(sceneName))
            _exhibitsInteracted[sceneName] = 0;
    }

    /// <summary>Incrementa contador de exhibits interagidos na cena atual.</summary>
    public void RegisterExhibitInteracted(string sceneName)
    {
        if (!_exhibitsInteracted.ContainsKey(sceneName))
            _exhibitsInteracted[sceneName] = 0;

        _exhibitsInteracted[sceneName]++;

        int current = _exhibitsInteracted[sceneName];
        int total   = _exhibitsTotal.ContainsKey(sceneName) ? _exhibitsTotal[sceneName] : 0;

        OnExhibitProgress?.Invoke(sceneName, current, total);
        Debug.Log($"[GameManager] Exhibit {current}/{total} na cena {sceneName}");
    }

    /// <summary>Retorna progresso de uma cena (0.0 a 1.0).</summary>
    public float GetSceneProgress(string sceneName)
    {
        if (!_exhibitsTotal.ContainsKey(sceneName) || _exhibitsTotal[sceneName] == 0)
            return 0f;

        float interacted = _exhibitsInteracted.ContainsKey(sceneName) ? _exhibitsInteracted[sceneName] : 0;
        return interacted / _exhibitsTotal[sceneName];
    }

    public bool HasVisitedScene(string sceneName) => _visitedScenes.Contains(sceneName);

    public int GetTotalScenesVisited() => _visitedScenes.Count;

    /// <summary>Reinicia todo o progresso (usado no menu ou créditos).</summary>
    public void ResetProgress()
    {
        _visitedScenes.Clear();
        _exhibitsInteracted.Clear();
        _exhibitsTotal.Clear();
        Debug.Log("[GameManager] Progresso resetado.");
    }
}