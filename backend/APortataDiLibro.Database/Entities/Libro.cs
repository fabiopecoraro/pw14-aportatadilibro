namespace APortataDiLibro.Database.Entities;

/// <summary>
/// Scheda "globale" di un'opera letteraria, condivisa da tutte le copie possedute
/// dagli utenti. I metadati core (Titolo, Autore) possono essere corretti solo
/// dall'Admin una volta inseriti, per garantire coerenza del catalogo condiviso.
/// </summary>
public class Libro
{
    public Guid Id { get; set; }

    public string Titolo { get; set; } = string.Empty;

    public string Autore { get; set; } = string.Empty;

    /// <summary>
    /// Anno di pubblicazione dell'edizione. Nullable perche' il catalogo e'
    /// alimentato a mano dagli utenti: un anno incerto va lasciato vuoto
    /// invece di essere inventato, e le opere gia' inserite prima
    /// dell'introduzione del campo restano valide.
    /// </summary>
    public int? Anno { get; set; }

    public string? Isbn { get; set; }

    public ICollection<CopiaLibro> Copie { get; set; } = new List<CopiaLibro>();

    public ICollection<LibroCategoria> Categorie { get; set; } = new List<LibroCategoria>();
}
