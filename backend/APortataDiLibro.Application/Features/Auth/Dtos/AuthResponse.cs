using APortataDiLibro.Core.Enums;

namespace APortataDiLibro.Application.Features.Auth.Dtos;

/// <summary>
/// Esito di login/registrazione restituito al client. Espone solo dati
/// pubblici dell'utente (Username/Nickname/Ruolo), mai l'anagrafica reale.
/// </summary>
public record AuthResponse(
    Guid Id,
    string Token,
    string Username,
    string Nickname,
    RuoloUtente Ruolo);
