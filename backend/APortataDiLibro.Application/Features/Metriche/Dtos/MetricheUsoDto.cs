namespace APortataDiLibro.Application.Features.Metriche.Dtos;

/// <summary>Un punto di una serie temporale giornaliera.</summary>
public record PuntoSerieDto(DateOnly Giorno, int Valore);

/// <summary>Una voce di un conteggio per categoria (barra o spicchio).</summary>
public record ContoEtichettaDto(string Etichetta, int Valore);

/// <summary>
/// Metriche d'uso della piattaforma mostrate nel modulo statistiche della
/// dashboard Admin (visualizzazioni delle schede libro e richieste di
/// prestito), con i totali e le serie giornaliere che alimentano i grafici.
/// </summary>
public record MetricheUsoDto(
    int VisualizzazioniTotali,
    int VisualizzazioniPeriodo,
    int RichiestePrestitoTotali,
    int RichiestePrestitoPeriodo,
    int GiorniPeriodo,
    IReadOnlyList<PuntoSerieDto> SerieVisualizzazioni,
    IReadOnlyList<PuntoSerieDto> SerieRichiestePrestito,
    IReadOnlyList<ContoEtichettaDto> LibriPiuVisti,
    IReadOnlyList<ContoEtichettaDto> PrestitiPerStato);
