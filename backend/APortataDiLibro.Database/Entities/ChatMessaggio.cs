namespace APortataDiLibro.Database.Entities;

/// <summary>
/// Messaggio scambiato nella chat privata associata a un <see cref="Prestito"/>.
/// La persistenza dei messaggi (oltre alla diffusione real-time via SignalR)
/// e' necessaria affinche' l'Admin possa ispezionare la cronologia delle
/// chat oggetto di un <see cref="Report"/>.
/// </summary>
public class ChatMessaggio
{
    public Guid Id { get; set; }

    public Guid PrestitoId { get; set; }

    public Guid MittenteId { get; set; }

    public string Testo { get; set; } = string.Empty;

    public DateTime DataInvio { get; set; }

    public Prestito Prestito { get; set; } = null!;

    public Utente Mittente { get; set; } = null!;
}
