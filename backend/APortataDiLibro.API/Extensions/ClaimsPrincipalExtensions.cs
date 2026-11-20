using System.Security.Claims;
using APortataDiLibro.Core.Constants;

namespace APortataDiLibro.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    /// <summary>Estrae l'identificativo dell'utente autenticato dalla claim "sub" del JWT.</summary>
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(AuthConstants.UserIdClaimType)
            ?? throw new InvalidOperationException("Claim utente mancante nel token.");

        return Guid.Parse(value);
    }
}
