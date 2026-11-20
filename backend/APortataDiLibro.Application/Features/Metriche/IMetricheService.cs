using APortataDiLibro.Application.Features.Metriche.Dtos;

namespace APortataDiLibro.Application.Features.Metriche;

/// <summary>
/// Raccolta e lettura delle metriche d'uso richieste dalla traccia
/// ("visualizzazioni, richieste di prestito").
/// </summary>
public interface IMetricheService
{
    /// <summary>
    /// Incrementa di uno il contatore di visualizzazioni del libro per il
    /// giorno corrente. Non deve mai far fallire la lettura della scheda.
    /// </summary>
    Task RegistraVisualizzazioneAsync(Guid libroId, CancellationToken cancellationToken = default);

    /// <summary>Numero complessivo di visualizzazioni di un singolo libro.</summary>
    Task<int> GetVisualizzazioniLibroAsync(Guid libroId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Metriche aggregate per la dashboard Admin sugli ultimi
    /// <paramref name="giorni"/> giorni.
    /// </summary>
    Task<MetricheUsoDto> GetMetricheUsoAsync(int giorni = 30, CancellationToken cancellationToken = default);
}
