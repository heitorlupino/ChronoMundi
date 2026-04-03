using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
#endif

/// <summary>
/// Gerencia a interação do jogador com IInteractable.
/// Compatível com VR (ray do controller) e Desktop (ray da câmera).
/// Suporta tanto o Input System legado quanto o novo.
/// </summary>
public class PlayerInteraction : MonoBehaviour
{
    [Header("Configurações de Interação")]
    public float interactionDistance = 3f;

    [Tooltip("Defina a layer 'Interactable' aqui. Objetos sem essa layer serão ignorados.")]
    public LayerMask interactableLayer = ~0; // default: All layers (ajuste no Inspector)

    [Header("Modo de Controle")]
    public bool isVRMode = false; // false = Desktop; true = VR controller

    [Header("Referências (Desktop)")]
    [SerializeField] private Camera playerCamera;

    [Header("Input (Desktop)")]
    public KeyCode interactionKey = KeyCode.E;

    [Header("Feedback Visual")]
    public GameObject interactionPrompt;
    public TMPro.TextMeshProUGUI promptText;

    // ── Estado interno ─────────────────────────────────────────────────────
    private IInteractable _currentTarget;
    private bool _layerWarningShown = false;

    // ──────────────────────────────────────────────────────────────────────
    void Start()
    {
        if (!isVRMode && playerCamera == null)
            playerCamera = Camera.main;

        // Aviso único no Start, não todo frame
        if (interactableLayer.value == 0)
        {
            Debug.LogWarning("[PlayerInteraction] LayerMask 'interactableLayer' está em Nothing. " +
                             "Nenhum objeto será detectado. Configure no Inspector.");
            _layerWarningShown = true;
        }

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
        if (interactableLayer.value == 0) return;

        Ray ray = isVRMode
            ? new Ray(transform.position, transform.forward)
            : new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactableLayer))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>()
                                      ?? hit.collider.GetComponentInParent<IInteractable>();

            if (interactable != null)
            {
                if (interactable != _currentTarget)
                {
                    _currentTarget = interactable;
                    UpdatePrompt(hit.collider.gameObject.name);
                    SetPromptVisible(true);
                }
                return;
            }
        }

        if (_currentTarget != null)
        {
            _currentTarget = null;
            SetPromptVisible(false);
        }
    }

    // ── Input — suporta legado e novo Input System ─────────────────────────
    void HandleInput()
    {
        if (_currentTarget == null) return;

        bool triggered = false;

        if (isVRMode)
        {
#if ENABLE_INPUT_SYSTEM
            // Novo Input System — trigger do controller
            var triggerAction = InputSystem.actions?.FindAction("XRI RightHand/Select");
            triggered = triggerAction != null && triggerAction.WasPressedThisFrame();
            if (!triggered) triggered = Input.GetButtonDown("Fire1"); // fallback
#else
            triggered = Input.GetButtonDown("Fire1");
#endif
        }
        else
        {
#if ENABLE_INPUT_SYSTEM
            triggered = Keyboard.current != null && Keyboard.current[Key.E].wasPressedThisFrame;
#else
            triggered = Input.GetKeyDown(interactionKey);
#endif
        }

        if (triggered)
            _currentTarget.Interact();
    }

    // ── UI do prompt ──────────────────────────────────────────────────────
    void UpdatePrompt(string objectName)
    {
        if (promptText == null) return;
        promptText.text = isVRMode
            ? $"Aponte e pressione o trigger\n<b>{objectName}</b>"
            : $"Pressione <b>[{interactionKey}]</b> para interagir\n<b>{objectName}</b>";
    }

    void SetPromptVisible(bool visible)
    {
        if (interactionPrompt != null)
            interactionPrompt.SetActive(visible);
    }

    void OnDrawGizmosSelected()
    {
        Transform origin = (!isVRMode && playerCamera != null) ? playerCamera.transform : transform;
        Gizmos.color = _currentTarget != null ? Color.green : Color.yellow;
        Gizmos.DrawRay(origin.position, origin.forward * interactionDistance);
    }
}
