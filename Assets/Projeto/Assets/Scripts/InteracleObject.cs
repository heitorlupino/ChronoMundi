using UnityEngine;

public class InteracleObject : MonoBehaviour, IInteractable // ✅ CORRIGIDO: implementa IInteractable
{
    [Header("Narração")]
    public AudioClip narrationClip;

    [TextArea]
    public string subtitleText;

    [Header("Info do Artefato (opcional)")]
    public string artifactName;
    [TextArea]
    public string artifactDescription;

    [Header("Interação Única")]
    public bool interactOnlyOnce = false; // true = pode ser ouvido apenas uma vez

    // ── Evento que o ExhibitManager escuta ────────────────────────────────
    public System.Action<InteracleObject> OnInteracted;

    private bool _hasBeenInteracted = false;

    // ──────────────────────────────────────────────────────────────────────
    public void Interact()
    {
        if (interactOnlyOnce && _hasBeenInteracted)
        {
            Debug.Log($"[InteracleObject] '{artifactName}' já foi explorado.");
            return;
        }

        if (NarratorSystem.Instance != null)
        {
            NarratorSystem.Instance.PlayNarration(narrationClip, subtitleText);
        }
        else
        {
            Debug.LogWarning("InteracleObject: NarratorSystem não encontrado na cena!");
        }

        if (!_hasBeenInteracted)
        {
            _hasBeenInteracted = true;
            OnInteracted?.Invoke(this); // Notifica ExhibitManager
        }
    }

    public bool HasBeenInteracted => _hasBeenInteracted;
}