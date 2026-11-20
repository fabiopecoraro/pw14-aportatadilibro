using APortataDiLibro.Application.Features.Chat.Dtos;

namespace APortataDiLibro.Application.Features.Chat;

public interface IChatService
{
    /// <summary>
    /// Verifica che <paramref name="utenteId"/> sia uno dei due partecipanti
    /// alla chat del prestito (richiedente o proprietario della copia).
    /// La chat e' utilizzabile fin dalla creazione della richiesta, prima
    /// ancora che il prestito venga accettato.
    /// </summary>
    Task<bool> UtenteAutorizzatoAsync(Guid prestitoId, Guid utenteId, CancellationToken cancellationToken = default);

    /// <summary>Cronologia completa dei messaggi di un prestito, in ordine cronologico.</summary>
    Task<IReadOnlyList<ChatMessaggioDto>> GetCronologiaAsync(Guid prestitoId, Guid utenteId, CancellationToken cancellationToken = default);

    /// <summary>Persiste un nuovo messaggio inviato da uno dei partecipanti alla chat.</summary>
    Task<ChatMessaggioDto> InviaMessaggioAsync(Guid prestitoId, Guid mittenteId, string testo, CancellationToken cancellationToken = default);
}
