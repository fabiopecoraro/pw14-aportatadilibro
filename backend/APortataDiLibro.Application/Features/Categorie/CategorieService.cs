using APortataDiLibro.Application.Features.Categorie.Dtos;
using APortataDiLibro.Core.Constants;
using APortataDiLibro.Core.Exceptions;
using APortataDiLibro.Database.Context;
using APortataDiLibro.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace APortataDiLibro.Application.Features.Categorie;

/// <summary>
/// Gestisce il vocabolario controllato delle categorie.
/// <para>
/// Il confronto fra nomi e' sempre case-insensitive: e' l'unico modo di
/// impedire che "Giallo" e "giallo" finiscano per essere due voci distinte,
/// cioe' esattamente il problema che il vocabolario chiuso esiste per
/// risolvere. Il database applica lo stesso vincolo con un indice unico su
/// <c>lower("Nome")</c>; il controllo qui serve a restituire un messaggio
/// leggibile invece di una violazione di vincolo.
/// </para>
/// </summary>
public class CategorieService : ICategorieService
{
    private readonly ApplicationDbContext _dbContext;

    public CategorieService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<CategoriaDto>> GetTutteAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Categorie
            .AsNoTracking()
            .OrderBy(c => c.Nome)
            .Select(c => new CategoriaDto(c.Id, c.Nome))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CategoriaAdminDto>> GetPerGestioneAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Categorie
            .AsNoTracking()
            .OrderBy(c => c.Nome)
            .Select(c => new CategoriaAdminDto(c.Id, c.Nome, c.Libri.Count, c.Proposte.Count))
            .ToListAsync(cancellationToken);
    }

    public async Task<CategoriaDto> CreaAsync(SalvaCategoriaRequest request, CancellationToken cancellationToken = default)
    {
        var nome = NormalizzaNome(request.Nome);

        await GarantisciNomeLiberoAsync(nome, null, cancellationToken);

        var categoria = new Categoria { Id = Guid.NewGuid(), Nome = nome };

        _dbContext.Categorie.Add(categoria);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CategoriaDto(categoria.Id, categoria.Nome);
    }

    public async Task<CategoriaDto> RinominaAsync(Guid id, SalvaCategoriaRequest request, CancellationToken cancellationToken = default)
    {
        var nome = NormalizzaNome(request.Nome);

        var categoria = await _dbContext.Categorie.SingleOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(Categoria), id);

        await GarantisciNomeLiberoAsync(nome, id, cancellationToken);

        // La rinomina si propaga da sola a tutte le opere: e' il vantaggio del
        // vocabolario rispetto al testo libero, dove correggere un refuso
        // avrebbe voluto dire aggiornare ogni riga che lo conteneva.
        categoria.Nome = nome;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CategoriaDto(categoria.Id, categoria.Nome);
    }

    public async Task EliminaAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var categoria = await _dbContext.Categorie.SingleOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(Categoria), id);

        var libriCollegati = await _dbContext.LibriCategorie.CountAsync(lc => lc.CategoriaId == id, cancellationToken);
        if (libriCollegati > 0)
        {
            throw new ConflictException(
                $"La categoria '{categoria.Nome}' è usata da {libriCollegati} " +
                $"{(libriCollegati == 1 ? "opera" : "opere")} e non può essere eliminata.");
        }

        var proposteCollegate = await _dbContext.ProposteLibroCategorie
            .CountAsync(pc => pc.CategoriaId == id, cancellationToken);
        if (proposteCollegate > 0)
        {
            throw new ConflictException(
                $"La categoria '{categoria.Nome}' è usata da una proposta ancora da valutare.");
        }

        _dbContext.Categorie.Remove(categoria);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Guid>> RisolviIdsAsync(IReadOnlyList<Guid>? ids, CancellationToken cancellationToken = default)
    {
        // Duplicati nella richiesta non sono un errore da segnalare all'utente:
        // la chiave composta della giunzione li rifiuterebbe comunque, e
        // ignorarli e' l'esito che chi ha inviato la richiesta si aspetta.
        var richiesti = (ids ?? []).Distinct().ToList();

        if (richiesti.Count < CatalogoConstants.CategorieMinimePerLibro)
        {
            throw new ValidationException(
                CatalogoConstants.CategorieMinimePerLibro == 1
                    ? "Indica almeno una categoria."
                    : $"Indica almeno {CatalogoConstants.CategorieMinimePerLibro} categorie.");
        }

        if (richiesti.Count > CatalogoConstants.CategorieMassimePerLibro)
        {
            throw new ValidationException(
                $"Puoi indicare al massimo {CatalogoConstants.CategorieMassimePerLibro} categorie.");
        }

        var esistenti = await _dbContext.Categorie
            .Where(c => richiesti.Contains(c.Id))
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        if (esistenti.Count != richiesti.Count)
        {
            throw new ValidationException("Una delle categorie indicate non esiste più.");
        }

        return richiesti;
    }

    private static string NormalizzaNome(string? nome)
    {
        var normalizzato = (nome ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(normalizzato))
        {
            throw new ValidationException("Il nome della categoria è obbligatorio.");
        }

        if (normalizzato.Length > CatalogoConstants.LunghezzaMassimaNomeCategoria)
        {
            throw new ValidationException(
                $"Il nome della categoria non può superare {CatalogoConstants.LunghezzaMassimaNomeCategoria} caratteri.");
        }

        return normalizzato;
    }

    private async Task GarantisciNomeLiberoAsync(string nome, Guid? idDaEscludere, CancellationToken cancellationToken)
    {
        var giaPresente = await _dbContext.Categorie
            .AnyAsync(c => c.Id != idDaEscludere && c.Nome.ToLower() == nome.ToLower(), cancellationToken);

        if (giaPresente)
        {
            throw new ConflictException($"Esiste già una categoria chiamata '{nome}'.");
        }
    }
}
