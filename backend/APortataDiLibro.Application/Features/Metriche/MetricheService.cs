using APortataDiLibro.Application.Features.Metriche.Dtos;
using APortataDiLibro.Core.Enums;
using APortataDiLibro.Database.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace APortataDiLibro.Application.Features.Metriche;

/// <summary>
/// Raccoglie e aggrega le metriche d'uso della piattaforma.
/// <para>
/// Le due grandezze tracciate sono quelle indicate dalla traccia e realmente
/// presenti nel dominio: le aperture della scheda di un libro e le richieste
/// di prestito. Il terzo esempio della traccia, i "download", non ha un
/// corrispondente qui — la piattaforma non distribuisce contenuti, i libri
/// sono oggetti fisici — e non viene simulato.
/// </para>
/// </summary>
public class MetricheService : IMetricheService
{
    private const int GiorniMassimiPeriodo = 365;
    private const int NumeroLibriPiuVisti = 5;

    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<MetricheService> _logger;

    public MetricheService(ApplicationDbContext dbContext, ILogger<MetricheService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task RegistraVisualizzazioneAsync(Guid libroId, CancellationToken cancellationToken = default)
    {
        var giorno = DateOnly.FromDateTime(DateTime.UtcNow);

        try
        {
            // Upsert atomico invece di leggi-modifica-scrivi: due lettori
            // simultanei della stessa scheda incrementerebbero altrimenti lo
            // stesso valore letto, perdendo silenziosamente una visualizzazione.
            // E' l'unico punto del layer Application che ricorre a SQL nativo,
            // perche' EF non espone ON CONFLICT ... DO UPDATE.
            await _dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"""
                 INSERT INTO "VisualizzazioniLibro" ("LibroId", "Giorno", "Conteggio")
                 VALUES ({libroId}, {giorno}, 1)
                 ON CONFLICT ("LibroId", "Giorno")
                 DO UPDATE SET "Conteggio" = "VisualizzazioniLibro"."Conteggio" + 1
                 """,
                cancellationToken);
        }
        catch (Exception ex)
        {
            // Una metrica non e' mai un motivo valido per negare all'utente la
            // scheda del libro che ha chiesto.
            _logger.LogWarning(ex, "Registrazione della visualizzazione del libro {LibroId} non riuscita.", libroId);
        }
    }

    public Task<int> GetVisualizzazioniLibroAsync(Guid libroId, CancellationToken cancellationToken = default)
    {
        return _dbContext.VisualizzazioniLibro
            .AsNoTracking()
            .Where(v => v.LibroId == libroId)
            .SumAsync(v => v.Conteggio, cancellationToken);
    }

    public async Task<MetricheUsoDto> GetMetricheUsoAsync(int giorni = 30, CancellationToken cancellationToken = default)
    {
        var giorniPeriodo = Math.Clamp(giorni, 1, GiorniMassimiPeriodo);

        var oggi = DateOnly.FromDateTime(DateTime.UtcNow);
        var primoGiorno = oggi.AddDays(-(giorniPeriodo - 1));
        var inizioPeriodo = primoGiorno.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        var visualizzazioniTotali = await _dbContext.VisualizzazioniLibro
            .AsNoTracking()
            .SumAsync(v => v.Conteggio, cancellationToken);

        var visualizzazioniPerGiorno = await _dbContext.VisualizzazioniLibro
            .AsNoTracking()
            .Where(v => v.Giorno >= primoGiorno)
            .GroupBy(v => v.Giorno)
            .Select(g => new { Giorno = g.Key, Valore = g.Sum(v => v.Conteggio) })
            .ToDictionaryAsync(x => x.Giorno, x => x.Valore, cancellationToken);

        var richiestePrestitoTotali = await _dbContext.Prestiti.CountAsync(cancellationToken);

        var richiestePerGiorno = await _dbContext.Prestiti
            .AsNoTracking()
            .Where(p => p.DataRichiesta >= inizioPeriodo)
            .GroupBy(p => DateOnly.FromDateTime(p.DataRichiesta))
            .Select(g => new { Giorno = g.Key, Valore = g.Count() })
            .ToDictionaryAsync(x => x.Giorno, x => x.Valore, cancellationToken);

        var libriPiuVisti = await _dbContext.VisualizzazioniLibro
            .AsNoTracking()
            .GroupBy(v => v.LibroId)
            .Select(g => new { LibroId = g.Key, Valore = g.Sum(v => v.Conteggio) })
            .OrderByDescending(x => x.Valore)
            .Take(NumeroLibriPiuVisti)
            .Join(
                _dbContext.Libri.AsNoTracking(),
                x => x.LibroId,
                l => l.Id,
                (x, l) => new ContoEtichettaDto(l.Titolo, x.Valore))
            .ToListAsync(cancellationToken);

        var prestitiPerStato = await _dbContext.Prestiti
            .AsNoTracking()
            .GroupBy(p => p.Stato)
            .Select(g => new { Stato = g.Key, Valore = g.Count() })
            .ToDictionaryAsync(x => x.Stato, x => x.Valore, cancellationToken);

        // Le serie vanno riempite anche nei giorni senza eventi: un grafico che
        // salta i giorni vuoti comprime l'asse del tempo e mostra un andamento
        // che non e' quello reale.
        var serieVisualizzazioni = CostruisciSerie(primoGiorno, giorniPeriodo, visualizzazioniPerGiorno);
        var serieRichieste = CostruisciSerie(primoGiorno, giorniPeriodo, richiestePerGiorno);

        return new MetricheUsoDto(
            visualizzazioniTotali,
            serieVisualizzazioni.Sum(p => p.Valore),
            richiestePrestitoTotali,
            serieRichieste.Sum(p => p.Valore),
            giorniPeriodo,
            serieVisualizzazioni,
            serieRichieste,
            libriPiuVisti,
            // Tutti gli stati compaiono sempre, anche a zero: la legenda del
            // grafico resta stabile fra un caricamento e l'altro.
            Enum.GetValues<StatoPrestito>()
                .Select(stato => new ContoEtichettaDto(stato.ToString(), prestitiPerStato.GetValueOrDefault(stato)))
                .ToList());
    }

    private static List<PuntoSerieDto> CostruisciSerie(
        DateOnly primoGiorno,
        int giorni,
        IReadOnlyDictionary<DateOnly, int> valoriPerGiorno)
    {
        var serie = new List<PuntoSerieDto>(giorni);

        for (var offset = 0; offset < giorni; offset++)
        {
            var giorno = primoGiorno.AddDays(offset);
            serie.Add(new PuntoSerieDto(giorno, valoriPerGiorno.GetValueOrDefault(giorno)));
        }

        return serie;
    }
}
