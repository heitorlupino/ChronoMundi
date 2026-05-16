using System.Collections;
using UnityEngine;
using UnityEngine.XR.Management;

/// <summary>
/// Detecta se um headset VR está conectado e ativo, depois habilita
/// o rig correto (XR Origin para VR, DesktopPlayer para PC).
///
/// Como usar:
///   1. Coloque este script em um GameObject vazio chamado "VRModeManager" na cena.
///   2. Arraste o XR Origin e o DesktopPlayer para os campos do Inspector.
///   3. O script decide automaticamente qual ativar ao iniciar.
///
/// Atalho de teste (Editor):
///   Defina forceDesktopMode = true para testar sem headset.
/// </summary>
public class VRModeManager : MonoBehaviour
{
    [Header("Players")]
    [Tooltip("O XR Origin (XR Rig) com Ray Interactors para VR.")]
    public GameObject xrOrigin;

    [Tooltip("O DesktopPlayer (CharacterController + DesktopPlayerController) para PC.")]
    public GameObject desktopPlayer;

    [Header("Configurações")]
    [Tooltip("Força o modo Desktop mesmo com headset conectado. Útil para testes no Editor.")]
    public bool forceDesktopMode = false;

    [Tooltip("Segundos para aguardar a inicialização do XR antes de decidir o modo.")]
    public float xrDetectionTimeout = 2f;

    // ── Estado ────────────────────────────────────────────────────────────
    public static bool IsVRMode { get; private set; } = false;

    // ─────────────────────────────────────────────────────────────────────
    void Start()
    {
        // Começa com tudo desativado até saber o modo
        SetActive(xrOrigin,   false);
        SetActive(desktopPlayer, false);

        StartCoroutine(DetectAndSetMode());
    }

    IEnumerator DetectAndSetMode()
    {
        if (forceDesktopMode)
        {
            ActivateDesktop();
            yield break;
        }

        // Aguarda o XR Manager inicializar
        float elapsed = 0f;
        while (XRGeneralSettings.Instance == null && elapsed < xrDetectionTimeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        bool vrActive = IsXRActive();
        Debug.Log($"[VRModeManager] VR detectado: {vrActive}");

        if (vrActive)
            ActivateVR();
        else
            ActivateDesktop();
    }

    bool IsXRActive()
    {
        var xrSettings = XRGeneralSettings.Instance;
        if (xrSettings == null) return false;

        var manager = xrSettings.Manager;
        if (manager == null) return false;

        return manager.isInitializationComplete && manager.activeLoader != null;
    }

    void ActivateVR()
    {
        IsVRMode = true;
        SetActive(xrOrigin,   true);
        SetActive(desktopPlayer, false);

        // Avisa PlayerInteraction (no XR Origin) que está em modo VR
        if (xrOrigin != null)
        {
            var pi = xrOrigin.GetComponentInChildren<PlayerInteraction>();
            if (pi != null) pi.isVRMode = true;
        }

        Debug.Log("[VRModeManager] Modo VR ativado.");
    }

    void ActivateDesktop()
    {
        IsVRMode = false;
        SetActive(xrOrigin,   false);
        SetActive(desktopPlayer, true);

        if (desktopPlayer != null)
        {
            var pi = desktopPlayer.GetComponentInChildren<PlayerInteraction>();
            if (pi != null) pi.isVRMode = false;
        }

        Debug.Log("[VRModeManager] Modo Desktop ativado.");
    }

    static void SetActive(GameObject go, bool active)
    {
        if (go != null) go.SetActive(active);
    }
}
