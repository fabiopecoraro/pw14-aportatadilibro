namespace APortataDiLibro.Database.Entities;

/// <summary>
/// Richiesta di un utente di essere avvisato (notifica simulata o email in
/// background) quando una copia di <see cref="Libro"/> torna disponibile,
/// nel caso in cui al momento della richiesta non vi siano copie libere.
/// </summary>
public class SottoscrizioneDisponibilita
{
    public Guid Id { get; set; }

    public Guid LibroId { get; set; }

    public Guid UtenteId { get; set; }

    public DateTime DataRichiesta { get; set; }

    public bool Notificata { get; set; }

    public Libro Libro { get; set; } = null!;

    public Utente Utente { get; set; } = null!;
}
