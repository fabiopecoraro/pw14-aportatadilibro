using APortataDiLibro.Application.Features.Ricerca;
using APortataDiLibro.Core.Enums;
using APortataDiLibro.Core.Exceptions;
using APortataDiLibro.Database.Context;
using APortataDiLibro.Database.Entities;

namespace APortataDiLibro.Tests;

/// <summary>
/// Ricerca geografica: e' il cuore del prodotto e l'unico punto in cui
/// convergono tre regole indipendenti — il bounding box, la disponibilita'
/// della copia e le esclusioni Copia/Hub. Un errore qui mostra a un utente un
/// libro che non puo' ritirare, o gliene nasconde uno disponibile.
/// </summary>
public class RicercaServiceTests
{
    // Area di riferimento: Salerno. Gli Hub "dentro" stanno in centro, quelli
    // "fuori" abbondantemente oltre il rettangolo.
    private const double Sud = 40.65;
    private const double Ovest = 14.72;
    private const double Nord = 40.72;
    private const double Est = 14.82;

    private static readonly Guid ProprietarioId = Guid.NewGuid();
    private static readonly Guid LibroId = Guid.NewGuid();

    private static ApplicationDbContext DbConUnLibro(
        double latHub = 40.67629,
        double lngHub = 14.75375,
        StatoCopia statoCopia = StatoCopia.Disponibile,
        bool conEsclusione = false)
    {
        var db = TestDbContextFactory.Crea();

        var hubId = Guid.NewGuid();
        var copiaId = Guid.NewGuid();

        db.Utenti.Add(new Utente
        {
            Id = ProprietarioId,
            Username = "proprietario",
            Nickname = "Proprietario",
            Nome = "N",
            Cognome = "C",
            Email = "p@x.it",
            PasswordHash = "h"
        });

        db.Libri.Add(new Libro { Id = LibroId, Titolo = "Il nome della rosa", Autore = "Umberto Eco", Anno = 1980 });

        db.Hub.Add(new Hub
        {
            Id = hubId,
            UtenteId = ProprietarioId,
            Nome = "Piazza della Liberta",
            Indirizzo = "Piazza della Liberta, Salerno",
            Lat = latHub,
            Lng = lngHub
        });

        db.CopieLibro.Add(new CopiaLibro
        {
            Id = copiaId,
            LibroId = LibroId,
            ProprietarioId = ProprietarioId,
            Stato = statoCopia
        });

        if (conEsclusione)
        {
            db.EsclusioniHubLibro.Add(new EsclusioneHubLibro { CopiaLibroId = copiaId, HubId = hubId });
        }

        db.SaveChanges();
        return db;
    }

    [Fact]
    public async Task LibroInAreaEDisponibile_eTrovato()
    {
        using var db = DbConUnLibro();
        var servizio = new RicercaService(db);

        var risultati = await servizio.CercaLibriAsync(Sud, Ovest, Nord, Est, null, null, null);

        var libro = Assert.Single(risultati);
        Assert.Equal("Il nome della rosa", libro.Titolo);
        Assert.Equal(1980, libro.Anno);
        Assert.Single(libro.HubDisponibili);
    }

    [Fact]
    public async Task HubFuoriDallAreaVisibile_nonCompare()
    {
        // Roma: fuori dal rettangolo di Salerno.
        using var db = DbConUnLibro(latHub: 41.90, lngHub: 12.49);
        var servizio = new RicercaService(db);

        var risultati = await servizio.CercaLibriAsync(Sud, Ovest, Nord, Est, null, null, null);

        Assert.Empty(risultati);
    }

    [Fact]
    public async Task CopiaInPrestito_nonCompare()
    {
        using var db = DbConUnLibro(statoCopia: StatoCopia.InPrestito);
        var servizio = new RicercaService(db);

        var risultati = await servizio.CercaLibriAsync(Sud, Ovest, Nord, Est, null, null, null);

        Assert.Empty(risultati);
    }

    [Fact]
    public async Task CopiaEsclusaDallHub_nonCompare()
    {
        // La copia c'e', e' disponibile e l'Hub e' nell'area: sparisce solo per
        // effetto dell'esclusione esplicita del proprietario.
        using var db = DbConUnLibro(conEsclusione: true);
        var servizio = new RicercaService(db);

        var risultati = await servizio.CercaLibriAsync(Sud, Ovest, Nord, Est, null, null, null);

        Assert.Empty(risultati);
    }

    [Fact]
    public async Task LibroPressoPiuHub_eRestituitoUnaVoltaConTuttiGliHub()
    {
        using var db = DbConUnLibro();

        // Secondo Hub dello stesso proprietario, anch'esso nell'area.
        db.Hub.Add(new Hub
        {
            Id = Guid.NewGuid(),
            UtenteId = ProprietarioId,
            Nome = "Villa Comunale",
            Indirizzo = "Villa Comunale, Salerno",
            Lat = 40.67856,
            Lng = 14.75353
        });
        await db.SaveChangesAsync();

        var servizio = new RicercaService(db);
        var risultati = await servizio.CercaLibriAsync(Sud, Ovest, Nord, Est, null, null, null);

        // Il raggruppamento deve dare un libro con due Hub, non due libri.
        var libro = Assert.Single(risultati);
        Assert.Equal(2, libro.HubDisponibili.Count);
    }

    [Fact]
    public async Task FiltroPerCategoria_escludeIlibriSenzaQuellaCategoria()
    {
        using var db = DbConUnLibro();
        var categoriaId = Guid.NewGuid();
        db.Categorie.Add(new Categoria { Id = categoriaId, Nome = "Giallo" });
        db.LibriCategorie.Add(new LibroCategoria { LibroId = LibroId, CategoriaId = categoriaId });
        await db.SaveChangesAsync();

        var servizio = new RicercaService(db);

        var conCategoriaGiusta = await servizio.CercaLibriAsync(Sud, Ovest, Nord, Est, null, null, categoriaId);
        var conCategoriaAssente = await servizio.CercaLibriAsync(Sud, Ovest, Nord, Est, null, null, Guid.NewGuid());

        var trovato = Assert.Single(conCategoriaGiusta);
        Assert.Equal("Giallo", Assert.Single(trovato.Categorie).Nome);
        Assert.Empty(conCategoriaAssente);
    }

    [Theory]
    [InlineData(-91, 14.72, 40.72, 14.82)]  // latitudine sud fuori scala
    [InlineData(40.65, -181, 40.72, 14.82)] // longitudine ovest fuori scala
    [InlineData(40.65, 14.72, 91, 14.82)]   // latitudine nord fuori scala
    public async Task CoordinateFuoriScala_sonoRifiutate(double sud, double ovest, double nord, double est)
    {
        using var db = TestDbContextFactory.Crea();
        var servizio = new RicercaService(db);

        await Assert.ThrowsAsync<ValidationException>(
            () => servizio.CercaLibriAsync(sud, ovest, nord, est, null, null, null));
    }

    [Fact]
    public async Task AreaConAngoliInvertiti_eRifiutata()
    {
        using var db = TestDbContextFactory.Crea();
        var servizio = new RicercaService(db);

        // Nord piu' a sud di sud: rettangolo impossibile, non area vuota.
        await Assert.ThrowsAsync<ValidationException>(
            () => servizio.CercaLibriAsync(Nord, Ovest, Sud, Est, null, null, null));
    }
}
