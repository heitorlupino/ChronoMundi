using System.Collections;
using UnityEngine;
using UnityEngine.Playables; // Timeline

/// <summary>
/// Controlador de introdução de cada era histórica.
/// Ao carregar a cena, toca uma sequência de entrada:
///   1. Fade in (via SceneFader)
///   2. Narração de boas-vindas
///   3. (Opcional) Timeline do Unity para cinemática de entrada
///   4. Libera o controle do jogador
///
/// Coloque UM deste por cena histórica (PreHistoria, IdadeMedia, FuturoTech).
/// </summary>
public class TimelineEra : MonoBehaviour
{
    [Header("Identificação da Era")]
    public string eraName = "Pré-História";

    [Header("Narração de Entrada")]
    public AudioClip introClip;
    [TextArea]
    public string introSubtitle = "Bem-vindo à Pré-História...";
    public float delayBeforeNarration = 1.2f;

    [Header("Timeline (opcional)")]
    [Tooltip("Arraste aqui um PlayableDirector para rodar uma cinemática de entrada.")]
    public PlayableDirector introTimeline;

    [Header("Jogador")]
    [Tooltip("Desabilita o controle do jogador durante a intro e re-habilita ao fim.")]
    public MonoBehaviour playerController; // DesktopPlayerController ou XR Rig

    [Header("Retorno ao Museu")]
    [Tooltip("Objeto com SimpleReturnToMuseum — posicionado na saída da cena.")]
    public SimpleReturnToMuseum returnPoint;

    // ──────────────────────────────────────────────────────────────────────
    void Start()
    {
        // Registra visita no GameManager
        if (GameManager.Instance != null)
            Debug.Log($"[TimelineEra] Cena '{eraName}' iniciada.");

        // Congela jogador durante intro
        SetPlayerControl(false);

        StartCoroutine(PlayIntroSequence());
    }

    // ──────────────────────────────────────────────────────────────────────
    IEnumerator PlayIntroSequence()
    {
        // 1. Aguarda fade in da SceneFader (se existir)
        yield return new WaitForSeconds(delayBeforeNarration);

        // 2. Narração de boas-vindas
        if (NarratorSystem.Instance != null)
            NarratorSystem.Instance.PlayNarration(introClip, introSubtitle);

        float narratorWait = introClip != null ? introClip.length : 3f;

        // 3. Timeline de entrada (opcional)
        if (introTimeline != null)
        {
            introTimeline.Play();
            yield return new WaitForSeconds((float)introTimeline.duration);
        }
        else
        {
            yield return new WaitForSeconds(narratorWait);
        }

        // 4. Libera jogador
        SetPlayerControl(true);
    }

    void SetPlayerControl(bool enabled)
    {
        if (playerController != null)
            playerController.enabled = enabled;
    }

    // ── Chamado pelo ExhibitManager quando todos os artefatos forem explorados
    public void OnAllExhibitsComplete()
    {
        if (NarratorSystem.Instance != null)
            NarratorSystem.Instance.PlayNarration(null,
                $"Você explorou todos os artefatos de {eraName}! Retorne ao museu.");

        // Destaca o ponto de retorno
        if (returnPoint != null)
        {
            var highlight = returnPoint.GetComponent<HighlightController>();
            if (highlight != null) highlight.StartHighlight();
        }
    }
}
