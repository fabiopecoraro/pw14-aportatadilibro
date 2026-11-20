using APortataDiLibro.Application.Features.Categorie;
using APortataDiLibro.Application.Features.Categorie.Dtos;
using APortataDiLibro.Core.Exceptions;
using APortataDiLibro.Database.Entities;

namespace APortataDiLibro.Tests;

/// <summary>
/// Regole del vocabolario controllato: sono la ragione per cui il vocabolario
/// esiste, quindi sono anche le regole che non devono poter regredire.
/// </summary>
public class CategorieServiceTests
{
    private static Categoria Categoria(string nome, Guid? id = null) =>
        new() { Id = id ?? Guid.NewGuid(), Nome = nome };

    [Fact]
    public async Task Crea_normalizzaGliSpaziEsterni()
    {
        using var db = TestDbContextFactory.Crea();
        var servizio = new CategorieService(db);

        var creata = await servizio.CreaAsync(new SalvaCategoriaRequest("  Narrativa  "));

        Assert.Equal("Narrativa", creata.Nome);
    }

    [Fact]
    public async Task Crea_rifiutaIlNomeVuoto()
    {
        using var db = TestDbContextFactory.Crea();
        var servizio = new CategorieService(db);

        await Assert.ThrowsAsync<ValidationException>(
            () => servizio.CreaAsync(new SalvaCategoriaRequest("   ")));
    }

    [Theory]
    [InlineData("giallo")]
    [InlineData("GIALLO")]
    [InlineData("GiAlLo")]
    public async Task Crea_rifiutaUnDuplicatoIndipendentementeDalleMaiuscole(string variante)
    {
        using var db = TestDbContextFactory.Crea();
        db.Categorie.Add(Categoria("Giallo"));
        await db.SaveChangesAsync();

        var servizio = new CategorieService(db);

        // E' il caso che distingue un vocabolario controllato da un campo di
        // testo: senza questo controllo il vocabolario si riempie di varianti
        // della stessa voce e i filtri si spezzano.
        await Assert.ThrowsAsync<ConflictException>(
            () => servizio.CreaAsync(new SalvaCategoriaRequest(variante)));
    }

    [Fact]
    public async Task Rinomina_consenteDiCambiareSoloLeMaiuscoleDelloStessoNome()
    {
        using var db = TestDbContextFactory.Crea();
        var id = Guid.NewGuid();
        db.Categorie.Add(Categoria("giallo", id));
        await db.SaveChangesAsync();

        var servizio = new CategorieService(db);

        // Il controllo di unicita' deve escludere la categoria stessa,
        // altrimenti correggere "giallo" in "Giallo" sarebbe impossibile.
        var rinominata = await servizio.RinominaAsync(id, new SalvaCategoriaRequest("Giallo"));

        Assert.Equal("Giallo", rinominata.Nome);
    }

    [Fact]
    public async Task Elimina_eBloccataSeUnOperaUsaLaCategoria()
    {
        using var db = TestDbContextFactory.Crea();
        var categoriaId = Guid.NewGuid();
        var libroId = Guid.NewGuid();
        db.Categorie.Add(Categoria("Giallo", categoriaId));
        db.Libri.Add(new Libro { Id = libroId, Titolo = "T", Autore = "A" });
        db.LibriCategorie.Add(new LibroCategoria { LibroId = libroId, CategoriaId = categoriaId });
        await db.SaveChangesAsync();

        var servizio = new CategorieService(db);

        await Assert.ThrowsAsync<ConflictException>(() => servizio.EliminaAsync(categoriaId));
    }

    [Fact]
    public async Task Elimina_eBloccataAnchePerUnaSolaPropostaInAttesa()
    {
        using var db = TestDbContextFactory.Crea();
        var categoriaId = Guid.NewGuid();
        var propostaId = Guid.NewGuid();
        var proponenteId = Guid.NewGuid();
        db.Categorie.Add(Categoria("Diari", categoriaId));
        db.Utenti.Add(new Utente { Id = proponenteId, Username = "u", Nickname = "U", Nome = "N", Cognome = "C", Email = "u@x.it", PasswordHash = "h" });
        db.ProposteLibro.Add(new PropostaLibro { Id = propostaId, Titolo = "T", Autore = "A", ProponenteId = proponenteId });
        db.ProposteLibroCategorie.Add(new PropostaLibroCategoria { PropostaLibroId = propostaId, CategoriaId = categoriaId });
        await db.SaveChangesAsync();

        var servizio = new CategorieService(db);

        // Caso facile da dimenticare: nessuna opera la usa, ma la proposta si'.
        await Assert.ThrowsAsync<ConflictException>(() => servizio.EliminaAsync(categoriaId));
    }

    [Fact]
    public async Task Elimina_riesceQuandoNessunoUsaLaCategoria()
    {
        using var db = TestDbContextFactory.Crea();
        var id = Guid.NewGuid();
        db.Categorie.Add(Categoria("Filosofia", id));
        await db.SaveChangesAsync();

        var servizio = new CategorieService(db);
        await servizio.EliminaAsync(id);

        Assert.Empty(db.Categorie);
    }

    [Fact]
    public async Task RisolviIds_rifiutaUnInsiemeVuoto()
    {
        using var db = TestDbContextFactory.Crea();
        var servizio = new CategorieService(db);

        await Assert.ThrowsAsync<ValidationException>(() => servizio.RisolviIdsAsync([]));
        await Assert.ThrowsAsync<ValidationException>(() => servizio.RisolviIdsAsync(null));
    }

    [Fact]
    public async Task RisolviIds_rifiutaOltreIlMassimo()
    {
        using var db = TestDbContextFactory.Crea();
        var ids = Enumerable.Range(0, 6).Select(_ => Guid.NewGuid()).ToList();
        db.Categorie.AddRange(ids.Select(id => Categoria($"C{id}", id)));
        await db.SaveChangesAsync();

        var servizio = new CategorieService(db);

        await Assert.ThrowsAsync<ValidationException>(() => servizio.RisolviIdsAsync(ids));
    }

    [Fact]
    public async Task RisolviIds_deduplicaSenzaSegnalareErrore()
    {
        using var db = TestDbContextFactory.Crea();
        var id = Guid.NewGuid();
        db.Categorie.Add(Categoria("Narrativa", id));
        await db.SaveChangesAsync();

        var servizio = new CategorieService(db);
        var risolti = await servizio.RisolviIdsAsync([id, id, id]);

        Assert.Equal([id], risolti);
    }

    [Fact]
    public async Task RisolviIds_rifiutaUnaCategoriaInesistente()
    {
        using var db = TestDbContextFactory.Crea();
        var esistente = Guid.NewGuid();
        db.Categorie.Add(Categoria("Narrativa", esistente));
        await db.SaveChangesAsync();

        var servizio = new CategorieService(db);

        await Assert.ThrowsAsync<ValidationException>(
            () => servizio.RisolviIdsAsync([esistente, Guid.NewGuid()]));
    }

    [Fact]
    public async Task GetPerGestione_riportaEntrambiIConteggiDUso()
    {
        using var db = TestDbContextFactory.Crea();
        var categoriaId = Guid.NewGuid();
        var libroId = Guid.NewGuid();
        var propostaId = Guid.NewGuid();
        var proponenteId = Guid.NewGuid();

        db.Categorie.Add(Categoria("Narrativa", categoriaId));
        db.Libri.Add(new Libro { Id = libroId, Titolo = "T", Autore = "A" });
        db.LibriCategorie.Add(new LibroCategoria { LibroId = libroId, CategoriaId = categoriaId });
        db.Utenti.Add(new Utente { Id = proponenteId, Username = "u", Nickname = "U", Nome = "N", Cognome = "C", Email = "u@x.it", PasswordHash = "h" });
        db.ProposteLibro.Add(new PropostaLibro { Id = propostaId, Titolo = "T", Autore = "A", ProponenteId = proponenteId });
        db.ProposteLibroCategorie.Add(new PropostaLibroCategoria { PropostaLibroId = propostaId, CategoriaId = categoriaId });
        await db.SaveChangesAsync();

        var servizio = new CategorieService(db);
        var voce = Assert.Single(await servizio.GetPerGestioneAsync());

        // Sono i due numeri su cui la dashboard decide se abilitare Elimina.
        Assert.Equal(1, voce.NumeroLibri);
        Assert.Equal(1, voce.NumeroProposte);
    }
}
