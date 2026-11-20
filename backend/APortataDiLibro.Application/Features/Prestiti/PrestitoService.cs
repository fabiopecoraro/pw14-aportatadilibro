using APortataDiLibro.Application.Features.Notifiche;
using APortataDiLibro.Application.Features.Prestiti.Dtos;
using APortataDiLibro.Core.Enums;
using APortataDiLibro.Core.Exceptions;
using APortataDiLibro.Database.Context;
using APortataDiLibro.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace APortataDiLibro.Application.Features.Prestiti;

/// <summary>
/// Gestisce il ciclo di vita delle richieste di prestito tra utenti, dalla
/// richiesta iniziale fino alla chiusura "Completato/Restituito" che, per
/// regola di business, e' permessa solo al proprietario della copia.
/// </summary>
public class PrestitoService : IPrestitoService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ISottoscrizioniService _sottoscrizioniService;

    public PrestitoService(ApplicationDbContext dbContext, ISottoscrizioniService sottoscrizioniService)
    {
        _dbContext = dbContext;
        _sottoscrizioniService = sottoscrizioniService;
    }

    public async Task<PrestitoDto> RichiediPrestitoAsync(Guid copiaId, Guid richiedenteId, RichiediPrestitoRequest request, CancellationToken cancellationToken = default)
    {
        if (request.GiorniStimati <= 0)
        {
            throw new ValidationException("Il numero di giorni stimati deve essere maggiore di zero.");
        }

        var copia = await _dbContext.CopieLibro
            .Include(c => c.Libro)
            .Include(c => c.Proprietario)
            .SingleOrDefaultAsync(c => c.Id == copiaId, cancellationToken)
            ?? throw new NotFoundException(nameof(CopiaLibro), copiaId);

        if (copia.ProprietarioId == richiedenteId)
        {
            throw new ForbiddenException("Non puoi richiedere in prestito una copia di tua proprietà.");
        }

        if (copia.Stato != StatoCopia.Disponibile)
        {
            throw new ConflictException("La copia richiesta non è attualmente disponibile.");
        }

        var richiestaGiaInCorso = await _dbContext.Prestiti
            .AnyAsync(p => p.CopiaLibroId == copiaId && p.RichiedenteId == richiedenteId && p.Stato == StatoPrestito.Richiesto, cancellationToken);

        if (richiestaGiaInCorso)
        {
            throw new ConflictException("Hai già una richiesta in attesa per questa copia.");
        }

        var richiedente = await _dbContext.Utenti.SingleOrDefaultAsync(u => u.Id == richiedenteId, cancellationToken)
            ?? throw new NotFoundException(nameof(Utente), richiedenteId);

        var prestito = new Prestito
        {
            Id = Guid.NewGuid(),
            CopiaLibroId = copiaId,
            RichiedenteId = richiedenteId,
            Stato = StatoPrestito.Richiesto,
            DataRichiesta = DateTime.UtcNow,
            GiorniStimati = request.GiorniStimati
        };

        _dbContext.Prestiti.Add(prestito);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToPrestitoDto(prestito, copia.Libro, richiedente, copia.Proprietario);
    }

    public async Task<PrestitoDto> AccettaPrestitoAsync(Guid prestitoId, Guid utenteId, CancellationToken cancellationToken = default)
    {
        var prestito = await CaricaPrestitoAsync(prestitoId, cancellationToken);

        if (prestito.CopiaLibro.ProprietarioId != utenteId)
        {
            throw new ForbiddenException("Solo il proprietario della copia può accettare questa richiesta.");
        }

        if (prestito.Stato != StatoPrestito.Richiesto)
        {
            throw new ConflictException("La richiesta non è più in attesa di una decisione.");
        }

        prestito.Stato = StatoPrestito.Accettato;
        prestito.CopiaLibro.Stato = StatoCopia.InPrestito;

        // Le altre richieste in attesa per la stessa copia non sono piu' soddisfacibili.
        var altreRichieste = await _dbContext.Prestiti
            .Where(p => p.CopiaLibroId == prestito.CopiaLibroId && p.Id != prestito.Id && p.Stato == StatoPrestito.Richiesto)
            .ToListAsync(cancellationToken);

        foreach (var richiesta in altreRichieste)
        {
            richiesta.Stato = StatoPrestito.Rifiutato;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToPrestitoDto(prestito, prestito.CopiaLibro.Libro, prestito.Richiedente, prestito.CopiaLibro.Proprietario);
    }

    public async Task<PrestitoDto> RifiutaPrestitoAsync(Guid prestitoId, Guid utenteId, CancellationToken cancellationToken = default)
    {
        var prestito = await CaricaPrestitoAsync(prestitoId, cancellationToken);

        if (prestito.CopiaLibro.ProprietarioId != utenteId)
        {
            throw new ForbiddenException("Solo il proprietario della copia può rifiutare questa richiesta.");
        }

        if (prestito.Stato != StatoPrestito.Richiesto)
        {
            throw new ConflictException("La richiesta non è più in attesa di una decisione.");
        }

        prestito.Stato = StatoPrestito.Rifiutato;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToPrestitoDto(prestito, prestito.CopiaLibro.Libro, prestito.Richiedente, prestito.CopiaLibro.Proprietario);
    }

    public async Task<PrestitoDto> AnnullaPrestitoAsync(Guid prestitoId, Guid utenteId, CancellationToken cancellationToken = default)
    {
        var prestito = await CaricaPrestitoAsync(prestitoId, cancellationToken);

        if (prestito.RichiedenteId != utenteId)
        {
            throw new ForbiddenException("Solo il richiedente può annullare questa richiesta.");
        }

        if (prestito.Stato != StatoPrestito.Richiesto)
        {
            throw new ConflictException("La richiesta non è più in attesa e non può essere annullata.");
        }

        prestito.Stato = StatoPrestito.Annullato;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToPrestitoDto(prestito, prestito.CopiaLibro.Libro, prestito.Richiedente, prestito.CopiaLibro.Proprietario);
    }

    public async Task<PrestitoDto> CompletaPrestitoAsync(Guid prestitoId, Guid utenteId, CancellationToken cancellationToken = default)
    {
        var prestito = await CaricaPrestitoAsync(prestitoId, cancellationToken);

        if (prestito.CopiaLibro.ProprietarioId != utenteId)
        {
            throw new ForbiddenException("Solo il proprietario della copia può chiudere il prestito alla restituzione.");
        }

        if (prestito.Stato != StatoPrestito.Accettato)
        {
            throw new ConflictException("Solo un prestito attivo può essere segnato come restituito.");
        }

        prestito.Stato = StatoPrestito.Completato;
        prestito.DataRestituzioneEffettiva = DateTime.UtcNow;
        prestito.CopiaLibro.Stato = StatoCopia.Disponibile;

        await _dbContext.SaveChangesAsync(cancellationToken);

        // La copia restituita torna disponibile: avvisa chi era in attesa.
        await _sottoscrizioniService.NotificaDisponibilitaAsync(prestito.CopiaLibro.LibroId, cancellationToken);

        return ToPrestitoDto(prestito, prestito.CopiaLibro.Libro, prestito.Richiedente, prestito.CopiaLibro.Proprietario);
    }

    public async Task<IReadOnlyList<PrestitoDto>> GetPrestitiInviatiAsync(Guid richiedenteId, CancellationToken cancellationToken = default)
    {
        var prestiti = await _dbContext.Prestiti
            .AsNoTracking()
            .Include(p => p.CopiaLibro).ThenInclude(c => c.Libro)
            .Include(p => p.CopiaLibro).ThenInclude(c => c.Proprietario)
            .Include(p => p.Richiedente)
            .Where(p => p.RichiedenteId == richiedenteId)
            .OrderByDescending(p => p.DataRichiesta)
            .ToListAsync(cancellationToken);

        var recensiti = await PrestitiRecensitiDaAsync(richiedenteId, prestiti, cancellationToken);

        return prestiti
            .Select(p => ToPrestitoDto(p, p.CopiaLibro.Libro, p.Richiedente, p.CopiaLibro.Proprietario, recensiti.Contains(p.Id)))
            .ToList();
    }

    public async Task<IReadOnlyList<PrestitoDto>> GetPrestitiRicevutiAsync(Guid proprietarioId, CancellationToken cancellationToken = default)
    {
        var prestiti = await _dbContext.Prestiti
            .AsNoTracking()
            .Include(p => p.CopiaLibro).ThenInclude(c => c.Libro)
            .Include(p => p.CopiaLibro).ThenInclude(c => c.Proprietario)
            .Include(p => p.Richiedente)
            .Where(p => p.CopiaLibro.ProprietarioId == proprietarioId)
            .OrderByDescending(p => p.DataRichiesta)
            .ToListAsync(cancellationToken);

        var recensiti = await PrestitiRecensitiDaAsync(proprietarioId, prestiti, cancellationToken);

        return prestiti
            .Select(p => ToPrestitoDto(p, p.CopiaLibro.Libro, p.Richiedente, p.CopiaLibro.Proprietario, recensiti.Contains(p.Id)))
            .ToList();
    }

    private async Task<Prestito> CaricaPrestitoAsync(Guid prestitoId, CancellationToken cancellationToken)
    {
        return await _dbContext.Prestiti
            .Include(p => p.CopiaLibro).ThenInclude(c => c.Libro)
            .Include(p => p.CopiaLibro).ThenInclude(c => c.Proprietario)
            .Include(p => p.Richiedente)
            .SingleOrDefaultAsync(p => p.Id == prestitoId, cancellationToken)
            ?? throw new NotFoundException(nameof(Prestito), prestitoId);
    }

    /// <summary>
    /// Fra i prestiti indicati, quelli per cui l'utente ha gia' lasciato almeno
    /// una recensione, della controparte o dell'esperienza di scambio. Due
    /// interrogazioni per tutto l'elenco, invece di due per ogni prestito.
    /// </summary>
    private async Task<HashSet<Guid>> PrestitiRecensitiDaAsync(
        Guid utenteId,
        IReadOnlyCollection<Prestito> prestiti,
        CancellationToken cancellationToken)
    {
        // Si recensisce solo un prestito concluso: gli altri non serve cercarli.
        var conclusi = prestiti.Where(p => p.Stato == StatoPrestito.Completato).Select(p => p.Id).ToList();
        if (conclusi.Count == 0)
        {
            return [];
        }

        var controparte = await _dbContext.RecensioniUtente
            .Where(r => r.AutoreId == utenteId && conclusi.Contains(r.PrestitoId))
            .Select(r => r.PrestitoId)
            .ToListAsync(cancellationToken);

        var esperienza = await _dbContext.RecensioniCopia
            .Where(r => r.AutoreId == utenteId && conclusi.Contains(r.PrestitoId))
            .Select(r => r.PrestitoId)
            .ToListAsync(cancellationToken);

        return [.. controparte, .. esperienza];
    }

    // Le azioni (richiesta, accettazione, rifiuto, annullamento, chiusura)
    // restituiscono sempre un prestito non ancora recensito: la recensione e'
    // possibile solo dopo la chiusura, quindi li' il valore predefinito e' esatto.
    private static PrestitoDto ToPrestitoDto(
        Prestito prestito,
        Libro libro,
        Utente richiedente,
        Utente proprietario,
        bool giaRecensito = false) =>
        new(
            prestito.Id,
            prestito.CopiaLibroId,
            libro.Id,
            libro.Titolo,
            richiedente.Id,
            richiedente.Nickname,
            proprietario.Id,
            proprietario.Nickname,
            prestito.Stato,
            prestito.DataRichiesta,
            prestito.GiorniStimati,
            prestito.DataRestituzioneEffettiva,
            prestito.Stato == StatoPrestito.Accettato ? prestito.DataRichiesta.AddDays(prestito.GiorniStimati) : null,
            giaRecensito);
}
