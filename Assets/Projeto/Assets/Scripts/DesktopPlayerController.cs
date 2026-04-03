using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Controlador de jogador para Desktop (sem VR).
/// Movimentação WASD + mouse look.
/// Útil para desenvolvimento e testes sem headset VR.
///
/// Como usar:
///   1. Crie um GameObject "DesktopPlayer" com:
///      - CharacterController (ou Rigidbody)
///      - Este script
///      - Camera filho (olhos do jogador)
///      - PlayerInteraction script (no mesmo GO ou na câmera)
///   2. Desative este GameObject quando rodar em modo VR.
///
/// Atalhos:
///   WASD / setas   — mover
///   Mouse          — olhar (capturado; Esc para soltar)
///   E              — interagir (via PlayerInteraction)
///   Esc            — liberar/capturar cursor
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class DesktopPlayerController : MonoBehaviour
{
    [Header("Movimento")]
    public float moveSpeed = 4f;
    public float sprintSpeed = 7f;
    public float gravity = -9.81f;

    [Header("Câmera / Mouse Look")]
    public Transform cameraTransform;
    public float mouseSensitivity = 2f;
    public float maxVerticalAngle = 80f;

    [Header("Modo")]
    [Tooltip("Desative em builds VR — este controlador é apenas para Desktop.")]
    public bool enabledOnStart = true;

    // ── Estado interno ─────────────────────────────────────────────────────
    private CharacterController _cc;
    private float _verticalAngle = 0f;
    private Vector3 _velocity = Vector3.zero;
    private bool _cursorLocked = true;

    // ──────────────────────────────────────────────────────────────────────
    void Awake()
    {
        _cc = GetComponent<CharacterController>();

        if (cameraTransform == null)
            cameraTransform = GetComponentInChildren<Camera>()?.transform;
    }

    void Start()
    {
        if (!enabledOnStart)
        {
            enabled = false;
            return;
        }

        LockCursor(true);
    }

    void Update()
    {
        HandleCursorToggle();
        if (_cursorLocked)
        {
            HandleMouseLook();
            HandleMovement();
        }
    }

    // ── Mouse look ─────────────────────────────────────────────────────────
    void HandleMouseLook()
    {
        float mouseX, mouseY;

#if ENABLE_INPUT_SYSTEM
        Vector2 delta = Mouse.current?.delta.ReadValue() ?? Vector2.zero;
        mouseX = delta.x * mouseSensitivity * 0.1f;
        mouseY = delta.y * mouseSensitivity * 0.1f;
#else
        mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
#endif

        // Rotação horizontal do corpo
        transform.Rotate(Vector3.up * mouseX);

        // Rotação vertical da câmera (clamp)
        _verticalAngle -= mouseY;
        _verticalAngle = Mathf.Clamp(_verticalAngle, -maxVerticalAngle, maxVerticalAngle);

        if (cameraTransform != null)
            cameraTransform.localRotation = Quaternion.Euler(_verticalAngle, 0f, 0f);
    }

    // ── Movimento ──────────────────────────────────────────────────────────
    void HandleMovement()
    {
        float h, v;
        bool sprint;

#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        h = (kb != null ? (kb.dKey.isPressed ? 1f : 0f) - (kb.aKey.isPressed ? 1f : 0f) : 0f);
        v = (kb != null ? (kb.wKey.isPressed ? 1f : 0f) - (kb.sKey.isPressed ? 1f : 0f) : 0f);
        sprint = kb != null && kb.leftShiftKey.isPressed;
#else
        h      = Input.GetAxis("Horizontal");
        v      = Input.GetAxis("Vertical");
        sprint = Input.GetKey(KeyCode.LeftShift);
#endif

        float speed = sprint ? sprintSpeed : moveSpeed;
        Vector3 move = transform.right * h + transform.forward * v;

        if (_cc.isGrounded && _velocity.y < 0f)
            _velocity.y = -2f;

        _velocity.y += gravity * Time.deltaTime;
        _cc.Move((move * speed + _velocity) * Time.deltaTime);
    }

    // ── Cursor ────────────────────────────────────────────────────────────
    void HandleCursorToggle()
    {
        bool escPressed;
#if ENABLE_INPUT_SYSTEM
        escPressed = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
#else
        escPressed = Input.GetKeyDown(KeyCode.Escape);
#endif
        if (escPressed)
            LockCursor(!_cursorLocked);
    }

    void LockCursor(bool locked)
    {
        _cursorLocked = locked;
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}
