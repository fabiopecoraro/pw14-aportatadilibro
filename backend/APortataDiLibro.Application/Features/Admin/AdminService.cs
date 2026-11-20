using System.Globalization;
using System.Text;
using APortataDiLibro.Application.Features.Admin.Dtos;
using APortataDiLibro.Application.Features.Metriche;
using APortataDiLibro.Application.Features.Metriche.Dtos;
using APortataDiLibro.Core.Enums;
using APortataDiLibro.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace APortataDiLibro.Application.Features.Admin;

/// <summary>
/// Fornisce le statistiche aggregate e gli export CSV (catalogo e metriche
/// d'uso) per l'area analitica riservata all'Admin.
/// </summary>
public class AdminService : IAdminService
{
    private readonly ApplicationDbContext _dbContext;

    // L'export delle metriche passa dal servizio che alimenta la dashboard
    // invece di interrogare da se' le visualizzazioni: il file scaricato deve
    // contenere gli stessi numeri che l'Admin vede a schermo, e la stessa
    // aggregazione scritta due volte divergerebbe alla prima modifica.
    private readonly IMetricheService _metricheService;

    public AdminService(ApplicationDbContext dbContext, IMetricheService metricheService)
    {
        _dbContext = dbContext;
        _metricheService = metricheService;
    }

    public async Task<DashboardStatisticheDto> GetStatisticheAsync(CancellationToken cancellationToken = default)
    {
        var totaleLibri = await _dbContext.Libri.CountAsync(cancellationToken);
        var totaleCopie = await _dbContext.CopieLibro.CountAsync(cancellationToken);
        var copieDisponibili = await _dbContext.CopieLibro.CountAsync(c => c.Stato == StatoCopia.Disponibile, cancellationToken);
        var copieInPrestito = await _dbContext.CopieLibro.CountAsync(c => c.Stato == StatoCopia.InPrestito, cancellationToken);
        var reportAperti = await _dbContext.Report.CountAsync(r => r.Stato == StatoReport.Aperto, cancellationToken);

        var prestitiCompletati = await _dbContext.Prestiti
            .Where(p => p.Stato == StatoPrestito.Completato && p.DataRestituzioneEffettiva != null)
            .Select(p => new { p.DataRichiesta, DataRestituzione = p.DataRestituzioneEffettiva!.Value })
            .ToListAsync(cancellationToken);

        double? durataMedia = prestitiCompletati.Count > 0
            ? prestitiCompletati.Average(p => (p.DataRestituzione - p.DataRichiesta).TotalDays)
            : null;

        return new DashboardStatisticheDto(totaleLibri, totaleCopie, copieDisponibili, copieInPrestito, durataMedia, reportAperti);
    }

    public async Task<byte[]> EsportaCatalogoCsvAsync(CancellationToken cancellationToken = default)
    {
        var libri = await _dbContext.Libri
            .AsNoTracking()
            .OrderBy(l => l.Titolo)
            .Select(l => new
            {
                l.Id,
                l.Titolo,
                l.Autore,
                Categorie = l.Categorie.OrderBy(lc => lc.Categoria.Nome).Select(lc => lc.Categoria.Nome).ToList(),
                l.Anno,
                l.Isbn,
                CopieTotali = l.Copie.Count,
                CopieDisponibili = l.Copie.Count(c => c.Stato == StatoCopia.Disponibile),
                CopieInPrestito = l.Copie.Count(c => c.Stato == StatoCopia.InPrestito)
            })
            .ToListAsync(cancellationToken);

        var medieVoti = await _dbContext.RecensioniLibro
            .GroupBy(r => r.LibroId)
            .Select(g => new { LibroId = g.Key, Media = g.Average(r => r.Voto) })
            .ToDictionaryAsync(x => x.LibroId, x => x.Media, cancellationToken);

        var csv = new StringBuilder();
        csv.AppendLine("Id,Titolo,Autore,Categorie,Anno,Isbn,MediaVotiRecensioni,CopieTotali,CopieDisponibili,CopieInPrestito");

        foreach (var libro in libri)
        {
            var mediaVoti = medieVoti.TryGetValue(libro.Id, out var media)
                ? media.ToString("F2", CultureInfo.InvariantCulture)
                : string.Empty;

            csv.AppendLine(string.Join(',',
                EscapeCsv(libro.Id.ToString()),
                EscapeCsv(libro.Titolo),
                EscapeCsv(libro.Autore),
                // Le categorie stanno in una sola cella separate da ";": la virgola
                // e' gia' il separatore di campo del file, e EscapeCsv mette comunque
                // il valore fra virgolette.
                EscapeCsv(string.Join("; ", libro.Categorie)),
                libro.Anno?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
                EscapeCsv(libro.Isbn ?? string.Empty),
                mediaVoti,
                libro.CopieTotali.ToString(CultureInfo.InvariantCulture),
                libro.CopieDisponibili.ToString(CultureInfo.InvariantCulture),
                libro.CopieInPrestito.ToString(CultureInfo.InvariantCulture)));
        }

        // BOM UTF-8 per la corretta apertura dei caratteri accentati in Excel.
        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray();
    }

    public async Task<EsportazioneCsvDto> EsportaMetricheUsoCsvAsync(int giorni = 30, CancellationToken cancellationToken = default)
    {
        var metriche = await _metricheService.GetMetricheUsoAsync(giorni, cancellationToken);

        var csv = new StringBuilder();
        csv.AppendLine("Giorno,Visualizzazioni,RichiestePrestito");

        // Nessun passaggio per EscapeCsv come nell'export del catalogo: qui i
        // tre campi sono una data ISO e due interi in cultura invariante, e
        // nessuno dei tre puo' contenere il separatore. La data e' ISO e non
        // locale perche' il file e' dato da rielaborare, non da leggere:
        // "03/04" e' ambiguo, "2026-04-03" no.
        foreach (var riga in UnisciSeriePerGiorno(metriche.SerieVisualizzazioni, metriche.SerieRichiestePrestito))
        {
            csv.AppendLine(string.Join(',',
                riga.Giorno.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                riga.Visualizzazioni.ToString(CultureInfo.InvariantCulture),
                riga.RichiestePrestito.ToString(CultureInfo.InvariantCulture)));
        }

        // Separatore, codifica e BOM sono quelli dell'export del catalogo: due
        // file della stessa area che si aprono in modo diverso nello stesso
        // foglio di calcolo sarebbero un difetto, non una variante.
        var contenuto = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray();

        return new EsportazioneCsvDto($"metriche-{metriche.GiorniPeriodo}-giorni.csv", contenuto);
    }

    /// <summary>
    /// Allinea le due serie giornaliere in righe (giorno, visualizzazioni,
    /// richieste) ordinate dal giorno piu' vecchio al piu' recente.
    /// </summary>
    private static IEnumerable<(DateOnly Giorno, int Visualizzazioni, int RichiestePrestito)> UnisciSeriePerGiorno(
        IReadOnlyList<PuntoSerieDto> serieVisualizzazioni,
        IReadOnlyList<PuntoSerieDto> serieRichiestePrestito)
    {
        // Unione per giorno e non accoppiamento per posizione: le due serie
        // nascono da query indipendenti e oggi coprono lo stesso intervallo, ma
        // basterebbe un filtro diverso su una delle due per sfasare le righe e
        // attribuire le richieste al giorno sbagliato. Un giorno presente in
        // una sola serie esce comunque, con 0 nell'altra colonna: una riga
        // mancante in un CSV si legge come "quel giorno non e' mai esistito".
        // SortedDictionary e non Dictionary: l'ordine cronologico e' parte del
        // contenuto del file, non una comodita' da aggiungere dopo.
        var righe = new SortedDictionary<DateOnly, (int Visualizzazioni, int RichiestePrestito)>();

        foreach (var punto in serieVisualizzazioni)
        {
            var riga = righe.GetValueOrDefault(punto.Giorno);
            // Si somma invece di sovrascrivere: se una serie arrivasse con due
            // punti sullo stesso giorno il file resterebbe a una riga per
            // giorno, che e' il suo contratto, senza perdere un valore.
            righe[punto.Giorno] = (riga.Visualizzazioni + punto.Valore, riga.RichiestePrestito);
        }

        foreach (var punto in serieRichiestePrestito)
        {
            var riga = righe.GetValueOrDefault(punto.Giorno);
            righe[punto.Giorno] = (riga.Visualizzazioni, riga.RichiestePrestito + punto.Valore);
        }

        return righe.Select(r => (r.Key, r.Value.Visualizzazioni, r.Value.RichiestePrestito));
    }

    private static string EscapeCsv(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}
