using APortataDiLibro.Core.Enums;

namespace APortataDiLibro.Database.Entities;

/// <summary>
/// Utente registrato della piattaforma. I dati anagrafici reali (Nome, Cognome, Email)
/// sono raccolti in fase di registrazione ma non vengono mai esposti pubblicamente:
/// la UI mostra esclusivamente <see cref="Nickname"/>.
/// </summary>
public class Utente
{
    public Guid Id { get; set; }

    public string Username { get; set; } = string.Empty;

    public string Nickname { get; set; } = string.Empty;

    public string Nome { get; set; } = string.Empty;

    public string Cognome { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public RuoloUtente Ruolo { get; set; } = RuoloUtente.Utente;

    public DateTime DataRegistrazione { get; set; }

    /// <summary>
    /// Momento in cui l'utente ha accettato informativa privacy e termini in
    /// fase di registrazione. E' la prova del consenso richiesta dalla
    /// traccia: nullable solo per gli account creati prima dell'introduzione
    /// del controllo (e per l'Admin di bootstrap), mai per le registrazioni
    /// successive, che senza consenso non vengono nemmeno create.
    /// </summary>
    public DateTime? DataAccettazioneTermini { get; set; }

    public ICollection<Hub> Hub { get; set; } = new List<Hub>();

    public ICollection<CopiaLibro> Copie { get; set; } = new List<CopiaLibro>();
}
