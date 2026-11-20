using APortataDiLibro.Application.Features.Notifiche.Dtos;

namespace APortataDiLibro.Application.Features.Notifiche;

public interface ISottoscrizioniService
{
    /// <summary>
    /// Iscrive l'utente alle notifiche di disponibilita' per un libro che al
    /// momento non ha copie libere. Se l'utente era gia' stato notificato in
    /// passato, riattiva la sottoscrizione.
    /// </summary>
    Task<SottoscrizioneDisponibilitaDto> SottoscriviAsync(Guid utenteId, Guid libroId, CancellationToken cancellationToken = default);

    /// <summary>Annulla una sottoscrizione attiva (non ancora notificata).</summary>
    Task AnnullaSottoscrizioneAsync(Guid utenteId, Guid libroId, CancellationToken cancellationToken = default);

    /// <summary>Sottoscrizioni attive dell'utente.</summary>
    Task<IReadOnlyList<SottoscrizioneDisponibilitaDto>> GetSottoscrizioniUtenteAsync(Guid utenteId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifica (in modo simulato/asincrono) tutti gli iscritti non ancora
    /// avvisati che una copia del libro e' tornata disponibile, e marca le
    /// rispettive sottoscrizioni come evase.
    /// </summary>
    Task NotificaDisponibilitaAsync(Guid libroId, CancellationToken cancellationToken = default);
}
