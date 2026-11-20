using APortataDiLibro.Application.Features.Profilo.Dtos;

namespace APortataDiLibro.Application.Features.Profilo;

/// <summary>
/// Lettura e modifica dei dati del proprio profilo utente.
/// </summary>
public interface IProfiloService
{
    Task<ProfiloDto> GetProfiloAsync(Guid utenteId, CancellationToken cancellationToken = default);

    Task<ProfiloDto> AggiornaProfiloAsync(Guid utenteId, AggiornaProfiloRequest request, CancellationToken cancellationToken = default);

    Task CambiaPasswordAsync(Guid utenteId, CambiaPasswordRequest request, CancellationToken cancellationToken = default);
}
