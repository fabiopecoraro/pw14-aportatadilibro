namespace APortataDiLibro.Core.Exceptions;

/// <summary>
/// Sollevata quando un'entita' richiesta per chiave primaria non esiste.
/// Mappata a HTTP 404 dal middleware globale di gestione errori dell'API.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }

    public NotFoundException(string entityName, object key)
        : base($"L'entità '{entityName}' con chiave '{key}' non è stata trovata.")
    {
    }
}
