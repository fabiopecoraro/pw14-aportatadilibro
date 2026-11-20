namespace APortataDiLibro.Database.Entities;

/// <summary>
/// Categorie scelte dall'utente per una <see cref="PropostaLibro"/>.
/// Anche in proposta si sceglie dal vocabolario esistente invece di digitare
/// testo libero: l'approvazione si limita cosi' a ricopiare le associazioni
/// sull'opera creata, senza dover interpretare o normalizzare nulla.
/// </summary>
public class PropostaLibroCategoria
{
    public Guid PropostaLibroId { get; set; }

    public Guid CategoriaId { get; set; }

    public PropostaLibro PropostaLibro { get; set; } = null!;

    public Categoria Categoria { get; set; } = null!;
}
