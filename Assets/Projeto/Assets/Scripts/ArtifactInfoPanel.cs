using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Painel de informações do artefato.
/// Abre ao interagir com um InteracleObject.
/// Fecha pressionando [F] (ou o botão, se existir).
/// </summary>
public class ArtifactInfoPanel : MonoBehaviour
{
    public static ArtifactInfoPanel Instance { get; private set; }

    [Header("Painel Principal")]
    public GameObject panel;

    [Header("Textos")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;

    [Header("Fechar com tecla")]
    public KeyCode closeKey = KeyCode.F;

    [Header("Botão Fechar (opcional)")]
    public Button closeButton;

    [Header("Auto-fechar")]
    [Tooltip("0 = não fecha automaticamente")]
    public float autoCloseDuration = 0f;

    [Header("Animação (opcional)")]
    public Animator panelAnimator;
    public string openTrigger  = "Open";
    public string closeTrigger = "Close";

    private Coroutine _autoCloseCoroutine;
    private bool      _isOpen = false;

    // ─────────────────────────────────────────────────────────────────────

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

    void Update()
    {
        // Fecha com [F] quando o painel estiver aberto
        if (_isOpen && Input.GetKeyDown(closeKey))
            Hide();
    }

    // ── API pública ───────────────────────────────────────────────────────

    public void Show(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(description))
            return;

        if (titleText       != null) titleText.text       = name;
        if (descriptionText != null) descriptionText.text = description;

        if (panel != null) panel.SetActive(true);
        _isOpen = true;

        if (panelAnimator != null)
            panelAnimator.SetTrigger(openTrigger);

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

        _isOpen = false;

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

    // ─────────────────────────────────────────────────────────────────────

    IEnumerator AutoClose()
    {
        yield return new WaitForSeconds(autoCloseDuration);
        Hide();
    }

    IEnumerator DisableAfterAnimation()
    {
        yield return null;
        if (panelAnimator != null)
            yield return new WaitForSeconds(
                panelAnimator.GetCurrentAnimatorStateInfo(0).length);
        if (panel != null) panel.SetActive(false);
    }
}