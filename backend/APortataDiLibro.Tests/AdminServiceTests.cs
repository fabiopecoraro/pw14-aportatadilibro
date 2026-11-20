using System.Text;
using APortataDiLibro.Application.Features.Admin;
using APortataDiLibro.Application.Features.Metriche;
using APortataDiLibro.Application.Features.Metriche.Dtos;

namespace APortataDiLibro.Tests;

/// <summary>
/// Esportazione CSV delle metriche d'uso. Il comportamento che conta e'
/// l'unione delle due serie giornaliere: un giorno presente in una sola delle
/// due deve comparire comunque, con 0 nell'altra colonna, perche' una riga
/// mancante in un CSV si legge come un giorno mai esistito.
/// <para>
/// Le metriche arrivano da un doppio di <see cref="IMetricheService"/> e non
/// dal database: il caso delle serie disallineate non e' producibile dal
/// servizio reale (riempie sempre entrambe le serie su tutto il periodo), e
/// il provider InMemory non eseguirebbe comunque l'upsert in SQL nativo del
/// contatore di visualizzazioni.
/// </para>
/// </summary>
public class AdminServiceTests
{
    private static readonly DateOnly Lunedi = new(2026, 4, 6);

    [Fact]
    public async Task CsvMetriche_unisceLeSerieEAzzeraIGiorniPresentiInUnaSola()
    {
        var metriche = CreaMetriche(
            giorniPeriodo: 3,
            serieVisualizzazioni:
            [
                new PuntoSerieDto(Lunedi, 4),
                new PuntoSerieDto(Lunedi.AddDays(2), 7)
            ],
            serieRichiestePrestito:
            [
                new PuntoSerieDto(Lunedi.AddDays(1), 2)
            ]);

        var righe = await EsportaRighe(metriche, giorni: 3);

        Assert.Equal("Giorno,Visualizzazioni,RichiestePrestito", righe[0]);
        // Tre giorni distinti nelle due serie, tre righe dopo l'intestazione, in
        // ordine cronologico e con lo zero dove la serie taceva.
        Assert.Equal(
            ["2026-04-06,4,0", "2026-04-07,0,2", "2026-04-08,7,0"],
            righe.Skip(1));
    }

    [Fact]
    public async Task CsvMetriche_portaNelNomeIlPeriodoEffettivamenteEsportato()
    {
        // Il servizio delle metriche riporta il periodo chiesto nei limiti
        // ammessi: il nome del file deve dire quanti giorni ci sono dentro, non
        // quanti ne sono stati chiesti.
        var metriche = CreaMetriche(giorniPeriodo: 365, serieVisualizzazioni: [], serieRichiestePrestito: []);
        var servizio = CreaServizio(metriche);

        var esportazione = await servizio.EsportaMetricheUsoCsvAsync(10_000);

        Assert.Equal("metriche-365-giorni.csv", esportazione.NomeFile);
        // Stesso BOM UTF-8 dell'export del catalogo: senza, Excel apre gli
        // accenti sbagliati e i due file dell'area Admin si comporterebbero in
        // modo diverso.
        Assert.Equal(Encoding.UTF8.GetPreamble(), esportazione.Contenuto.Take(3));
    }

    [Fact]
    public async Task CsvMetriche_nonContieneGliAggregatiDellaDashboard()
    {
        var metriche = CreaMetriche(
            giorniPeriodo: 1,
            serieVisualizzazioni: [new PuntoSerieDto(Lunedi, 1)],
            serieRichiestePrestito: [new PuntoSerieDto(Lunedi, 1)],
            libriPiuVisti: [new ContoEtichettaDto("Il nome della rosa", 9)]);

        var righe = await EsportaRighe(metriche, giorni: 1);

        // Un CSV regge una sola tabella: titoli dei libri piu' visti e stati
        // dei prestiti hanno una forma diversa dalla serie e la romperebbero.
        Assert.Equal(2, righe.Count);
        Assert.DoesNotContain(righe, riga => riga.Contains("rosa"));
    }

    private static async Task<List<string>> EsportaRighe(MetricheUsoDto metriche, int giorni)
    {
        var servizio = CreaServizio(metriche);
        var esportazione = await servizio.EsportaMetricheUsoCsvAsync(giorni);

        var testo = Encoding.UTF8.GetString(esportazione.Contenuto.Skip(Encoding.UTF8.GetPreamble().Length).ToArray());

        return [.. testo.Split('\n', '\r').Where(riga => riga.Length > 0)];
    }

    private static AdminService CreaServizio(MetricheUsoDto metriche)
    {
        // Il contesto serve solo al costruttore: l'export delle metriche non
        // interroga il database, i numeri vengono dal servizio delle metriche.
        return new AdminService(TestDbContextFactory.Crea(), new MetricheServiceDoppio(metriche));
    }

    private static MetricheUsoDto CreaMetriche(
        int giorniPeriodo,
        IReadOnlyList<PuntoSerieDto> serieVisualizzazioni,
        IReadOnlyList<PuntoSerieDto> serieRichiestePrestito,
        IReadOnlyList<ContoEtichettaDto>? libriPiuVisti = null)
    {
        return new MetricheUsoDto(
            VisualizzazioniTotali: serieVisualizzazioni.Sum(p => p.Valore),
            VisualizzazioniPeriodo: serieVisualizzazioni.Sum(p => p.Valore),
            RichiestePrestitoTotali: serieRichiestePrestito.Sum(p => p.Valore),
            RichiestePrestitoPeriodo: serieRichiestePrestito.Sum(p => p.Valore),
            GiorniPeriodo: giorniPeriodo,
            SerieVisualizzazioni: serieVisualizzazioni,
            SerieRichiestePrestito: serieRichiestePrestito,
            LibriPiuVisti: libriPiuVisti ?? [],
            PrestitiPerStato: []);
    }

    private class MetricheServiceDoppio : IMetricheService
    {
        private readonly MetricheUsoDto _metriche;

        public MetricheServiceDoppio(MetricheUsoDto metriche)
        {
            _metriche = metriche;
        }

        public Task RegistraVisualizzazioneAsync(Guid libroId, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<int> GetVisualizzazioniLibroAsync(Guid libroId, CancellationToken cancellationToken = default)
            => Task.FromResult(0);

        public Task<MetricheUsoDto> GetMetricheUsoAsync(int giorni = 30, CancellationToken cancellationToken = default)
            => Task.FromResult(_metriche);
    }
}
