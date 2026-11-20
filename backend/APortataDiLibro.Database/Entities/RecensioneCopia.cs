namespace APortataDiLibro.Database.Entities;

/// <summary>
/// Terzo livello del sistema di recensioni: valuta l'esperienza specifica di
/// scambio (stato fisico della copia, puntualita' dell'incontro, ecc.) legata
/// a un singolo prestito concluso.
/// </summary>
public class RecensioneCopia
{
    public Guid Id { get; set; }

    public Guid PrestitoId { get; set; }

    public Guid AutoreId { get; set; }

    public int Voto { get; set; }

    public string? Commento { get; set; }

    public DateTime DataCreazione { get; set; }

    public Prestito Prestito { get; set; } = null!;

    public Utente Autore { get; set; } = null!;
}
