using APortataDiLibro.Application.Features.Hub.Dtos;

namespace APortataDiLibro.Application.Features.Hub;

public interface IHubService
{
    Task<HubDto> CreaHubAsync(Guid utenteId, CreaHubRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<HubDto>> GetHubUtenteAsync(Guid utenteId, CancellationToken cancellationToken = default);

    Task EliminaHubAsync(Guid hubId, Guid utenteId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Elenca gli Hub del proprietario della copia indicando, per ciascuno, se
    /// la copia vi e' esclusa. Serve al frontend per presentare in un'unica
    /// schermata la disponibilita' effettiva della copia.
    /// </summary>
    Task<IReadOnlyList<DisponibilitaHubCopiaDto>> GetDisponibilitaCopiaAsync(Guid copiaId, Guid utenteId, CancellationToken cancellationToken = default);

    /// <summary>Esclude una copia posseduta dall'utente da uno dei suoi Hub.</summary>
    Task EscludiCopiaAsync(Guid copiaId, Guid hubId, Guid utenteId, CancellationToken cancellationToken = default);

    /// <summary>Rimuove un'esclusione precedentemente impostata, ripristinando la disponibilita' di default.</summary>
    Task RimuoviEsclusioneAsync(Guid copiaId, Guid hubId, Guid utenteId, CancellationToken cancellationToken = default);
}
