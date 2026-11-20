using APortataDiLibro.Application.Features.Admin.Dtos;

namespace APortataDiLibro.Application.Features.Admin;

public interface IAdminService
{
    /// <summary>Statistiche aggregate per la dashboard Admin.</summary>
    Task<DashboardStatisticheDto> GetStatisticheAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Genera il report CSV dell'intero catalogo: dettagli del libro, media voti
    /// recensioni, copie totali, disponibili e in prestito.
    /// </summary>
    Task<byte[]> EsportaCatalogoCsvAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Genera il CSV della serie temporale delle metriche d'uso degli ultimi
    /// <paramref name="giorni"/> giorni: una riga per giorno con
    /// visualizzazioni e richieste di prestito.
    /// <para>
    /// Gli aggregati della dashboard (totali, libri piu' visti, prestiti per
    /// stato) restano fuori: un CSV regge una sola tabella, e i totali si
    /// riottengono sommando le colonne.
    /// </para>
    /// </summary>
    Task<EsportazioneCsvDto> EsportaMetricheUsoCsvAsync(int giorni = 30, CancellationToken cancellationToken = default);
}
