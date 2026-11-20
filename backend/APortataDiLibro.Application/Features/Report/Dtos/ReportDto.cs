using APortataDiLibro.Core.Enums;

namespace APortataDiLibro.Application.Features.Report.Dtos;

public record ReportDto(
    Guid Id,
    Guid PrestitoId,
    string SegnalanteNickname,
    string UtenteSegnalatoNickname,
    string Motivo,
    StatoReport Stato,
    DateTime DataCreazione);
