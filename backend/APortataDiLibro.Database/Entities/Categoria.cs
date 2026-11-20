namespace APortataDiLibro.Database.Entities;

/// <summary>
/// Voce del vocabolario controllato delle categorie del catalogo.
/// <para>
/// Le categorie sono un elenco chiuso gestito dall'Admin, non testo libero
/// digitato dagli utenti: un vocabolario aperto si riempie in fretta di
/// varianti della stessa voce ("Giallo", "gialli", "Giallo/Thriller") che
/// spezzano i filtri e rendono inutilizzabile la navigazione per categoria.
/// E' la stessa logica per cui i metadati core del <see cref="Libro"/> sono
/// correggibili solo dall'Admin: il catalogo e' condiviso da tutti.
/// </para>
/// </summary>
public class Categoria
{
    public Guid Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public ICollection<LibroCategoria> Libri { get; set; } = new List<LibroCategoria>();

    public ICollection<PropostaLibroCategoria> Proposte { get; set; } = new List<PropostaLibroCategoria>();
}
