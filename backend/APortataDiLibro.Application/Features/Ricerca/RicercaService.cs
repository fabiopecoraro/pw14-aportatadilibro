using APortataDiLibro.Application.Features.Categorie.Dtos;
using APortataDiLibro.Application.Features.Ricerca.Dtos;
using APortataDiLibro.Core.Enums;
using APortataDiLibro.Core.Exceptions;
using APortataDiLibro.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace APortataDiLibro.Application.Features.Ricerca;

/// <summary>
/// Esegue la ricerca geografica dei libri disponibili presso gli Hub compresi
/// nell'area di mappa (bounding box) richiesta dal frontend. Le coordinate
/// arrivano dal browser solo per calcolare questa query e non vengono mai
/// scritte su database.
/// </summary>
public class RicercaService : IRicercaService
{
    private readonly ApplicationDbContext _dbContext;

    public RicercaService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<RicercaLibroDto>> CercaLibriAsync(
        double sud,
        double ovest,
        double nord,
        double est,
        string? titolo,
        string? autore,
        Guid? categoriaId,
        CancellationToken cancellationToken = default)
    {
        if (sud is < -90 or > 90 || nord is < -90 or > 90 || ovest is < -180 or > 180 || est is < -180 or > 180)
        {
            throw new ValidationException("Coordinate geografiche non valide.");
        }

        if (sud > nord || ovest > est)
        {
            throw new ValidationException("L'area di ricerca non è valida.");
        }

        var query =
            from hub in _dbContext.Hub.AsNoTracking()
            where hub.Lat >= sud && hub.Lat <= nord && hub.Lng >= ovest && hub.Lng <= est
            from copia in _dbContext.CopieLibro.AsNoTracking()
            where copia.ProprietarioId == hub.UtenteId
                && copia.Stato == StatoCopia.Disponibile
                && !_dbContext.EsclusioniHubLibro.Any(e => e.HubId == hub.Id && e.CopiaLibroId == copia.Id)
            select new
            {
                HubId = hub.Id,
                HubNome = hub.Nome,
                HubIndirizzo = hub.Indirizzo,
                HubLat = hub.Lat,
                HubLng = hub.Lng,
                LibroId = copia.Libro.Id,
                Titolo = copia.Libro.Titolo,
                Autore = copia.Libro.Autore,
                Anno = copia.Libro.Anno
            };

        if (!string.IsNullOrWhiteSpace(titolo))
        {
            query = query.Where(r => EF.Functions.ILike(r.Titolo, $"%{titolo}%"));
        }

        if (!string.IsNullOrWhiteSpace(autore))
        {
            query = query.Where(r => EF.Functions.ILike(r.Autore, $"%{autore}%"));
        }

        if (categoriaId is { } id)
        {
            query = query.Where(r => _dbContext.LibriCategorie.Any(lc => lc.LibroId == r.LibroId && lc.CategoriaId == id));
        }

        var righe = await query.ToListAsync(cancellationToken);

        // Le categorie si caricano in una seconda query invece di entrare nella
        // proiezione qui sopra, per due ragioni: il raggruppamento successivo
        // usa una chiave anonima, e una collezione dentro una chiave non
        // confronta per valore; e ripetere le categorie su ogni riga del
        // prodotto Hub x copia significherebbe trasferirle decine di volte per
        // lo stesso libro. Una query in piu' costa meno di entrambe le cose.
        var libriIds = righe.Select(r => r.LibroId).Distinct().ToList();

        var categoriePerLibro = await _dbContext.LibriCategorie
            .AsNoTracking()
            .Where(lc => libriIds.Contains(lc.LibroId))
            .OrderBy(lc => lc.Categoria.Nome)
            .Select(lc => new { lc.LibroId, Categoria = new CategoriaDto(lc.CategoriaId, lc.Categoria.Nome) })
            .ToListAsync(cancellationToken);

        var indiceCategorie = categoriePerLibro
            .GroupBy(x => x.LibroId)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<CategoriaDto>)g.Select(x => x.Categoria).ToList());

        return righe
            .GroupBy(r => new { r.LibroId, r.Titolo, r.Autore, r.Anno })
            .Select(libroGroup => new RicercaLibroDto(
                libroGroup.Key.LibroId,
                libroGroup.Key.Titolo,
                libroGroup.Key.Autore,
                indiceCategorie.GetValueOrDefault(libroGroup.Key.LibroId, []),
                libroGroup.Key.Anno,
                libroGroup
                    .GroupBy(r => new { r.HubId, r.HubNome, r.HubIndirizzo, r.HubLat, r.HubLng })
                    .Select(hubGroup => new HubDisponibilitaDto(
                        hubGroup.Key.HubId,
                        hubGroup.Key.HubNome,
                        hubGroup.Key.HubIndirizzo,
                        hubGroup.Key.HubLat,
                        hubGroup.Key.HubLng,
                        hubGroup.Count()))
                    .OrderBy(h => h.Nome)
                    .ToList()))
            .OrderBy(l => l.Titolo)
            .ToList();
    }
}
