namespace APortataDiLibro.Core.Exceptions;

/// <summary>
/// Sollevata quando un'operazione confligge con lo stato corrente del dominio
/// (es. richiesta di prestito su una copia gia' InPrestito).
/// Mappata a HTTP 409 dal middleware globale di gestione errori dell'API.
/// </summary>
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message)
    {
    }
}
