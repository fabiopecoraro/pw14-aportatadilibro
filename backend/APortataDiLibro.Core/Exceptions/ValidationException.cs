namespace APortataDiLibro.Core.Exceptions;

/// <summary>
/// Sollevata quando i dati di input non superano le regole di validazione
/// applicative (distinte dalle data annotation, per regole che richiedono
/// accesso al dominio, es. coerenza geospaziale o stato del prestito).
/// Mappata a HTTP 400 dal middleware globale di gestione errori dell'API.
/// </summary>
public class ValidationException : Exception
{
    public ValidationException(string message) : base(message)
    {
    }

    public ValidationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
