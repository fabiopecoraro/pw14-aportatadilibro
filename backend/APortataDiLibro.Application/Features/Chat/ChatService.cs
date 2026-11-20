using APortataDiLibro.Application.Features.Chat.Dtos;
using APortataDiLibro.Core.Exceptions;
using APortataDiLibro.Database.Context;
using APortataDiLibro.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace APortataDiLibro.Application.Features.Chat;

/// <summary>
/// Gestisce la chat privata associata a un prestito. Per design la chat e'
/// disponibile fin dal momento della richiesta (stato <c>Richiesto</c>):
/// permette al richiedente e al proprietario della copia di accordarsi
/// ancora prima che la richiesta venga accettata.
/// </summary>
public class ChatService : IChatService
{
    private readonly ApplicationDbContext _dbContext;

    public ChatService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> UtenteAutorizzatoAsync(Guid prestitoId, Guid utenteId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Prestiti
            .AsNoTracking()
            .AnyAsync(p => p.Id == prestitoId
                && (p.RichiedenteId == utenteId || p.CopiaLibro.ProprietarioId == utenteId), cancellationToken);
    }

    public async Task<IReadOnlyList<ChatMessaggioDto>> GetCronologiaAsync(Guid prestitoId, Guid utenteId, CancellationToken cancellationToken = default)
    {
        if (!await UtenteAutorizzatoAsync(prestitoId, utenteId, cancellationToken))
        {
            throw new ForbiddenException("Non sei un partecipante alla chat di questo prestito.");
        }

        return await _dbContext.MessaggiChat
            .AsNoTracking()
            .Include(m => m.Mittente)
            .Where(m => m.PrestitoId == prestitoId)
            .OrderBy(m => m.DataInvio)
            .Select(m => new ChatMessaggioDto(m.Id, m.PrestitoId, m.MittenteId, m.Mittente.Nickname, m.Testo, m.DataInvio))
            .ToListAsync(cancellationToken);
    }

    public async Task<ChatMessaggioDto> InviaMessaggioAsync(Guid prestitoId, Guid mittenteId, string testo, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(testo))
        {
            throw new ValidationException("Il messaggio non può essere vuoto.");
        }

        var prestito = await _dbContext.Prestiti
            .AsNoTracking()
            .SingleOrDefaultAsync(p => p.Id == prestitoId, cancellationToken)
            ?? throw new NotFoundException(nameof(Prestito), prestitoId);

        if (!await UtenteAutorizzatoAsync(prestitoId, mittenteId, cancellationToken))
        {
            throw new ForbiddenException("Non sei un partecipante alla chat di questo prestito.");
        }

        var mittente = await _dbContext.Utenti.SingleOrDefaultAsync(u => u.Id == mittenteId, cancellationToken)
            ?? throw new NotFoundException(nameof(Utente), mittenteId);

        var messaggio = new ChatMessaggio
        {
            Id = Guid.NewGuid(),
            PrestitoId = prestitoId,
            MittenteId = mittenteId,
            Testo = testo.Trim(),
            DataInvio = DateTime.UtcNow
        };

        _dbContext.MessaggiChat.Add(messaggio);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ChatMessaggioDto(messaggio.Id, messaggio.PrestitoId, messaggio.MittenteId, mittente.Nickname, messaggio.Testo, messaggio.DataInvio);
    }
}
