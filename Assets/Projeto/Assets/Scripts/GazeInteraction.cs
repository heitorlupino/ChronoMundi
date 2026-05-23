using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Interação por olhar (gaze) para Google Cardboard.
///
/// Como funciona:
///   - Raycast parte do centro da câmera principal
///   - Ao mirar num artefato por 'gazeTime' segundos, dispara Interact()
///   - Um anel de progresso circular (Image fillAmount) mostra o progresso
///   - Toque na tela / botão Cardboard também dispara imediatamente
///
/// Coloque no mesmo GameObject que a Main Camera do jogador.
///
/// Compatível com Desktop: no Editor, o clique do mouse dispara o gaze.
/// </summary>
public class GazeInteraction : MonoBehaviour
{
    [Header("Configuração")]
    [Tooltip("Segundos olhando para o objeto antes de interagir.")]
    public float gazeTime = 2f;

    [Tooltip("Distância máxima do raycast.")]
    public float gazeDistance = 5f;

    [Tooltip("Layer dos objetos interagíveis.")]
    public LayerMask interactableLayer = ~0;

    [Header("UI — Reticle (mira)")]
    [Tooltip("Objeto raiz da mira (ativa/desativa conforme hover).")]
    public GameObject reticleRoot;

    [Tooltip("Image com fillAmount para mostrar o progresso do gaze.")]
    public Image reticleProgress;

    [Tooltip("Cor da mira em repouso.")]
    public Color reticleIdleColor = new Color(1f, 1f, 1f, 0.6f);

    [Tooltip("Cor da mira ao mirar em algo interagível.")]
    public Color reticleActiveColor = new Color(0.3f, 1f, 0.5f, 1f);

    // ── Internos ──────────────────────────────────────────────────────────
    Camera       _cam;
    float        _gazeTimer   = 0f;
    IInteractable _currentTarget;
    GameObject   _currentGO;
    bool         _interacted  = false;

    // ─────────────────────────────────────────────────────────────────────
    void Start()
    {
        _cam = GetComponent<Camera>() ?? Camera.main;
        ResetReticle();
    }

    void Update()
    {
        // Toque ou botão Cardboard = dispara imediatamente
        bool tapped = Input.GetMouseButtonDown(0)
                   || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began);

        Ray ray = new Ray(_cam.transform.position, _cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, gazeDistance, interactableLayer))
        {
            var interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                // Mudou de alvo?
                if (hit.collider.gameObject != _currentGO)
                {
                    ResetGaze();
                    _currentGO     = hit.collider.gameObject;
                    _currentTarget = interactable;
                    _interacted    = false;
                    SetReticleActive(true);
                }

                if (!_interacted)
                {
                    // Toque imediato
                    if (tapped)
                    {
                        TriggerInteract();
                        return;
                    }

                    // Progresso por tempo
                    _gazeTimer += Time.deltaTime;
                    UpdateReticle(_gazeTimer / gazeTime);

                    if (_gazeTimer >= gazeTime)
                        TriggerInteract();
                }
                return;
            }
        }

        // Sem alvo
        if (_currentTarget != null) ResetGaze();
    }

    // ─────────────────────────────────────────────────────────────────────

    void TriggerInteract()
    {
        if (_interacted || _currentTarget == null) return;
        _interacted = true;

        _currentTarget.Interact();

        // Animação de conclusão: pisca o reticle
        StartCoroutine(FlashReticle());
    }

    void ResetGaze()
    {
        _gazeTimer    = 0f;
        _currentTarget = null;
        _currentGO    = null;
        _interacted   = false;
        ResetReticle();
    }

    // ── Reticle ───────────────────────────────────────────────────────────

    void ResetReticle()
    {
        if (reticleProgress != null)
        {
            reticleProgress.fillAmount = 0f;
            reticleProgress.color      = reticleIdleColor;
        }
        SetReticleActive(false);
    }

    void SetReticleActive(bool active)
    {
        if (reticleProgress != null)
            reticleProgress.color = active ? reticleActiveColor : reticleIdleColor;
    }

    void UpdateReticle(float t)
    {
        if (reticleProgress != null)
            reticleProgress.fillAmount = Mathf.Clamp01(t);
    }

    IEnumerator FlashReticle()
    {
        if (reticleProgress == null) yield break;
        reticleProgress.fillAmount = 1f;
        reticleProgress.color      = Color.white;
        yield return new WaitForSeconds(0.18f);
        ResetReticle();
    }

    // ── Gizmo no Editor ───────────────────────────────────────────────────
    void OnDrawGizmosSelected()
    {
        if (_cam == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawRay(_cam.transform.position,
                       _cam.transform.forward * gazeDistance);
    }
}
