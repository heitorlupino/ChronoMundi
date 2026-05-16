using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Ponte entre o XR Interaction Toolkit e o sistema de artefatos do ChronoMundi.
///
/// Coloque este script junto com InteracleObject em cada artefato.
/// Ele exige um XRSimpleInteractable no mesmo GameObject (adicionado automaticamente).
///
/// O que faz:
///   - Ouve o evento "Select" do XRI (trigger/aperto de mão do controller)
///   - Chama InteracleObject.Interact() para tocar narração e registrar progresso
///   - Feedback de hover: escala suave ao apontar o ray para o objeto
///   - Compatível com Ray Interactor (controller) e Poke Interactor (mão direta)
/// </summary>
[RequireComponent(typeof(XRSimpleInteractable))]
[RequireComponent(typeof(InteracleObject))]
public class XRIExhibitAdapter : MonoBehaviour
{
    [Header("Feedback de Hover (VR)")]
    [Tooltip("Escala o objeto levemente quando o ray aponta para ele.")]
    public bool enableHoverScale = true;
    public float hoverScaleMultiplier = 1.08f;
    public float hoverScaleSpeed = 6f;

    // ── Internos ──────────────────────────────────────────────────────────
    private XRSimpleInteractable _interactable;
    private InteracleObject      _exhibit;
    private Vector3              _originalScale;
    private bool                 _isHovered = false;

    // ─────────────────────────────────────────────────────────────────────
    void Awake()
    {
        _interactable = GetComponent<XRSimpleInteractable>();
        _exhibit      = GetComponent<InteracleObject>();
        _originalScale = transform.localScale;
    }

    void OnEnable()
    {
        _interactable.selectEntered.AddListener(OnSelectEntered);
        _interactable.hoverEntered.AddListener(OnHoverEntered);
        _interactable.hoverExited.AddListener(OnHoverExited);
    }

    void OnDisable()
    {
        _interactable.selectEntered.RemoveListener(OnSelectEntered);
        _interactable.hoverEntered.RemoveListener(OnHoverEntered);
        _interactable.hoverExited.RemoveListener(OnHoverExited);
    }

    void Update()
    {
        if (!enableHoverScale) return;

        Vector3 target = _isHovered
            ? _originalScale * hoverScaleMultiplier
            : _originalScale;

        transform.localScale = Vector3.Lerp(
            transform.localScale, target,
            Time.deltaTime * hoverScaleSpeed);
    }

    // ── Callbacks XRI ─────────────────────────────────────────────────────

    void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (_exhibit != null)
            _exhibit.Interact();
    }

    void OnHoverEntered(HoverEnterEventArgs args)
    {
        _isHovered = true;
    }

    void OnHoverExited(HoverExitEventArgs args)
    {
        _isHovered = false;
    }
}
