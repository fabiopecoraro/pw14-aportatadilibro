using APortataDiLibro.Application.Features.Metriche;
using APortataDiLibro.Core.Enums;
using APortataDiLibro.Database.Entities;
using Microsoft.Extensions.Logging.Abstractions;

namespace APortataDiLibro.Tests;

/// <summary>
/// Metriche d'uso della dashboard. Il comportamento che conta e' il
/// riempimento a zero dei giorni senza eventi: una serie che salta i giorni
/// vuoti comprime l'asse del tempo e disegna un andamento che non e' quello
/// reale.
/// </summary>
public class MetricheServiceTests
{
    private static readonly Guid LibroId = Guid.NewGuid();

    [Fact]
    public async Task SerieVisualizzazioni_haUnPuntoPerGiornoAncheSenzaEventi()
    {
        using var db = TestDbContextFactory.Crea();
        db.Libri.Add(new Libro { Id = LibroId, Titolo = "T", Autore = "A" });
        db.VisualizzazioniLibro.Add(new VisualizzazioneLibro
        {
            LibroId = LibroId,
            Giorno = DateOnly.FromDateTime(DateTime.UtcNow),
            Conteggio = 5
        });
        await db.SaveChangesAsync();

        var servizio = new MetricheService(db, NullLogger<MetricheService>.Instance);
        var metriche = await servizio.GetMetricheUsoAsync(7);

        Assert.Equal(7, metriche.SerieVisualizzazioni.Count);
        Assert.Equal(7, metriche.SerieRichiestePrestito.Count);
        // Un solo giorno ha dati: gli altri sei devono esserci comunque, a zero.
        Assert.Equal(6, metriche.SerieVisualizzazioni.Count(p => p.Valore == 0));
        Assert.Equal(5, metriche.SerieVisualizzazioni.Last().Valore);
    }

    [Fact]
    public async Task SerieOrdinata_dalPiuVecchioAlPiuRecente()
    {
        using var db = TestDbContextFactory.Crea();
        var servizio = new MetricheService(db, NullLogger<MetricheService>.Instance);

        var metriche = await servizio.GetMetricheUsoAsync(30);

        var giorni = metriche.SerieVisualizzazioni.Select(p => p.Giorno).ToList();
        Assert.Equal(giorni.OrderBy(g => g), giorni);
        Assert.Equal(DateOnly.FromDateTime(DateTime.UtcNow), giorni.Last());
    }

    [Fact]
    public async Task PeriodoFuoriScala_eRiportatoNeiLimiti()
    {
        using var db = TestDbContextFactory.Crea();
        var servizio = new MetricheService(db, NullLogger<MetricheService>.Instance);

        // Un periodo negativo o enorme non deve generare serie assurde.
        var minimo = await servizio.GetMetricheUsoAsync(0);
        var massimo = await servizio.GetMetricheUsoAsync(10_000);

        Assert.Equal(1, minimo.GiorniPeriodo);
        Assert.Equal(365, massimo.GiorniPeriodo);
        Assert.Equal(365, massimo.SerieVisualizzazioni.Count);
    }

    [Fact]
    public async Task VisualizzazioniTotali_sommanoAncheOltreIlPeriodo()
    {
        using var db = TestDbContextFactory.Crea();
        db.Libri.Add(new Libro { Id = LibroId, Titolo = "T", Autore = "A" });
        db.VisualizzazioniLibro.AddRange(
            new VisualizzazioneLibro { LibroId = LibroId, Giorno = DateOnly.FromDateTime(DateTime.UtcNow), Conteggio = 3 },
            new VisualizzazioneLibro { LibroId = LibroId, Giorno = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-100)), Conteggio = 7 });
        await db.SaveChangesAsync();

        var servizio = new MetricheService(db, NullLogger<MetricheService>.Instance);
        var metriche = await servizio.GetMetricheUsoAsync(7);

        // Il totale e' storico, il periodo no: sono due numeri diversi e la
        // dashboard li mostra affiancati.
        Assert.Equal(10, metriche.VisualizzazioniTotali);
        Assert.Equal(3, metriche.VisualizzazioniPeriodo);
    }

    [Fact]
    public async Task PrestitiPerStato_elencaTuttiGliStatiAncheAZero()
    {
        using var db = TestDbContextFactory.Crea();
        var servizio = new MetricheService(db, NullLogger<MetricheService>.Instance);

        var metriche = await servizio.GetMetricheUsoAsync(30);

        // La legenda del grafico deve restare stabile fra un caricamento e
        // l'altro, anche su un database senza prestiti.
        Assert.Equal(Enum.GetValues<StatoPrestito>().Length, metriche.PrestitiPerStato.Count);
        Assert.All(metriche.PrestitiPerStato, voce => Assert.Equal(0, voce.Valore));
    }

    [Fact]
    public async Task LibriPiuVisti_ordinatiPerConteggioDecrescente()
    {
        using var db = TestDbContextFactory.Crea();
        var meno = Guid.NewGuid();
        var piu = Guid.NewGuid();
        db.Libri.AddRange(
            new Libro { Id = meno, Titolo = "Meno visto", Autore = "A" },
            new Libro { Id = piu, Titolo = "Piu' visto", Autore = "A" });
        db.VisualizzazioniLibro.AddRange(
            new VisualizzazioneLibro { LibroId = meno, Giorno = DateOnly.FromDateTime(DateTime.UtcNow), Conteggio = 2 },
            new VisualizzazioneLibro { LibroId = piu, Giorno = DateOnly.FromDateTime(DateTime.UtcNow), Conteggio = 9 });
        await db.SaveChangesAsync();

        var servizio = new MetricheService(db, NullLogger<MetricheService>.Instance);
        var metriche = await servizio.GetMetricheUsoAsync(30);

        Assert.Equal("Piu' visto", metriche.LibriPiuVisti.First().Etichetta);
        Assert.Equal(9, metriche.LibriPiuVisti.First().Valore);
    }
}
