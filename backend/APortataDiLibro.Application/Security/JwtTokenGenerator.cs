using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using APortataDiLibro.Application.Abstractions;
using APortataDiLibro.Core.Constants;
using APortataDiLibro.Core.Settings;
using APortataDiLibro.Database.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace APortataDiLibro.Application.Security;

/// <summary>
/// Emette JWT firmati con chiave simmetrica (HMAC-SHA256) usando i parametri
/// di <see cref="JwtSettings"/>, condivisi con la pipeline di validazione
/// configurata nel layer API affinche' i token emessi qui siano accettati
/// dal middleware di autenticazione.
/// </summary>
public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtSettings _settings;

    public JwtTokenGenerator(IOptions<JwtSettings> settings)
    {
        _settings = settings.Value;
    }

    public string GenerateToken(Utente utente)
    {
        var claims = new[]
        {
            new Claim(AuthConstants.UserIdClaimType, utente.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, utente.Username),
            new Claim(AuthConstants.RoleClaimType, utente.Ruolo.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_settings.ExpirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
