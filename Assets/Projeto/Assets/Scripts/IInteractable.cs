/// <summary>
/// Interface que todos os objetos interagíveis do ChronoMundi devem implementar.
/// Usada pelo PlayerInteraction para disparar ações sem depender de tipo concreto.
///
/// Scripts que implementam esta interface:
///   - InteracleObject   (artefatos/exhibits)
///   - MuseumDoor        (portas de viagem no Hub)
///   - SimpleReturnToMuseum (pontos de retorno ao museu)
/// </summary>
public interface IInteractable
{
    /// <summary>
    /// Chamado pelo PlayerInteraction quando o jogador aciona a interação
    /// (tecla E no Desktop ou trigger do controller no VR).
    /// </summary>
    void Interact();
}
