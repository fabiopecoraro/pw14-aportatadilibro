using APortataDiLibro.Application.Features.Recensioni.Dtos;
using APortataDiLibro.Core.Enums;
using APortataDiLibro.Core.Exceptions;
using APortataDiLibro.Database.Context;
using APortataDiLibro.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace APortataDiLibro.Application.Features.Recensioni;

/// <summary>
/// Gestisce il sistema di recensioni a 3 livelli: affidabilita' della
/// controparte, opera letteraria e singola esperienza di scambio. Le
/// recensioni di prestito (Utente/Copia) richiedono un prestito in stato
/// <see cref="StatoPrestito.Completato"/> e sono limitate a una per autore.
/// </summary>
public class RecensioniService : IRecensioniService
{
    private readonly ApplicationDbContext _dbContext;

    public RecensioniService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RecensioneUtenteDto> CreaRecensioneUtenteAsync(Guid autoreId, CreaRecensioneUtenteRequest request, CancellationToken cancellationToken = default)
    {
        ValidaVoto(request.Voto);

        var prestito = await _dbContext.Prestiti
            .Include(p => p.CopiaLibro)
            .SingleOrDefaultAsync(p => p.Id == request.PrestitoId, cancellationToken)
            ?? throw new NotFoundException(nameof(Prestito), request.PrestitoId);

        if (prestito.Stato != StatoPrestito.Completato)
        {
            throw new ConflictException("Puoi recensire la controparte solo dopo la chiusura del prestito.");
        }

        Guid utenteRecensitoId;
        if (prestito.RichiedenteId == autoreId)
        {
            utenteRecensitoId = prestito.CopiaLibro.ProprietarioId;
        }
        else if (prestito.CopiaLibro.ProprietarioId == autoreId)
        {
            utenteRecensitoId = prestito.RichiedenteId;
        }
        else
        {
            throw new ForbiddenException("Non sei un partecipante a questo prestito.");
        }

        var giaRecensito = await _dbContext.RecensioniUtente
            .AnyAsync(r => r.PrestitoId == request.PrestitoId && r.AutoreId == autoreId, cancellationToken);

        if (giaRecensito)
        {
            throw new ConflictException("Hai già recensito la controparte di questo prestito.");
        }

        var autore = await _dbContext.Utenti.SingleOrDefaultAsync(u => u.Id == autoreId, cancellationToken)
            ?? throw new NotFoundException(nameof(Utente), autoreId);

        var utenteRecensito = await _dbContext.Utenti.SingleOrDefaultAsync(u => u.Id == utenteRecensitoId, cancellationToken)
            ?? throw new NotFoundException(nameof(Utente), utenteRecensitoId);

        var recensione = new RecensioneUtente
        {
            Id = Guid.NewGuid(),
            PrestitoId = request.PrestitoId,
            AutoreId = autoreId,
            UtenteRecensitoId = utenteRecensitoId,
            Voto = request.Voto,
            Commento = request.Commento,
            DataCreazione = DateTime.UtcNow
        };

        _dbContext.RecensioniUtente.Add(recensione);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new RecensioneUtenteDto(
            recensione.Id,
            recensione.PrestitoId,
            autore.Nickname,
            utenteRecensito.Nickname,
            recensione.Voto,
            recensione.Commento,
            recensione.DataCreazione);
    }

    public async Task<RecensioneLibroDto> CreaRecensioneLibroAsync(Guid autoreId, CreaRecensioneLibroRequest request, CancellationToken cancellationToken = default)
    {
        ValidaVoto(request.Voto);

        var libroEsiste = await _dbContext.Libri.AnyAsync(l => l.Id == request.LibroId, cancellationToken);
        if (!libroEsiste)
        {
            throw new NotFoundException(nameof(Libro), request.LibroId);
        }

        var haScambiatoLibro = await _dbContext.Prestiti
            .AnyAsync(p => p.Stato == StatoPrestito.Completato
                && p.CopiaLibro.LibroId == request.LibroId
                && (p.RichiedenteId == autoreId || p.CopiaLibro.ProprietarioId == autoreId), cancellationToken);

        if (!haScambiatoLibro)
        {
            throw new ForbiddenException("Puoi recensire solo libri scambiati tramite un prestito concluso.");
        }

        var giaRecensito = await _dbContext.RecensioniLibro
            .AnyAsync(r => r.LibroId == request.LibroId && r.AutoreId == autoreId, cancellationToken);

        if (giaRecensito)
        {
            throw new ConflictException("Hai già recensito questo libro.");
        }

        var autore = await _dbContext.Utenti.SingleOrDefaultAsync(u => u.Id == autoreId, cancellationToken)
            ?? throw new NotFoundException(nameof(Utente), autoreId);

        var recensione = new RecensioneLibro
        {
            Id = Guid.NewGuid(),
            LibroId = request.LibroId,
            AutoreId = autoreId,
            Voto = request.Voto,
            Commento = request.Commento,
            DataCreazione = DateTime.UtcNow
        };

        _dbContext.RecensioniLibro.Add(recensione);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new RecensioneLibroDto(recensione.Id, recensione.LibroId, autore.Nickname, recensione.Voto, recensione.Commento, recensione.DataCreazione);
    }

    public async Task<RecensioneCopiaDto> CreaRecensioneCopiaAsync(Guid autoreId, CreaRecensioneCopiaRequest request, CancellationToken cancellationToken = default)
    {
        ValidaVoto(request.Voto);

        var prestito = await _dbContext.Prestiti
            .Include(p => p.CopiaLibro)
            .SingleOrDefaultAsync(p => p.Id == request.PrestitoId, cancellationToken)
            ?? throw new NotFoundException(nameof(Prestito), request.PrestitoId);

        if (prestito.Stato != StatoPrestito.Completato)
        {
            throw new ConflictException("Puoi recensire l'esperienza di scambio solo dopo la chiusura del prestito.");
        }

        if (prestito.RichiedenteId != autoreId && prestito.CopiaLibro.ProprietarioId != autoreId)
        {
            throw new ForbiddenException("Non sei un partecipante a questo prestito.");
        }

        var giaRecensito = await _dbContext.RecensioniCopia
            .AnyAsync(r => r.PrestitoId == request.PrestitoId && r.AutoreId == autoreId, cancellationToken);

        if (giaRecensito)
        {
            throw new ConflictException("Hai già recensito l'esperienza di scambio di questo prestito.");
        }

        var autore = await _dbContext.Utenti.SingleOrDefaultAsync(u => u.Id == autoreId, cancellationToken)
            ?? throw new NotFoundException(nameof(Utente), autoreId);

        var recensione = new RecensioneCopia
        {
            Id = Guid.NewGuid(),
            PrestitoId = request.PrestitoId,
            AutoreId = autoreId,
            Voto = request.Voto,
            Commento = request.Commento,
            DataCreazione = DateTime.UtcNow
        };

        _dbContext.RecensioniCopia.Add(recensione);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new RecensioneCopiaDto(recensione.Id, recensione.PrestitoId, autore.Nickname, recensione.Voto, recensione.Commento, recensione.DataCreazione);
    }

    public async Task<IReadOnlyList<RecensioneUtenteDto>> GetRecensioniUtenteAsync(Guid utenteId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.RecensioniUtente
            .AsNoTracking()
            .Include(r => r.Autore)
            .Include(r => r.UtenteRecensito)
            .Where(r => r.UtenteRecensitoId == utenteId)
            .OrderByDescending(r => r.DataCreazione)
            .Select(r => new RecensioneUtenteDto(r.Id, r.PrestitoId, r.Autore.Nickname, r.UtenteRecensito.Nickname, r.Voto, r.Commento, r.DataCreazione))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<RecensioneLibroDto>> GetRecensioniLibroAsync(Guid libroId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.RecensioniLibro
            .AsNoTracking()
            .Include(r => r.Autore)
            .Where(r => r.LibroId == libroId)
            .OrderByDescending(r => r.DataCreazione)
            .Select(r => new RecensioneLibroDto(r.Id, r.LibroId, r.Autore.Nickname, r.Voto, r.Commento, r.DataCreazione))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<RecensioneCopiaDto>> GetRecensioniCopiaAsync(Guid prestitoId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.RecensioniCopia
            .AsNoTracking()
            .Include(r => r.Autore)
            .Where(r => r.PrestitoId == prestitoId)
            .OrderByDescending(r => r.DataCreazione)
            .Select(r => new RecensioneCopiaDto(r.Id, r.PrestitoId, r.Autore.Nickname, r.Voto, r.Commento, r.DataCreazione))
            .ToListAsync(cancellationToken);
    }

    private static void ValidaVoto(int voto)
    {
        if (voto is < 1 or > 5)
        {
            throw new ValidationException("Il voto deve essere compreso tra 1 e 5.");
        }
    }
}
