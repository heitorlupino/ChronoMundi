using UnityEngine;
using UnityEngine.SceneManagement; // ✅ CORRIGIDO: using necessário para SceneManager

public class MuseumDoor : MonoBehaviour, IInteractable // ✅ CORRIGIDO: implementa IInteractable
{
    [Header("Cena de Destino")]
    public string nomeScene;

    [Header("Feedback Visual (opcional)")]
    public Animator doorAnimator;         // Animação de abrir a porta
    public string   openAnimationTrigger = "Open";

    [Header("Restrição de Acesso (opcional)")]
    public bool requiresProgress = false; // true = só abre se jogador visitou outra cena antes
    public string requiredScene  = "";    // cena que precisa ter sido visitada

    public void Interact()
    {
        // Verifica pré-requisito de progresso
        if (requiresProgress && GameManager.Instance != null)
        {
            if (!GameManager.Instance.HasVisitedScene(requiredScene))
            {
                Debug.Log($"[MuseumDoor] Acesso bloqueado. Visite '{requiredScene}' primeiro.");

                // Aqui você pode tocar uma narração de "porta bloqueada"
                if (NarratorSystem.Instance != null)
                    NarratorSystem.Instance.PlayNarration(null,
                        $"Você precisa explorar '{requiredScene}' antes de entrar aqui.");

                return;
            }
        }

        Debug.Log($"[MuseumDoor] Carregando cena: {nomeScene}");

        // Toca animação de abertura se houver
        if (doorAnimator != null)
            doorAnimator.SetTrigger(openAnimationTrigger);

        LoadingScreen.LoadScene(nomeScene); // ✅ Usa loading assíncrono
    }
}