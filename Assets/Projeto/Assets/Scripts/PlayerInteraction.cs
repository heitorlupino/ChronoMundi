using UnityEngine;

/// <summary>
/// Gerencia a interação do jogador com objetos IInteractable.
/// Funciona tanto em VR (ray do controller) quanto em Desktop (ray da câmera).
/// </summary>
public class PlayerInteraction : MonoBehaviour
{
    [Header("Configurações de Interação")]
    public float interactionDistance = 3f;
    public LayerMask interactableLayer;

    [Header("Modo de Controle")]
    public bool isVRMode = true; // true = ray sai deste transform (controller), false = ray da câmera

    [Header("Referências (Desktop)")]
    [SerializeField] private Camera playerCamera;

    [Header("Input")]
    public KeyCode interactionKey = KeyCode.E; // Usado no modo Desktop

    [Header("Feedback Visual")]
    public GameObject interactionPrompt;        // Ex: ícone "pressione E" ou "aponte o controller"
    public TMPro.TextMeshProUGUI promptText;

    // ── Estado interno ─────────────────────────────────────────────────────
    private IInteractable _currentTarget;

    // ──────────────────────────────────────────────────────────────────────
    void Start()
    {
        if (!isVRMode && playerCamera == null)
            playerCamera = Camera.main;

        SetPromptVisible(false);
    }

    void Update()
    {
        DetectInteractable();
        HandleInput();
    }

    // ── Detecta o objeto interagível na mira ──────────────────────────────
    void DetectInteractable()
    {
        Ray ray = isVRMode
            ? new Ray(transform.position, transform.forward)                        // VR: ray do controller
            : new Ray(playerCamera.transform.position, playerCamera.transform.forward); // Desktop: ray da câmera

        if (interactableLayer == 0)
            Debug.LogWarning("[PlayerInteraction] LayerMask não configurado no Inspector!");

        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactableLayer))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            if (interactable != null)
            {
                // Entrou em um novo alvo
                if (interactable != _currentTarget)
                {
                    _currentTarget = interactable;
                    UpdatePrompt(hit.collider.gameObject.name);
                    SetPromptVisible(true);
                }
                return;
            }
        }

        // Nenhum alvo válido
        if (_currentTarget != null)
        {
            _currentTarget = null;
            SetPromptVisible(false);
        }
    }

    // ── Lida com o input de interação ─────────────────────────────────────
    void HandleInput()
    {
        if (_currentTarget == null) return;

        bool triggered = isVRMode
            ? Input.GetButtonDown("Fire1")        // Trigger do controller (adapte ao seu binding)
            : Input.GetKeyDown(interactionKey);   // Teclado no Desktop

        if (triggered)
            _currentTarget.Interact();
    }

    // ── UI do prompt ──────────────────────────────────────────────────────
    void UpdatePrompt(string objectName)
    {
        if (promptText == null) return;

        promptText.text = isVRMode
            ? $"Aponte para interagir\n<b>{objectName}</b>"
            : $"Pressione <b>[{interactionKey}]</b> para interagir\n<b>{objectName}</b>";
    }

    void SetPromptVisible(bool visible)
    {
        if (interactionPrompt != null)
            interactionPrompt.SetActive(visible);
    }

    // ── Gizmo de debug no Editor ──────────────────────────────────────────
    void OnDrawGizmosSelected()
    {
        Transform origin = isVRMode ? transform : (playerCamera != null ? playerCamera.transform : transform);

        Gizmos.color = _currentTarget != null ? Color.green : Color.yellow;
        Gizmos.DrawRay(origin.position, origin.forward * interactionDistance);
    }
}