namespace APortataDiLibro.Database.Entities;

/// <summary>
/// Associazione fra un'opera del catalogo e una <see cref="Categoria"/>.
/// Modellata come entita' esplicita e non come relazione skip-navigation
/// implicita per coerenza con <see cref="EsclusioneHubLibro"/>, l'altra
/// giunzione del progetto: entrambe restano interrogabili direttamente dalle
/// query dei servizi.
/// </summary>
public class LibroCategoria
{
    public Guid LibroId { get; set; }

    public Guid CategoriaId { get; set; }

    public Libro Libro { get; set; } = null!;

    public Categoria Categoria { get; set; } = null!;
}
