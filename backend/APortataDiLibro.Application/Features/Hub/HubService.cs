using APortataDiLibro.Application.Features.Hub.Dtos;
using APortataDiLibro.Core.Exceptions;
using APortataDiLibro.Database.Context;
using APortataDiLibro.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace APortataDiLibro.Application.Features.Hub;

/// <summary>
/// Gestisce gli Hub (punti di scambio pubblici) registrati dagli utenti e il
/// sistema di esclusioni Copia/Hub. Per default ogni copia e' disponibile
/// presso tutti gli Hub del proprietario; <see cref="EsclusioneHubLibro"/>
/// modella le sole eccezioni a questa regola.
/// </summary>
public class HubService : IHubService
{
    private readonly ApplicationDbContext _dbContext;

    public HubService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<HubDto> CreaHubAsync(Guid utenteId, CreaHubRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Lat is < -90 or > 90 || request.Lng is < -180 or > 180)
        {
            throw new ValidationException("Coordinate geografiche non valide.");
        }

        var hub = new APortataDiLibro.Database.Entities.Hub
        {
            Id = Guid.NewGuid(),
            UtenteId = utenteId,
            Nome = request.Nome,
            Indirizzo = request.Indirizzo,
            Lat = request.Lat,
            Lng = request.Lng
        };

        _dbContext.Hub.Add(hub);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToHubDto(hub);
    }

    public async Task<IReadOnlyList<HubDto>> GetHubUtenteAsync(Guid utenteId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Hub
            .AsNoTracking()
            .Where(h => h.UtenteId == utenteId)
            .OrderBy(h => h.Nome)
            .Select(h => new HubDto(h.Id, h.Nome, h.Indirizzo, h.Lat, h.Lng))
            .ToListAsync(cancellationToken);
    }

    public async Task EliminaHubAsync(Guid hubId, Guid utenteId, CancellationToken cancellationToken = default)
    {
        var hub = await _dbContext.Hub.SingleOrDefaultAsync(h => h.Id == hubId, cancellationToken)
            ?? throw new NotFoundException(nameof(Database.Entities.Hub), hubId);

        if (hub.UtenteId != utenteId)
        {
            throw new ForbiddenException("Solo il proprietario può eliminare questo Hub.");
        }

        _dbContext.Hub.Remove(hub);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DisponibilitaHubCopiaDto>> GetDisponibilitaCopiaAsync(Guid copiaId, Guid utenteId, CancellationToken cancellationToken = default)
    {
        var copia = await _dbContext.CopieLibro
            .AsNoTracking()
            .SingleOrDefaultAsync(c => c.Id == copiaId, cancellationToken)
            ?? throw new NotFoundException(nameof(CopiaLibro), copiaId);

        if (copia.ProprietarioId != utenteId)
        {
            throw new ForbiddenException("Puoi consultare le esclusioni solo per le tue copie.");
        }

        // Si parte dagli Hub dell'utente (dove la copia e' disponibile per
        // default) e si marca come escluso solo cio' che compare in
        // EsclusioniHubLibro: la sottoquery EXISTS evita di caricare le
        // esclusioni in memoria per poi ricalcolare la differenza lato client.
        return await _dbContext.Hub
            .AsNoTracking()
            .Where(h => h.UtenteId == utenteId)
            .OrderBy(h => h.Nome)
            .Select(h => new DisponibilitaHubCopiaDto(
                h.Id,
                h.Nome,
                h.Indirizzo,
                _dbContext.EsclusioniHubLibro.Any(e => e.HubId == h.Id && e.CopiaLibroId == copiaId)))
            .ToListAsync(cancellationToken);
    }

    public async Task EscludiCopiaAsync(Guid copiaId, Guid hubId, Guid utenteId, CancellationToken cancellationToken = default)
    {
        var (copia, hub) = await CaricaCopiaEHubDelProprietarioAsync(copiaId, hubId, utenteId, cancellationToken);

        var esclusioneEsistente = await _dbContext.EsclusioniHubLibro
            .AnyAsync(e => e.CopiaLibroId == copia.Id && e.HubId == hub.Id, cancellationToken);

        if (esclusioneEsistente)
        {
            return;
        }

        _dbContext.EsclusioniHubLibro.Add(new EsclusioneHubLibro
        {
            CopiaLibroId = copia.Id,
            HubId = hub.Id
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RimuoviEsclusioneAsync(Guid copiaId, Guid hubId, Guid utenteId, CancellationToken cancellationToken = default)
    {
        var (copia, hub) = await CaricaCopiaEHubDelProprietarioAsync(copiaId, hubId, utenteId, cancellationToken);

        var esclusione = await _dbContext.EsclusioniHubLibro
            .SingleOrDefaultAsync(e => e.CopiaLibroId == copia.Id && e.HubId == hub.Id, cancellationToken)
            ?? throw new NotFoundException("L'esclusione richiesta non esiste.");

        _dbContext.EsclusioniHubLibro.Remove(esclusione);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<(CopiaLibro Copia, Database.Entities.Hub Hub)> CaricaCopiaEHubDelProprietarioAsync(
        Guid copiaId, Guid hubId, Guid utenteId, CancellationToken cancellationToken)
    {
        var copia = await _dbContext.CopieLibro.SingleOrDefaultAsync(c => c.Id == copiaId, cancellationToken)
            ?? throw new NotFoundException(nameof(CopiaLibro), copiaId);

        var hub = await _dbContext.Hub.SingleOrDefaultAsync(h => h.Id == hubId, cancellationToken)
            ?? throw new NotFoundException(nameof(Database.Entities.Hub), hubId);

        if (copia.ProprietarioId != utenteId || hub.UtenteId != utenteId)
        {
            throw new ForbiddenException("Puoi gestire le esclusioni solo per le tue copie e i tuoi Hub.");
        }

        return (copia, hub);
    }

    private static HubDto ToHubDto(Database.Entities.Hub hub) =>
        new(hub.Id, hub.Nome, hub.Indirizzo, hub.Lat, hub.Lng);
}
