namespace APortataDiLibro.Core.Settings;

/// <summary>
/// Opzioni per la creazione automatica del primo utente Admin (sezione
/// "AdminBootstrap").
/// <para>
/// Serve a chiudere un vuoto operativo del sistema: la registrazione assegna
/// sempre <see cref="Enums.RuoloUtente.Utente"/> e non esiste alcun endpoint
/// di promozione, quindi su un database appena creato la dashboard Admin
/// sarebbe irraggiungibile senza una UPDATE manuale. Con questa sezione
/// abilitata l'avvio dell'applicazione crea l'account amministrativo se e
/// solo se non ne esiste ancora nessuno.
/// </para>
/// </summary>
public class AdminBootstrapSettings
{
    public const string SectionName = "AdminBootstrap";

    /// <summary>
    /// Interruttore esplicito: la creazione di un account privilegiato non
    /// deve mai essere un effetto collaterale implicito dell'avvio.
    /// </summary>
    public bool Abilitato { get; set; }

    public string Username { get; set; } = string.Empty;

    public string Nickname { get; set; } = string.Empty;

    public string Nome { get; set; } = string.Empty;

    public string Cognome { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Password in chiaro, letta dalla configurazione e mai persistita come
    /// tale: viene immediatamente sottoposta a hash PBKDF2. Va tenuta fuori
    /// da appsettings.json in favore di user-secrets o variabili d'ambiente
    /// in qualunque scenario diverso dallo sviluppo locale.
    /// </summary>
    public string Password { get; set; } = string.Empty;
}
