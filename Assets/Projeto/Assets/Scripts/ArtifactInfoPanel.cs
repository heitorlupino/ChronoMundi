using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Painel de informações do artefato.
/// Exibe nome e descrição quando o jogador interage com um InteracleObject.
/// Persiste entre cenas junto com o NarratorSystem.
///
/// Como usar no Inspector:
///   1. Crie um Canvas (Screen Space – Overlay) com DontDestroyOnLoad.
///   2. Adicione um painel com: título (TMP), descrição (TMP), botão fechar.
///   3. Arraste os campos abaixo no Inspector.
///   4. O painel abre automaticamente ao interagir; fecha pelo botão ou pelo timer.
/// </summary>
public class ArtifactInfoPanel : MonoBehaviour
{
    public static ArtifactInfoPanel Instance { get; private set; }

    [Header("Painel Principal")]
    public GameObject panel;

    [Header("Textos")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;

    [Header("Botão Fechar")]
    public Button closeButton;

    [Header("Auto-fechar")]
    [Tooltip("0 = não fecha automaticamente")]
    public float autoCloseDuration = 8f;

    [Header("Animação (opcional)")]
    public Animator panelAnimator;
    public string openTrigger = "Open";
    public string closeTrigger = "Close";

    // ??????????????????????????????????????????????????????????????????????
    private Coroutine _autoCloseCoroutine;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (panel != null) panel.SetActive(false);
        if (closeButton != null) closeButton.onClick.AddListener(Hide);
    }

    void OnDestroy()
    {
        if (closeButton != null) closeButton.onClick.RemoveListener(Hide);
    }

    // ?? API pública ????????????????????????????????????????????????????????

    /// <summary>Mostra o painel com nome e descrição do artefato.</summary>
    public void Show(string name, string description)
    {
        // Sem conteúdo relevante: não abre o painel
        if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(description))
            return;

        if (titleText != null) titleText.text = name;
        if (descriptionText != null) descriptionText.text = description;

        if (panel != null) panel.SetActive(true);

        if (panelAnimator != null)
            panelAnimator.SetTrigger(openTrigger);

        // Auto-fechar
        if (_autoCloseCoroutine != null) StopCoroutine(_autoCloseCoroutine);
        if (autoCloseDuration > 0f)
            _autoCloseCoroutine = StartCoroutine(AutoClose());
    }

    public void Hide()
    {
        if (_autoCloseCoroutine != null)
        {
            StopCoroutine(_autoCloseCoroutine);
            _autoCloseCoroutine = null;
        }

        if (panelAnimator != null)
        {
            panelAnimator.SetTrigger(closeTrigger);
            StartCoroutine(DisableAfterAnimation());
        }
        else
        {
            if (panel != null) panel.SetActive(false);
        }
    }

    // ??????????????????????????????????????????????????????????????????????
    IEnumerator AutoClose()
    {
        yield return new WaitForSeconds(autoCloseDuration);
        Hide();
    }

    IEnumerator DisableAfterAnimation()
    {
        // Espera a animação de fechar terminar (1 frame + duração da clip)
        yield return null;
        if (panelAnimator != null)
            yield return new WaitForSeconds(
                panelAnimator.GetCurrentAnimatorStateInfo(0).length);

        if (panel != null) panel.SetActive(false);
    }
}
