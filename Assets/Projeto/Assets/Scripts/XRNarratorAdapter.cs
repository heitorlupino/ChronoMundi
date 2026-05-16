using UnityEngine;
using TMPro;
using UnityEngine.XR.Management;

/// <summary>
/// Adapta o painel de legendas do NarratorSystem para funcionar em VR.
///
/// Em VR, o canvas de legendas deve ser World Space, posicionado na
/// frente do jogador (não Screen Space Overlay, que não aparece no headset).
///
/// Como usar:
///   Adicione este script no mesmo GameObject do NarratorSystem.
///   Ele detecta automaticamente se está em VR e converte o canvas.
///
/// O script também faz o painel "olhar" para o jogador (billboard).
/// </summary>
[RequireComponent(typeof(NarratorSystem))]
public class XRNarratorAdapter : MonoBehaviour
{
    [Header("Configuração WorldSpace (VR)")]
    [Tooltip("Distância à frente do jogador onde o painel de legenda aparece.")]
    public float distanceFromPlayer = 2.5f;

    [Tooltip("Altura abaixo do centro da visão (negativo = mais baixo).")]
    public float heightOffset = -0.6f;

    [Tooltip("Tamanho do canvas WorldSpace (escala).")]
    public float canvasScale = 0.002f;

    // ── Internos ──────────────────────────────────────────────────────────
    private NarratorSystem _narrator;
    private Canvas         _subtitleCanvas;
    private Transform      _playerCamera;
    private bool           _isVRMode = false;

    // ─────────────────────────────────────────────────────────────────────
    void Start()
    {
        _narrator = GetComponent<NarratorSystem>();
        _isVRMode = IsXRActive();

        if (!_isVRMode)
        {
            enabled = false;
            return;
        }

        Debug.Log("[XRNarratorAdapter] Modo VR detectado. Adaptando canvas de legendas.");
        ConvertToWorldSpace();
        FindPlayerCamera();
    }

    void LateUpdate()
    {
        if (!_isVRMode || _subtitleCanvas == null) return;
        if (_narrator.subtitlePanel == null || !_narrator.subtitlePanel.activeSelf) return;

        if (_playerCamera == null) FindPlayerCamera();
        if (_playerCamera == null) return;

        // Posiciona o painel na frente do jogador
        Vector3 camForward = _playerCamera.forward;
        camForward.y = 0;
        camForward.Normalize();

        Vector3 targetPos = _playerCamera.position
            + _playerCamera.forward * distanceFromPlayer
            + Vector3.up * heightOffset;

        _subtitleCanvas.transform.position = Vector3.Lerp(
            _subtitleCanvas.transform.position, targetPos,
            Time.deltaTime * 8f);

        // Billboard: olha para o jogador
        _subtitleCanvas.transform.LookAt(
            _subtitleCanvas.transform.position + _playerCamera.forward);
    }

    // ─────────────────────────────────────────────────────────────────────

    void ConvertToWorldSpace()
    {
        if (_narrator.subtitlePanel == null) return;

        // Sobe até o Canvas pai
        _subtitleCanvas = _narrator.subtitlePanel.GetComponentInParent<Canvas>();
        if (_subtitleCanvas == null)
        {
            // Cria canvas WorldSpace se não existir
            var canvasGO = new GameObject("NarratorSubtitleCanvas_VR");
            canvasGO.transform.SetParent(transform, false);
            _subtitleCanvas = canvasGO.AddComponent<Canvas>();

            // Move o painel para o novo canvas
            _narrator.subtitlePanel.transform.SetParent(canvasGO.transform, false);
        }

        // Converte para World Space
        _subtitleCanvas.renderMode = UnityEngine.RenderMode.WorldSpace;
        _subtitleCanvas.transform.localScale = Vector3.one * canvasScale;

        // Ajusta tamanho do RectTransform do canvas
        var rt = _subtitleCanvas.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.sizeDelta = new Vector2(1200, 200);
        }

        // Aumenta tamanho da fonte para WorldSpace
        var texts = _narrator.subtitlePanel.GetComponentsInChildren<TextMeshProUGUI>();
        foreach (var t in texts)
        {
            if (t.fontSize < 40) t.fontSize = 48;
        }

        Debug.Log("[XRNarratorAdapter] Canvas de legendas convertido para WorldSpace.");
    }

    void FindPlayerCamera()
    {
        // Tenta pegar via VRModeManager / XR Origin
        if (VRModeManager.IsVRMode)
        {
            var cam = Camera.main;
            if (cam != null) { _playerCamera = cam.transform; return; }
        }

        // Fallback: qualquer câmera
        var fallback = FindFirstObjectByType<Camera>();
        if (fallback != null) _playerCamera = fallback.transform;
    }

    static bool IsXRActive()
    {
        if (VRModeManager.IsVRMode) return true;

        var settings = XRGeneralSettings.Instance;
        if (settings == null) return false;
        return settings.Manager?.activeLoader != null;
    }
}
