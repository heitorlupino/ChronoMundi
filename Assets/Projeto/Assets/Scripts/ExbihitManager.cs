using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Gerencia todos os exhibits (objetos interativos) de uma cena.
/// Controla highlight, progresso e feedback visual.
/// Coloque um deste por cena histórica.
/// </summary>
public class ExhibitManager : MonoBehaviour
{
    [Header("Exhibits da Cena")]
    public List<InteracleObject> exhibits = new List<InteracleObject>();

    [Header("UI de Progresso")]
    public GameObject progressPanel;
    public TextMeshProUGUI progressText;   // Ex: "2 / 5 artefatos explorados"
    public UnityEngine.UI.Slider progressBar;

    [Header("Highlight nos objetos")]
    public Material highlightMaterial;     // Material de destaque (outline, brilho etc.)
    public Color    interactedColor = new Color(0.4f, 1f, 0.4f); // verde ao interagir

    // ── Estado interno ─────────────────────────────────────────────────────
    private HashSet<InteracleObject> _interacted = new HashSet<InteracleObject>();
    private Dictionary<InteracleObject, Material[]> _originalMaterials
        = new Dictionary<InteracleObject, Material[]>();

    // ──────────────────────────────────────────────────────────────────────
    void Start()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        // Registra total no GameManager
        if (GameManager.Instance != null)
            GameManager.Instance.RegisterExhibitTotal(sceneName, exhibits.Count);

        // Guarda materiais originais e aplica highlight inicial
        foreach (var exhibit in exhibits)
        {
            if (exhibit == null) continue;

            Renderer rend = exhibit.GetComponent<Renderer>();
            if (rend != null)
            {
                _originalMaterials[exhibit] = rend.materials;

                if (highlightMaterial != null)
                {
                    // Adiciona highlight como material extra
                    var mats = new Material[rend.materials.Length + 1];
                    rend.materials.CopyTo(mats, 0);
                    mats[mats.Length - 1] = highlightMaterial;
                    rend.materials = mats;
                }
            }

            // Escuta quando este exhibit for interagido
            exhibit.OnInteracted += HandleExhibitInteracted;
        }

        UpdateProgressUI();
    }

    void OnDestroy()
    {
        foreach (var exhibit in exhibits)
        {
            if (exhibit != null)
                exhibit.OnInteracted -= HandleExhibitInteracted;
        }
    }

    // ── Chamado pelo InteracleObject ao ser interagido ─────────────────────
    void HandleExhibitInteracted(InteracleObject exhibit)
    {
        if (_interacted.Contains(exhibit)) return;

        _interacted.Add(exhibit);

        // Remove highlight e troca cor para "já visitado"
        Renderer rend = exhibit.GetComponent<Renderer>();
        if (rend != null && _originalMaterials.ContainsKey(exhibit))
        {
            rend.materials = _originalMaterials[exhibit];

            // Tinge o primeiro material com a cor de "visitado"
            if (rend.material != null)
                rend.material.color = interactedColor;
        }

        // Registra no GameManager
        string sceneName = SceneManager.GetActiveScene().name;
        if (GameManager.Instance != null)
            GameManager.Instance.RegisterExhibitInteracted(sceneName);

        UpdateProgressUI();
        CheckCompletion();
    }

    void UpdateProgressUI()
    {
        int current = _interacted.Count;
        int total   = exhibits.Count;

        if (progressText != null)
            progressText.text = $"{current} / {total} artefatos explorados";

        if (progressBar != null)
            progressBar.value = total > 0 ? (float)current / total : 0f;
    }

    void CheckCompletion()
    {
        if (_interacted.Count >= exhibits.Count)
        {
            Debug.Log("[ExhibitManager] Todos os artefatos explorados nesta cena!");
            // Aqui você pode disparar um evento especial, cutscene, etc.
        }
    }

    public float GetProgress() =>
        exhibits.Count > 0 ? (float)_interacted.Count / exhibits.Count : 0f;
}