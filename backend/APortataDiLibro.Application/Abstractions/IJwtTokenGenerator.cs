using APortataDiLibro.Database.Entities;

namespace APortataDiLibro.Application.Abstractions;

/// <summary>
/// Astrazione per l'emissione dei JWT di accesso a partire da un utente autenticato.
/// </summary>
public interface IJwtTokenGenerator
{
    /// <summary>Genera un token firmato contenente le claim minime per RBAC e identificazione utente.</summary>
    string GenerateToken(Utente utente);
}
