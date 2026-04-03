using UnityEngine;

/// <summary>
/// Efeito de destaque pulsante para objetos interagíveis.
/// Anima a intensidade da emissão do material para chamar atenção do jogador.
/// Desaparece após o objeto ser explorado.
///
/// Como usar:
///   1. Adicione este script ao mesmo GameObject do InteracleObject.
///   2. O material do objeto deve ter a propriedade "_EmissionColor" (URP Lit / Standard).
///   3. Configure a cor de emissão base no Inspector.
///
/// Alternativa: use com um segundo objeto filho como "glow ring" no chão.
/// </summary>
[RequireComponent(typeof(Renderer))]
public class HighlightController : MonoBehaviour
{
    [Header("Pulsação")]
    public Color emissionColor = new Color(0.2f, 0.6f, 1f);
    public float pulseSpeed = 2f;
    [Range(0f, 2f)]
    public float minIntensity = 0.1f;
    [Range(0f, 4f)]
    public float maxIntensity = 1.2f;

    [Header("Comportamento")]
    [Tooltip("Para a pulsação quando o objeto for interagido.")]
    public bool stopOnInteract = true;

    // ── Estado interno ─────────────────────────────────────────────────────
    private Renderer _renderer;
    private Material _material;
    private bool _active = true;
    private InteracleObject _exhibit;

    static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");

    // ──────────────────────────────────────────────────────────────────────
    void Awake()
    {
        _renderer = GetComponent<Renderer>();

        // Cria instância do material para não afetar o asset
        _material = _renderer.material;
        _material.EnableKeyword("_EMISSION");
    }

    void Start()
    {
        if (stopOnInteract)
        {
            _exhibit = GetComponent<InteracleObject>();
            if (_exhibit != null)
                _exhibit.OnInteracted += OnExhibitInteracted;
        }
    }

    void OnDestroy()
    {
        if (_exhibit != null)
            _exhibit.OnInteracted -= OnExhibitInteracted;
    }

    void Update()
    {
        if (!_active || _material == null) return;

        float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f; // 0 a 1
        float intensity = Mathf.Lerp(minIntensity, maxIntensity, t);

        _material.SetColor(EmissionColorID, emissionColor * intensity);
    }

    // ──────────────────────────────────────────────────────────────────────
    void OnExhibitInteracted(InteracleObject _)
    {
        StopHighlight();
    }

    /// <summary>Para a pulsação e apaga a emissão.</summary>
    public void StopHighlight()
    {
        _active = false;
        if (_material != null)
            _material.SetColor(EmissionColorID, Color.black);
    }

    /// <summary>Reinicia o efeito.</summary>
    public void StartHighlight()
    {
        _active = true;
    }
}
