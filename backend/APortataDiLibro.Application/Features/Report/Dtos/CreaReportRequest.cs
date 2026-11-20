namespace APortataDiLibro.Application.Features.Report.Dtos;

/// <summary>Segnalazione di un comportamento scorretto nella chat di un prestito.</summary>
public record CreaReportRequest(Guid PrestitoId, Guid UtenteSegnalatoId, string Motivo);
