namespace APortataDiLibro.Application.Features.Admin;

/// <summary>
/// Garantisce l'esistenza di un utente con ruolo Admin all'avvio
/// dell'applicazione, quando la sezione di configurazione "AdminBootstrap"
/// e' abilitata.
/// </summary>
public interface IAdminBootstrapper
{
    /// <summary>
    /// Crea l'account amministrativo configurato se nel database non esiste
    /// ancora alcun Admin. Non solleva mai eccezioni bloccanti: un problema
    /// qui non deve impedire l'avvio dell'API.
    /// </summary>
    Task EnsureAdminAsync(CancellationToken cancellationToken = default);
}
