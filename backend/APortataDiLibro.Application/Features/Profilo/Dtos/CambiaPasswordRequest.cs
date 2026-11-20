namespace APortataDiLibro.Application.Features.Profilo.Dtos;

/// <summary>
/// Cambio password dell'utente autenticato. La password attuale e' richiesta
/// anche a fronte di un token valido: un token rubato non deve bastare a
/// prendere possesso permanente dell'account.
/// </summary>
public record CambiaPasswordRequest(
    string PasswordAttuale,
    string NuovaPassword);
