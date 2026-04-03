using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Objeto interagível (artefato/exhibit) do ChronoMundi.
/// Toca narração, exibe painel de informações e notifica o ExhibitManager.
/// </summary>
public class InteracleObject : MonoBehaviour, IInteractable
{
    [Header("Narração")]
    public AudioClip narrationClip;
    [TextArea]
    public string subtitleText;

    [Header("Info do Artefato")]
    public string artifactName;
    [TextArea]
    public string artifactDescription;

    [Header("Interação Única")]
    public bool interactOnlyOnce = false;

    // event (não field público) — impede código externo de invocar ou sobrescrever
    public event System.Action<InteracleObject> OnInteracted;

    private bool _hasBeenInteracted = false;

    // ──────────────────────────────────────────────────────────────────────
    public void Interact()
    {
        if (interactOnlyOnce && _hasBeenInteracted)
        {
            Debug.Log($"[InteracleObject] '{artifactName}' já foi explorado.");
            return;
        }

        // Narração / legenda
        if (NarratorSystem.Instance != null)
            NarratorSystem.Instance.PlayNarration(narrationClip, subtitleText);
        else
            Debug.LogWarning("[InteracleObject] NarratorSystem não encontrado!");

        // Painel de informações do artefato
        if (ArtifactInfoPanel.Instance != null)
            ArtifactInfoPanel.Instance.Show(artifactName, artifactDescription);

        if (!_hasBeenInteracted)
        {
            _hasBeenInteracted = true;
            OnInteracted?.Invoke(this);
        }
    }

    public bool HasBeenInteracted => _hasBeenInteracted;
}
