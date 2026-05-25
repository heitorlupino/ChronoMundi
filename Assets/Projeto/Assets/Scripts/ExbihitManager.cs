using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Gerencia todos os exhibits (artefatos interativos) de uma cena histórica.
/// Controla highlight, progresso e notifica TimelineEra ao completar tudo.
/// Coloque UM deste por cena histórica.
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
    public Material highlightMaterial;
    public Color interactedColor = new Color(0.4f, 1f, 0.4f);

    [Header("Integração")]
    [Tooltip("Referência opcional ao TimelineEra para disparar evento de conclusão.")]
    public TimelineEra eraController;

    // ── Estado interno ─────────────────────────────────────────────────────
    private HashSet<InteracleObject> _interacted = new HashSet<InteracleObject>();
    private Dictionary<InteracleObject, Material[]> _originalMaterials = new Dictionary<InteracleObject, Material[]>();

    // ──────────────────────────────────────────────────────────────────────
    void Start()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (GameManager.Instance != null)
            GameManager.Instance.RegisterExhibitTotal(sceneName, exhibits.Count);

        foreach (var exhibit in exhibits)
        {
            if (exhibit == null) continue;

            Renderer rend = exhibit.GetComponent<Renderer>();
            if (rend != null)
            {
                _originalMaterials[exhibit] = rend.materials;

                if (highlightMaterial != null)
                {
                    var mats = new Material[rend.materials.Length + 1];
                    rend.materials.CopyTo(mats, 0);
                    mats[mats.Length - 1] = highlightMaterial;
                    rend.materials = mats;
                }
            }

            // Usa += em event (não em campo público) — seguro após correção do InteracleObject
            exhibit.OnInteracted += HandleExhibitInteracted;
        }

        UpdateProgressUI();
    }

    void OnDestroy()
    {
        foreach (var exhibit in exhibits)
            if (exhibit != null)
                exhibit.OnInteracted -= HandleExhibitInteracted;
    }

    // ──────────────────────────────────────────────────────────────────────
    void HandleExhibitInteracted(InteracleObject exhibit)
    {
        if (_interacted.Contains(exhibit)) return;

        _interacted.Add(exhibit);

        // Troca visual de "já visitado"
        Renderer rend = exhibit.GetComponent<Renderer>();
        if (rend != null && _originalMaterials.ContainsKey(exhibit))
        {
            rend.materials = _originalMaterials[exhibit];
            if (rend.material != null)
                rend.material.color = interactedColor;
        }

        string sceneName = SceneManager.GetActiveScene().name;
        if (GameManager.Instance != null)
            GameManager.Instance.RegisterExhibitInteracted(sceneName);

        UpdateProgressUI();
        CheckCompletion();
    }

    void UpdateProgressUI()
    {
        int current = _interacted.Count;
        int total = exhibits.Count;

        if (progressText != null)
            progressText.text = $"{current} / {total} artefatos explorados";

        if (progressBar != null)
        {
            float newValue = total > 0 ? (float)current / total : 0f;
            progressBar.value = newValue;
            Debug.Log($"[ExhibitManager] Progresso: {current}/{total} = {newValue:P0} | Slider.value={progressBar.value}");
        }
        else
        {
            Debug.LogWarning("[ExhibitManager] progressBar é NULL!");
        }
    }

    void CheckCompletion()
    {
        if (_interacted.Count < exhibits.Count) return;

        Debug.Log("[ExhibitManager] Todos os artefatos explorados nesta cena!");

        // Notifica a era para tocar feedback de conclusão
        if (eraController != null)
            eraController.OnAllExhibitsComplete();
    }

    public float GetProgress() =>
        exhibits.Count > 0 ? (float)_interacted.Count / exhibits.Count : 0f;
}
