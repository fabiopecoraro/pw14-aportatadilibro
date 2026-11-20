using APortataDiLibro.Application.Features.Categorie;
using APortataDiLibro.Application.Features.Categorie.Dtos;
using APortataDiLibro.Application.Features.Proposte.Dtos;
using APortataDiLibro.Core.Enums;
using APortataDiLibro.Core.Exceptions;
using APortataDiLibro.Core.Validation;
using APortataDiLibro.Database.Context;
using APortataDiLibro.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace APortataDiLibro.Application.Features.Proposte;

/// <summary>
/// Gestisce le proposte di nuovi libri inviate dagli utenti dalla propria
/// libreria personale quando il titolo cercato non e' presente in catalogo.
/// Ogni proposta resta <see cref="StatoProposta.InAttesa"/> finche' l'Admin
/// non la valuta: l'approvazione crea automaticamente la scheda
/// <see cref="Libro"/> condivisa e una <see cref="CopiaLibro"/> per il
/// proponente, mantenendo traccia di chi ha effettuato l'inserimento.
/// </summary>
public class PropostaLibroService : IPropostaLibroService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICategorieService _categorieService;

    public PropostaLibroService(ApplicationDbContext dbContext, ICategorieService categorieService)
    {
        _dbContext = dbContext;
        _categorieService = categorieService;
    }

    public async Task<PropostaLibroDto> CreaPropostaAsync(Guid proponenteId, CreaPropostaLibroRequest request, CancellationToken cancellationToken = default)
    {
        CatalogoGuard.EnsureTitoloEAutoreValidi(request.Titolo, request.Autore);
        CatalogoGuard.EnsureAnnoValido(request.Anno);
        var isbn = CatalogoGuard.NormalizzaIsbn(request.Isbn);

        // Anche in proposta si sceglie dal vocabolario esistente: l'approvazione
        // si limita cosi' a ricopiare le associazioni, senza dover interpretare
        // o normalizzare testo libero scritto da chi ha compilato il form.
        var categorieIds = await _categorieService.RisolviIdsAsync(request.CategorieIds, cancellationToken);

        var proponenteEsiste = await _dbContext.Utenti.AnyAsync(u => u.Id == proponenteId, cancellationToken);
        if (!proponenteEsiste)
        {
            throw new NotFoundException(nameof(Utente), proponenteId);
        }

        var proposta = new PropostaLibro
        {
            Id = Guid.NewGuid(),
            Titolo = request.Titolo.Trim(),
            Autore = request.Autore.Trim(),
            Anno = request.Anno,
            Isbn = isbn,
            ProponenteId = proponenteId,
            Stato = StatoProposta.InAttesa,
            DataProposta = DateTime.UtcNow,
            Categorie = categorieIds.Select(id => new PropostaLibroCategoria { CategoriaId = id }).ToList()
        };

        _dbContext.ProposteLibro.Add(proposta);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await ToDtoAsync(proposta.Id, cancellationToken);
    }

    public async Task<IReadOnlyList<PropostaLibroDto>> GetPropostePersonaliAsync(Guid proponenteId, CancellationToken cancellationToken = default)
    {
        return await ProiettaProposte()
            .Where(p => p.ProponenteId == proponenteId)
            .OrderByDescending(p => p.DataProposta)
            .Select(ToDtoExpression())
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PropostaLibroDto>> GetPropostInAttesaAsync(CancellationToken cancellationToken = default)
    {
        return await ProiettaProposte()
            .Where(p => p.Stato == StatoProposta.InAttesa)
            .OrderBy(p => p.DataProposta)
            .Select(ToDtoExpression())
            .ToListAsync(cancellationToken);
    }

    public async Task<PropostaLibroDto> ApprovaPropostaAsync(Guid propostaId, CancellationToken cancellationToken = default)
    {
        var proposta = await _dbContext.ProposteLibro
            .Include(p => p.Proponente)
            .Include(p => p.Categorie)
            .SingleOrDefaultAsync(p => p.Id == propostaId, cancellationToken)
            ?? throw new NotFoundException(nameof(PropostaLibro), propostaId);

        if (proposta.Stato != StatoProposta.InAttesa)
        {
            throw new ConflictException("La proposta è già stata gestita.");
        }

        if (!string.IsNullOrWhiteSpace(proposta.Isbn))
        {
            var isbnGiaPresente = await _dbContext.Libri.AnyAsync(l => l.Isbn == proposta.Isbn, cancellationToken);
            if (isbnGiaPresente)
            {
                throw new ConflictException($"Esiste già un libro in catalogo con ISBN '{proposta.Isbn}'.");
            }
        }

        // Una categoria puo' essere stata eliminata fra l'invio della proposta e
        // la sua valutazione: le associazioni superstiti sono la fonte di
        // verita', e se non ne resta nessuna l'Admin deve saperlo invece di
        // ritrovarsi un'opera senza classificazione.
        var categorieIds = proposta.Categorie.Select(pc => pc.CategoriaId).ToList();
        var categorieValide = await _dbContext.Categorie
            .Where(c => categorieIds.Contains(c.Id))
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        if (categorieValide.Count == 0)
        {
            throw new ConflictException(
                "Le categorie indicate nella proposta non esistono più: assegnane di nuove prima di approvarla.");
        }

        var libro = new Libro
        {
            Id = Guid.NewGuid(),
            Titolo = proposta.Titolo,
            Autore = proposta.Autore,
            Anno = proposta.Anno,
            Isbn = proposta.Isbn,
            Categorie = categorieValide.Select(id => new LibroCategoria { CategoriaId = id }).ToList()
        };

        var copia = new CopiaLibro
        {
            Id = Guid.NewGuid(),
            LibroId = libro.Id,
            ProprietarioId = proposta.ProponenteId,
            Stato = StatoCopia.Disponibile
        };

        proposta.Stato = StatoProposta.Approvata;
        proposta.DataGestione = DateTime.UtcNow;
        proposta.LibroCreatoId = libro.Id;

        _dbContext.Libri.Add(libro);
        _dbContext.CopieLibro.Add(copia);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await ToDtoAsync(proposta.Id, cancellationToken);
    }

    public async Task<PropostaLibroDto> RifiutaPropostaAsync(Guid propostaId, CancellationToken cancellationToken = default)
    {
        var proposta = await _dbContext.ProposteLibro
            .SingleOrDefaultAsync(p => p.Id == propostaId, cancellationToken)
            ?? throw new NotFoundException(nameof(PropostaLibro), propostaId);

        if (proposta.Stato != StatoProposta.InAttesa)
        {
            throw new ConflictException("La proposta è già stata gestita.");
        }

        proposta.Stato = StatoProposta.Rifiutata;
        proposta.DataGestione = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await ToDtoAsync(proposta.Id, cancellationToken);
    }

    private IQueryable<PropostaLibro> ProiettaProposte() =>
        _dbContext.ProposteLibro.AsNoTracking().Include(p => p.Proponente);

    /// <summary>
    /// Proiezione condivisa dalle due query di elenco e dalla rilettura
    /// successiva a una scrittura: tenerla in un punto solo evita che i tre
    /// percorsi restituiscano forme leggermente diverse dello stesso oggetto.
    /// </summary>
    private static System.Linq.Expressions.Expression<Func<PropostaLibro, PropostaLibroDto>> ToDtoExpression() =>
        p => new PropostaLibroDto(
            p.Id,
            p.Titolo,
            p.Autore,
            p.Categorie
                .OrderBy(pc => pc.Categoria.Nome)
                .Select(pc => new CategoriaDto(pc.CategoriaId, pc.Categoria.Nome))
                .ToList(),
            p.Anno,
            p.Isbn,
            p.ProponenteId,
            p.Proponente.Nickname,
            p.Stato,
            p.DataProposta,
            p.DataGestione,
            p.LibroCreatoId);

    private Task<PropostaLibroDto> ToDtoAsync(Guid propostaId, CancellationToken cancellationToken) =>
        ProiettaProposte()
            .Where(p => p.Id == propostaId)
            .Select(ToDtoExpression())
            .SingleAsync(cancellationToken);
}
