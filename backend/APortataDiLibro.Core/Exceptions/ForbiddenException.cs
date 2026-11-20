namespace APortataDiLibro.Core.Exceptions;

/// <summary>
/// Sollevata quando l'utente autenticato non ha i permessi per eseguire
/// l'operazione richiesta (es. modifica metadati libro non da Admin,
/// chiusura prestito non da proprietario della copia).
/// Mappata a HTTP 403 dal middleware globale di gestione errori dell'API.
/// </summary>
public class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message)
    {
    }
}
