namespace APortataDiLibro.Database.Entities;

/// <summary>
/// Secondo livello del sistema di recensioni: valuta l'opera letteraria in se',
/// indipendentemente dalla copia o dal prestito tramite cui e' stata letta.
/// Contribuisce alla "Media Voto Recensioni" mostrata nel report CSV dell'Admin.
/// </summary>
public class RecensioneLibro
{
    public Guid Id { get; set; }

    public Guid LibroId { get; set; }

    public Guid AutoreId { get; set; }

    public int Voto { get; set; }

    public string? Commento { get; set; }

    public DateTime DataCreazione { get; set; }

    public Libro Libro { get; set; } = null!;

    public Utente Autore { get; set; } = null!;
}
