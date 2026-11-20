using APortataDiLibro.Application.Features.Report.Dtos;
using APortataDiLibro.Core.Enums;

namespace APortataDiLibro.Application.Features.Report;

public interface IReportService
{
    /// <summary>Crea una segnalazione su un partecipante alla chat di un proprio prestito.</summary>
    Task<ReportDto> CreaReportAsync(Guid segnalanteId, CreaReportRequest request, CancellationToken cancellationToken = default);

    /// <summary>Elenco dei report, opzionalmente filtrato per stato. Riservato all'Admin.</summary>
    Task<IReadOnlyList<ReportDto>> GetReportsAsync(StatoReport? stato, CancellationToken cancellationToken = default);

    /// <summary>
    /// Dettaglio di un report con la cronologia della chat associata.
    /// Riservato all'Admin e disponibile solo per i report ancora <see cref="StatoReport.Aperto"/>.
    /// </summary>
    Task<ReportDettaglioDto> GetDettaglioReportAsync(Guid reportId, CancellationToken cancellationToken = default);

    /// <summary>Chiude un report dopo la moderazione. Riservato all'Admin.</summary>
    Task<ReportDto> ChiudiReportAsync(Guid reportId, CancellationToken cancellationToken = default);
}
