namespace APortataDiLibro.Core.Exceptions;

/// <summary>
/// Sollevata quando le credenziali fornite in fase di login non sono valide.
/// Mappata a HTTP 401 dal middleware globale di gestione errori dell'API.
/// </summary>
public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message) : base(message)
    {
    }
}
