using APortataDiLibro.Application.Features.Chat.Dtos;

namespace APortataDiLibro.Application.Features.Report.Dtos;

/// <summary>
/// Dettaglio di un report ad uso Admin, comprensivo della cronologia della
/// chat del prestito segnalato. Disponibile solo per i report ancora
/// <see cref="Core.Enums.StatoReport.Aperto"/>.
/// </summary>
public record ReportDettaglioDto(ReportDto Report, IReadOnlyList<ChatMessaggioDto> Messaggi);
