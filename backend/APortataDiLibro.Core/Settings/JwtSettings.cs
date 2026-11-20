namespace APortataDiLibro.Core.Settings;

/// <summary>
/// Opzioni di configurazione per l'emissione e la validazione dei JWT,
/// risolte da appsettings tramite binding (sezione "Jwt") sia nel layer API
/// (validazione token in ingresso) sia nel layer Application (emissione token in login).
/// </summary>
public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    public string Secret { get; set; } = string.Empty;

    public int ExpirationMinutes { get; set; } = 60;
}
