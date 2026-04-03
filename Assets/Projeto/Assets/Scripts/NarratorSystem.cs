using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Sistema de narração do ChronoMundi.
/// Persiste entre cenas (DontDestroyOnLoad).
///
/// CORREÇÕES nesta versão:
///   - Suporte a narração texto-apenas (clip == null): exibe legenda sem áudio.
///     Necessário para MuseumDoor exibir mensagem de "porta bloqueada".
///   - Singleton usa property { get; private set; } (padrão seguro).
///   - HideSubtitle migrado para Coroutine (evita Invoke com string).
/// </summary>
public class NarratorSystem : MonoBehaviour
{
    public static NarratorSystem Instance { get; private set; }

    [Header("Áudio")]
    public AudioSource audioSource;

    [Header("Legenda")]
    public TextMeshProUGUI subtitleText;
    public GameObject subtitlePanel;

    [Header("Duração da legenda sem áudio (segundos)")]
    public float textOnlyDuration = 3f;

    private Coroutine _hideCoroutine;

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
        }
    }

    /// <summary>
    /// Toca narração com áudio + legenda.
    /// Se clip for null, exibe apenas a legenda por textOnlyDuration segundos
    /// (ex.: mensagem de porta bloqueada no MuseumDoor).
    /// </summary>
    public void PlayNarration(AudioClip clip, string subtitle)
    {
        StopHide();

        float duration;

        if (clip != null)
        {
            if (audioSource == null)
            {
                Debug.LogWarning("[NarratorSystem] AudioSource não atribuído! Exibindo apenas legenda.");
                duration = textOnlyDuration;
            }
            else
            {
                audioSource.Stop();
                audioSource.clip = clip;
                audioSource.Play();
                duration = clip.length;
            }
        }
        else
        {
            // Sem áudio — exibe legenda por tempo fixo
            if (audioSource != null) audioSource.Stop();
            duration = textOnlyDuration;
        }

        if (!string.IsNullOrEmpty(subtitle))
        {
            if (subtitlePanel != null) subtitlePanel.SetActive(true);
            if (subtitleText != null) subtitleText.text = subtitle;
        }

        _hideCoroutine = StartCoroutine(HideAfter(duration));
    }

    public void StopNarration()
    {
        StopHide();
        if (audioSource != null) audioSource.Stop();
        HideSubtitle();
    }

    IEnumerator HideAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        HideSubtitle();
    }

    void HideSubtitle()
    {
        if (subtitlePanel != null) subtitlePanel.SetActive(false);
        if (subtitleText != null) subtitleText.text = "";
    }

    void StopHide()
    {
        if (_hideCoroutine != null)
        {
            StopCoroutine(_hideCoroutine);
            _hideCoroutine = null;
        }
    }
}
