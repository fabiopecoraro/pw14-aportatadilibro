namespace APortataDiLibro.Core.Constants;

/// <summary>
/// Nomi condivisi tra Application (emissione JWT) e API (configurazione
/// delle policy di autorizzazione), per evitare letterali duplicati e
/// disallineamenti tra i due layer.
/// </summary>
public static class AuthConstants
{
    /// <summary>Nome della policy di autorizzazione riservata al ruolo Admin.</summary>
    public const string AdminPolicy = "AdminOnly";

    /// <summary>Claim type utilizzato per veicolare il ruolo dell'utente nel JWT.</summary>
    public const string RoleClaimType = "role";

    /// <summary>Claim type utilizzato per veicolare l'identificativo dell'utente nel JWT.</summary>
    public const string UserIdClaimType = "sub";

    /// <summary>
    /// Lunghezza minima della password, applicata sia in registrazione sia in
    /// cambio password. Vive qui perche' i due percorsi sono gestiti da servizi
    /// diversi e il vincolo deve restare uno solo.
    /// </summary>
    public const int LunghezzaMinimaPassword = 8;
}
