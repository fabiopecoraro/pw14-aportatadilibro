using APortataDiLibro.Application.Features.Notifiche.Dtos;
using APortataDiLibro.Core.Enums;
using APortataDiLibro.Core.Exceptions;
using APortataDiLibro.Database.Context;
using APortataDiLibro.Database.Entities;
using APortataDiLibro.Infrastructure.Notifications;
using Microsoft.EntityFrameworkCore;

namespace APortataDiLibro.Application.Features.Notifiche;

/// <summary>
/// Gestisce le iscrizioni "avvisami quando torna disponibile" per i libri
/// senza copie libere e l'invio (simulato) delle relative notifiche email.
/// </summary>
public class SottoscrizioniService : ISottoscrizioniService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IEmailNotificationSender _emailSender;

    public SottoscrizioniService(ApplicationDbContext dbContext, IEmailNotificationSender emailSender)
    {
        _dbContext = dbContext;
        _emailSender = emailSender;
    }

    public async Task<SottoscrizioneDisponibilitaDto> SottoscriviAsync(Guid utenteId, Guid libroId, CancellationToken cancellationToken = default)
    {
        var libro = await _dbContext.Libri
            .Include(l => l.Copie)
            .SingleOrDefaultAsync(l => l.Id == libroId, cancellationToken)
            ?? throw new NotFoundException(nameof(Libro), libroId);

        if (libro.Copie.Any(c => c.Stato == StatoCopia.Disponibile))
        {
            throw new ConflictException("Il libro ha già copie disponibili: la sottoscrizione non è necessaria.");
        }

        var esistente = await _dbContext.SottoscrizioniDisponibilita
            .SingleOrDefaultAsync(s => s.LibroId == libroId && s.UtenteId == utenteId, cancellationToken);

        if (esistente is not null)
        {
            if (!esistente.Notificata)
            {
                throw new ConflictException("Sei già iscritto alle notifiche di disponibilità per questo libro.");
            }

            esistente.Notificata = false;
            esistente.DataRichiesta = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new SottoscrizioneDisponibilitaDto(esistente.Id, libroId, libro.Titolo, esistente.DataRichiesta);
        }

        var sottoscrizione = new SottoscrizioneDisponibilita
        {
            Id = Guid.NewGuid(),
            LibroId = libroId,
            UtenteId = utenteId,
            DataRichiesta = DateTime.UtcNow,
            Notificata = false
        };

        _dbContext.SottoscrizioniDisponibilita.Add(sottoscrizione);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new SottoscrizioneDisponibilitaDto(sottoscrizione.Id, libroId, libro.Titolo, sottoscrizione.DataRichiesta);
    }

    public async Task AnnullaSottoscrizioneAsync(Guid utenteId, Guid libroId, CancellationToken cancellationToken = default)
    {
        var sottoscrizione = await _dbContext.SottoscrizioniDisponibilita
            .SingleOrDefaultAsync(s => s.LibroId == libroId && s.UtenteId == utenteId && !s.Notificata, cancellationToken)
            ?? throw new NotFoundException("Nessuna sottoscrizione attiva trovata per questo libro.");

        _dbContext.SottoscrizioniDisponibilita.Remove(sottoscrizione);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SottoscrizioneDisponibilitaDto>> GetSottoscrizioniUtenteAsync(Guid utenteId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.SottoscrizioniDisponibilita
            .AsNoTracking()
            .Include(s => s.Libro)
            .Where(s => s.UtenteId == utenteId && !s.Notificata)
            .OrderByDescending(s => s.DataRichiesta)
            .Select(s => new SottoscrizioneDisponibilitaDto(s.Id, s.LibroId, s.Libro.Titolo, s.DataRichiesta))
            .ToListAsync(cancellationToken);
    }

    public async Task NotificaDisponibilitaAsync(Guid libroId, CancellationToken cancellationToken = default)
    {
        var sottoscrizioni = await _dbContext.SottoscrizioniDisponibilita
            .Include(s => s.Utente)
            .Include(s => s.Libro)
            .Where(s => s.LibroId == libroId && !s.Notificata)
            .ToListAsync(cancellationToken);

        if (sottoscrizioni.Count == 0)
        {
            return;
        }

        foreach (var sottoscrizione in sottoscrizioni)
        {
            await _emailSender.InviaAsync(
                sottoscrizione.Utente.Email,
                $"\"{sottoscrizione.Libro.Titolo}\" è di nuovo disponibile",
                $"Ciao {sottoscrizione.Utente.Nickname}, una copia di \"{sottoscrizione.Libro.Titolo}\" è tornata disponibile su A portata di libro.",
                cancellationToken);

            sottoscrizione.Notificata = true;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
