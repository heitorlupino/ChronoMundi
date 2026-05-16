using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Retorna o jogador ao MuseuHub.
/// Pode ser chamado via botão UI, trigger de colisão ou objeto interagível.
/// </summary>
public class SimpleReturnToMuseum : MonoBehaviour, IInteractable
{
    [Header("Cena do Museu")]
    public string museumSceneName = "MuseuHubScene";

    [Header("Modo de Ativação")]
    public ActivationMode activationMode = ActivationMode.Interactable;

    public enum ActivationMode
    {
        Interactable, // Jogador interage diretamente com o objeto (IInteractable)
        UIButton,     // Ativado por um botão na UI
        OnTrigger     // Ativado ao entrar na área (collider trigger)
    }

    [Header("UI (apenas no modo UIButton)")]
    public Button returnButton;

    [Header("Confirmação (opcional)")]
    public bool askConfirmation = false;
    public GameObject confirmationPanel;  // Painel com "Tem certeza? Sim / Não"
    public Button btnConfirm;
    public Button btnCancel;

    [Header("Narração ao sair (opcional)")]
    public AudioClip exitNarrationClip;
    [TextArea]
    public string exitSubtitleText = "Retornando ao Museu...";

    // ──────────────────────────────────────────────────────────────────────
    void Start()
    {
        // Esconde painel de confirmação
        if (confirmationPanel != null) confirmationPanel.SetActive(false);

        // Liga botão principal
        if (activationMode == ActivationMode.UIButton && returnButton != null)
            returnButton.onClick.AddListener(TryReturn);

        // Liga botões de confirmação
        if (btnConfirm != null) btnConfirm.onClick.AddListener(ConfirmReturn);
        if (btnCancel  != null) btnCancel.onClick.AddListener(CancelReturn);
    }

    // Previne reentradas ao tentar voltar
    bool _isProcessing = false;

    void OnDestroy()
    {
        if (returnButton != null) returnButton.onClick.RemoveListener(TryReturn);
        if (btnConfirm   != null) btnConfirm.onClick.RemoveListener(ConfirmReturn);
        if (btnCancel    != null) btnCancel.onClick.RemoveListener(CancelReturn);
    }

    // ── Ativação por trigger de colisão ───────────────────────────────────
    void OnTriggerEnter(Collider other)
    {
        if (activationMode != ActivationMode.OnTrigger) return;
        if (IsPlayerCollider(other))
            TryReturn();
    }

    // ── Ativação por IInteractable ────────────────────────────────────────
    public void Interact()
    {
        if (activationMode != ActivationMode.Interactable) return;
        if (_isProcessing || LoadingScreen.IsLoading) return;
        TryReturn();
    }

    // ── Lógica de retorno ─────────────────────────────────────────────────
    public void TryReturn()
    {
        if (askConfirmation && confirmationPanel != null)
        {
            confirmationPanel.SetActive(true); // Exibe "Tem certeza?"
            return;
        }

        ExecuteReturn();
    }

    void ConfirmReturn()
    {
        if (confirmationPanel != null) confirmationPanel.SetActive(false);
        ExecuteReturn();
    }

    void CancelReturn()
    {
        if (confirmationPanel != null) confirmationPanel.SetActive(false);
        Debug.Log("[SimpleReturnToMuseum] Retorno cancelado pelo jogador.");
    }

    void ExecuteReturn()
    {
        if (_isProcessing || LoadingScreen.IsLoading) return;
        _isProcessing = true;
        StartCoroutine(ResetProcessing());

        Debug.Log("[SimpleReturnToMuseum] Retornando ao museu...");

        // Toca narração de saída se houver
        if (NarratorSystem.Instance != null && exitNarrationClip != null)
            NarratorSystem.Instance.PlayNarration(exitNarrationClip, exitSubtitleText);

        LoadingScreen.LoadScene(museumSceneName); // ✅ Usa loading assíncrono
    }

    IEnumerator ResetProcessing()
    {
        yield return new WaitForSecondsRealtime(1f);
        _isProcessing = false;
    }

    bool IsPlayerCollider(Collider other)
    {
        if (other == null) return false;
        if (other.CompareTag("Player")) return true;
        if (other.GetComponentInParent<CharacterController>() != null) return true;
        if (other.GetComponentInParent<PlayerInteraction>() != null) return true;
        if (other.GetComponentInParent<DesktopPlayerController>() != null) return true;
        return false;
    }
}