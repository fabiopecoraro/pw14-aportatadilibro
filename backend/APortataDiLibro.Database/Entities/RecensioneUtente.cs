namespace APortataDiLibro.Database.Entities;

/// <summary>
/// Primo livello del sistema di recensioni: valuta l'affidabilita' della
/// controparte di uno scambio (puntualita', correttezza nella consegna, ecc.).
/// Una sola recensione per autore per ciascun prestito concluso.
/// </summary>
public class RecensioneUtente
{
    public Guid Id { get; set; }

    public Guid PrestitoId { get; set; }

    public Guid AutoreId { get; set; }

    public Guid UtenteRecensitoId { get; set; }

    public int Voto { get; set; }

    public string? Commento { get; set; }

    public DateTime DataCreazione { get; set; }

    public Prestito Prestito { get; set; } = null!;

    public Utente Autore { get; set; } = null!;

    public Utente UtenteRecensito { get; set; } = null!;
}
