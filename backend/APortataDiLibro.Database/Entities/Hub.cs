namespace APortataDiLibro.Database.Entities;

/// <summary>
/// Punto di scambio pubblico (es. bar, parco, piazza) registrato da un utente.
/// La posizione e' memorizzata come coppia di coordinate WGS84 (Lat/Lng) e
/// filtrata tramite bounding box calcolati lato applicativo.
/// </summary>
public class Hub
{
    public Guid Id { get; set; }

    public Guid UtenteId { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string Indirizzo { get; set; } = string.Empty;

    public double Lat { get; set; }

    public double Lng { get; set; }

    public Utente Utente { get; set; } = null!;

    public ICollection<EsclusioneHubLibro> Esclusioni { get; set; } = new List<EsclusioneHubLibro>();
}
