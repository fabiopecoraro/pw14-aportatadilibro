using APortataDiLibro.Application.Features.Chat.Dtos;
using APortataDiLibro.Application.Features.Report.Dtos;
using APortataDiLibro.Core.Enums;
using APortataDiLibro.Core.Exceptions;
using APortataDiLibro.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace APortataDiLibro.Application.Features.Report;

/// <summary>
/// Gestisce le segnalazioni di comportamento scorretto in chat e la
/// moderazione Admin. Per regola di privacy, la cronologia di una chat e'
/// ispezionabile dall'Admin solo finche' il relativo report e' Aperto.
/// </summary>
public class ReportService : IReportService
{
    private readonly ApplicationDbContext _dbContext;

    public ReportService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ReportDto> CreaReportAsync(Guid segnalanteId, CreaReportRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Motivo))
        {
            throw new ValidationException("Il motivo della segnalazione è obbligatorio.");
        }

        var prestito = await _dbContext.Prestiti
            .Include(p => p.CopiaLibro)
            .SingleOrDefaultAsync(p => p.Id == request.PrestitoId, cancellationToken)
            ?? throw new NotFoundException(nameof(Database.Entities.Prestito), request.PrestitoId);

        var proprietarioId = prestito.CopiaLibro.ProprietarioId;

        if (prestito.RichiedenteId != segnalanteId && proprietarioId != segnalanteId)
        {
            throw new ForbiddenException("Non sei un partecipante a questo prestito.");
        }

        if (request.UtenteSegnalatoId == segnalanteId)
        {
            throw new ValidationException("Non puoi segnalare te stesso.");
        }

        if (request.UtenteSegnalatoId != prestito.RichiedenteId && request.UtenteSegnalatoId != proprietarioId)
        {
            throw new ValidationException("L'utente segnalato deve essere la controparte di questo prestito.");
        }

        var segnalante = await _dbContext.Utenti.SingleOrDefaultAsync(u => u.Id == segnalanteId, cancellationToken)
            ?? throw new NotFoundException(nameof(Database.Entities.Utente), segnalanteId);

        var segnalato = await _dbContext.Utenti.SingleOrDefaultAsync(u => u.Id == request.UtenteSegnalatoId, cancellationToken)
            ?? throw new NotFoundException(nameof(Database.Entities.Utente), request.UtenteSegnalatoId);

        var report = new Database.Entities.Report
        {
            Id = Guid.NewGuid(),
            PrestitoId = request.PrestitoId,
            SegnalanteId = segnalanteId,
            UtenteSegnalatoId = request.UtenteSegnalatoId,
            Motivo = request.Motivo.Trim(),
            Stato = StatoReport.Aperto,
            DataCreazione = DateTime.UtcNow
        };

        _dbContext.Report.Add(report);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToReportDto(report, segnalante.Nickname, segnalato.Nickname);
    }

    public async Task<IReadOnlyList<ReportDto>> GetReportsAsync(StatoReport? stato, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Report
            .AsNoTracking()
            .Include(r => r.Segnalante)
            .Include(r => r.UtenteSegnalato)
            .AsQueryable();

        if (stato.HasValue)
        {
            query = query.Where(r => r.Stato == stato.Value);
        }

        return await query
            .OrderByDescending(r => r.DataCreazione)
            .Select(r => new ReportDto(r.Id, r.PrestitoId, r.Segnalante.Nickname, r.UtenteSegnalato.Nickname, r.Motivo, r.Stato, r.DataCreazione))
            .ToListAsync(cancellationToken);
    }

    public async Task<ReportDettaglioDto> GetDettaglioReportAsync(Guid reportId, CancellationToken cancellationToken = default)
    {
        var report = await _dbContext.Report
            .AsNoTracking()
            .Include(r => r.Segnalante)
            .Include(r => r.UtenteSegnalato)
            .SingleOrDefaultAsync(r => r.Id == reportId, cancellationToken)
            ?? throw new NotFoundException(nameof(Database.Entities.Report), reportId);

        if (report.Stato != StatoReport.Aperto)
        {
            throw new ForbiddenException("La cronologia della chat è ispezionabile solo per i report ancora aperti.");
        }

        var messaggi = await _dbContext.MessaggiChat
            .AsNoTracking()
            .Include(m => m.Mittente)
            .Where(m => m.PrestitoId == report.PrestitoId)
            .OrderBy(m => m.DataInvio)
            .Select(m => new ChatMessaggioDto(m.Id, m.PrestitoId, m.MittenteId, m.Mittente.Nickname, m.Testo, m.DataInvio))
            .ToListAsync(cancellationToken);

        return new ReportDettaglioDto(ToReportDto(report, report.Segnalante.Nickname, report.UtenteSegnalato.Nickname), messaggi);
    }

    public async Task<ReportDto> ChiudiReportAsync(Guid reportId, CancellationToken cancellationToken = default)
    {
        var report = await _dbContext.Report
            .Include(r => r.Segnalante)
            .Include(r => r.UtenteSegnalato)
            .SingleOrDefaultAsync(r => r.Id == reportId, cancellationToken)
            ?? throw new NotFoundException(nameof(Database.Entities.Report), reportId);

        if (report.Stato == StatoReport.Chiuso)
        {
            throw new ConflictException("Il report è già chiuso.");
        }

        report.Stato = StatoReport.Chiuso;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToReportDto(report, report.Segnalante.Nickname, report.UtenteSegnalato.Nickname);
    }

    private static ReportDto ToReportDto(Database.Entities.Report report, string segnalanteNickname, string utenteSegnalatoNickname) =>
        new(report.Id, report.PrestitoId, segnalanteNickname, utenteSegnalatoNickname, report.Motivo, report.Stato, report.DataCreazione);
}
