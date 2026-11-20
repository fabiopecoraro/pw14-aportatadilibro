using APortataDiLibro.Application.Features.Prestiti.Dtos;

namespace APortataDiLibro.Application.Features.Prestiti;

public interface IPrestitoService
{
    /// <summary>Richiede in prestito una copia disponibile di proprieta' altrui.</summary>
    Task<PrestitoDto> RichiediPrestitoAsync(Guid copiaId, Guid richiedenteId, RichiediPrestitoRequest request, CancellationToken cancellationToken = default);

    /// <summary>Accetta una richiesta in attesa: attiva il prestito e segna la copia come InPrestito.</summary>
    Task<PrestitoDto> AccettaPrestitoAsync(Guid prestitoId, Guid utenteId, CancellationToken cancellationToken = default);

    /// <summary>Rifiuta una richiesta in attesa, lasciando la copia disponibile.</summary>
    Task<PrestitoDto> RifiutaPrestitoAsync(Guid prestitoId, Guid utenteId, CancellationToken cancellationToken = default);

    /// <summary>Annulla una richiesta ancora in attesa, da parte del richiedente.</summary>
    Task<PrestitoDto> AnnullaPrestitoAsync(Guid prestitoId, Guid utenteId, CancellationToken cancellationToken = default);

    /// <summary>Chiude il prestito alla restituzione fisica della copia. Permesso solo al proprietario.</summary>
    Task<PrestitoDto> CompletaPrestitoAsync(Guid prestitoId, Guid utenteId, CancellationToken cancellationToken = default);

    /// <summary>Richieste di prestito inviate dall'utente per copie altrui.</summary>
    Task<IReadOnlyList<PrestitoDto>> GetPrestitiInviatiAsync(Guid richiedenteId, CancellationToken cancellationToken = default);

    /// <summary>Richieste di prestito ricevute sulle copie possedute dall'utente.</summary>
    Task<IReadOnlyList<PrestitoDto>> GetPrestitiRicevutiAsync(Guid proprietarioId, CancellationToken cancellationToken = default);
}
