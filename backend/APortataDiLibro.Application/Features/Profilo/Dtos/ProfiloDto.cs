namespace APortataDiLibro.Application.Features.Profilo.Dtos;

/// <summary>
/// Profilo completo dell'utente autenticato, restituito solo a lui stesso.
/// A differenza dei DTO pubblici (che espongono il solo <c>Nickname</c>)
/// include l'anagrafica reale, perche' e' il proprietario del dato a leggerla
/// per poterla correggere.
/// </summary>
public record ProfiloDto(
    Guid Id,
    string Username,
    string Nickname,
    string Nome,
    string Cognome,
    string Email,
    DateTime DataRegistrazione,
    DateTime? DataAccettazioneTermini);
