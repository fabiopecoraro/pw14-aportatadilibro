using APortataDiLibro.Application.Features.Catalogo.Dtos;
using APortataDiLibro.Application.Features.Categorie;
using APortataDiLibro.Application.Features.Categorie.Dtos;
using APortataDiLibro.Application.Features.Metriche;
using APortataDiLibro.Application.Features.Notifiche;
using APortataDiLibro.Core.Enums;
using APortataDiLibro.Core.Exceptions;
using APortataDiLibro.Core.Validation;
using APortataDiLibro.Database.Context;
using APortataDiLibro.Database.Entities;
using APortataDiLibro.Infrastructure.Covers;
using Microsoft.EntityFrameworkCore;

namespace APortataDiLibro.Application.Features.Catalogo;

/// <summary>
/// Gestisce il catalogo globale dei libri e le copie possedute dagli utenti.
/// I metadati core (Titolo, Autore, Categorie, Anno, ISBN) sono modificabili solo
/// dall'Admin tramite <see cref="AggiornaMetadatiAsync"/> una volta inseriti,
/// per garantire la coerenza della scheda condivisa da tutte le copie.
/// </summary>
public class LibroService : ILibroService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ISottoscrizioniService _sottoscrizioniService;
    private readonly ICategorieService _categorieService;
    private readonly IMetricheService _metricheService;
    private readonly ICoverImageService _coverImageService;

    public LibroService(
        ApplicationDbContext dbContext,
        ISottoscrizioniService sottoscrizioniService,
        ICategorieService categorieService,
        IMetricheService metricheService,
        ICoverImageService coverImageService)
    {
        _dbContext = dbContext;
        _sottoscrizioniService = sottoscrizioniService;
        _categorieService = categorieService;
        _metricheService = metricheService;
        _coverImageService = coverImageService;
    }

    public async Task<LibroDto> CreaLibroAsync(CreaLibroRequest request, CancellationToken cancellationToken = default)
    {
        CatalogoGuard.EnsureTitoloEAutoreValidi(request.Titolo, request.Autore);
        CatalogoGuard.EnsureAnnoValido(request.Anno);
        var isbn = CatalogoGuard.NormalizzaIsbn(request.Isbn);

        var categorieIds = await _categorieService.RisolviIdsAsync(request.CategorieIds, cancellationToken);

        if (isbn is not null && await _dbContext.Libri.AnyAsync(l => l.Isbn == isbn, cancellationToken))
        {
            throw new ConflictException($"Esiste già un libro in catalogo con ISBN '{isbn}'.");
        }

        var libro = new Libro
        {
            Id = Guid.NewGuid(),
            Titolo = request.Titolo.Trim(),
            Autore = request.Autore.Trim(),
            Anno = request.Anno,
            Isbn = isbn,
            Categorie = categorieIds.Select(id => new LibroCategoria { CategoriaId = id }).ToList()
        };

        _dbContext.Libri.Add(libro);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await ToLibroDtoAsync(libro.Id, cancellationToken);
    }

    public async Task<IReadOnlyList<LibroSummaryDto>> CercaLibriAsync(
        string? titolo,
        string? autore,
        Guid? categoriaId,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Libri.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(titolo))
        {
            query = query.Where(l => EF.Functions.ILike(l.Titolo, $"%{titolo}%"));
        }

        if (!string.IsNullOrWhiteSpace(autore))
        {
            query = query.Where(l => EF.Functions.ILike(l.Autore, $"%{autore}%"));
        }

        // Con il vocabolario controllato il filtro e' un confronto di
        // identificativi sulla giunzione, non piu' una ILIKE su testo libero:
        // sparisce la classe di errori per cui "giallo" non trovava "Giallo".
        if (categoriaId is { } id)
        {
            query = query.Where(l => l.Categorie.Any(lc => lc.CategoriaId == id));
        }

        return await query
            .OrderBy(l => l.Titolo)
            .Select(l => new LibroSummaryDto(
                l.Id,
                l.Titolo,
                l.Autore,
                l.Categorie
                    .OrderBy(lc => lc.Categoria.Nome)
                    .Select(lc => new CategoriaDto(lc.CategoriaId, lc.Categoria.Nome))
                    .ToList(),
                l.Anno,
                l.Copie.Count(c => c.Stato == StatoCopia.Disponibile),
                l.Copie.Count))
            .ToListAsync(cancellationToken);
    }

    public async Task<LibroDetailDto> GetDettaglioAsync(Guid libroId, CancellationToken cancellationToken = default)
    {
        var libro = await _dbContext.Libri
            .AsNoTracking()
            .Include(l => l.Copie)
                .ThenInclude(c => c.Proprietario)
            .Include(l => l.Copie)
                .ThenInclude(c => c.Prestiti)
            .Include(l => l.Categorie)
                .ThenInclude(lc => lc.Categoria)
            .SingleOrDefaultAsync(l => l.Id == libroId, cancellationToken)
            ?? throw new NotFoundException(nameof(Libro), libroId);

        var recensioni = await _dbContext.RecensioniLibro
            .AsNoTracking()
            .Where(r => r.LibroId == libroId)
            .Select(r => r.Voto)
            .ToListAsync(cancellationToken);

        // La visualizzazione si conta qui e non nell'endpoint perche' questo e'
        // l'unico percorso che apre davvero la scheda di un libro. Il conteggio
        // viene letto dopo l'incremento, cosi' chi apre la pagina vede gia'
        // inclusa la propria visita invece di un valore in ritardo di uno.
        await _metricheService.RegistraVisualizzazioneAsync(libroId, cancellationToken);
        var numeroVisualizzazioni = await _metricheService.GetVisualizzazioniLibroAsync(libroId, cancellationToken);

        var copie = libro.Copie
            .Select(c => new CopiaLibroDto(
                c.Id,
                c.ProprietarioId,
                c.Proprietario.Nickname,
                c.Stato,
                c.PercorsoCopertina,
                c.PercorsoCopertinaThumbnail,
                CalcolaDataDisponibilitaStimata(c)))
            .ToList();

        return new LibroDetailDto(
            libro.Id,
            libro.Titolo,
            libro.Autore,
            ToCategorieDto(libro.Categorie),
            libro.Anno,
            libro.Isbn,
            recensioni.Count > 0 ? recensioni.Average() : null,
            recensioni.Count,
            numeroVisualizzazioni,
            copie);
    }

    public async Task<IReadOnlyList<MiaCopiaLibroDto>> GetLibreriaUtenteAsync(Guid utenteId, CancellationToken cancellationToken = default)
    {
        var copie = await _dbContext.CopieLibro
            .AsNoTracking()
            .Include(c => c.Libro)
                .ThenInclude(l => l.Categorie)
                    .ThenInclude(lc => lc.Categoria)
            .Include(c => c.Prestiti)
            .Where(c => c.ProprietarioId == utenteId)
            .OrderBy(c => c.Libro.Titolo)
            .ToListAsync(cancellationToken);

        // Gli Hub sono per utente, quindi il totale e' lo stesso per ogni copia:
        // basta contarli una volta e sottrarre le esclusioni della singola copia,
        // evitando una query per riga (N+1) nella pagina di libreria.
        var hubTotali = await _dbContext.Hub
            .CountAsync(h => h.UtenteId == utenteId, cancellationToken);

        var esclusioniPerCopia = await _dbContext.EsclusioniHubLibro
            .Where(e => e.CopiaLibro.ProprietarioId == utenteId)
            .GroupBy(e => e.CopiaLibroId)
            .Select(g => new { CopiaId = g.Key, Numero = g.Count() })
            .ToDictionaryAsync(x => x.CopiaId, x => x.Numero, cancellationToken);

        return copie
            .Select(c => new MiaCopiaLibroDto(
                c.Id,
                c.LibroId,
                c.Libro.Titolo,
                c.Libro.Autore,
                ToCategorieDto(c.Libro.Categorie),
                c.Libro.Anno,
                c.Stato,
                c.PercorsoCopertina,
                c.PercorsoCopertinaThumbnail,
                CalcolaDataDisponibilitaStimata(c),
                hubTotali - esclusioniPerCopia.GetValueOrDefault(c.Id),
                hubTotali))
            .ToList();
    }

    public async Task<LibroDto> AggiornaMetadatiAsync(Guid libroId, AggiornaLibroRequest request, CancellationToken cancellationToken = default)
    {
        var libro = await _dbContext.Libri
            .Include(l => l.Categorie)
            .SingleOrDefaultAsync(l => l.Id == libroId, cancellationToken)
            ?? throw new NotFoundException(nameof(Libro), libroId);

        CatalogoGuard.EnsureTitoloEAutoreValidi(request.Titolo, request.Autore);
        CatalogoGuard.EnsureAnnoValido(request.Anno);
        var isbn = CatalogoGuard.NormalizzaIsbn(request.Isbn);

        var categorieIds = await _categorieService.RisolviIdsAsync(request.CategorieIds, cancellationToken);

        // Il duplicato si cerca fra le altre opere: senza escludere questa, una
        // correzione che lascia l'ISBN invariato verrebbe respinta da se stessa.
        if (isbn is not null && await _dbContext.Libri.AnyAsync(l => l.Isbn == isbn && l.Id != libroId, cancellationToken))
        {
            throw new ConflictException($"Esiste già un libro in catalogo con ISBN '{isbn}'.");
        }

        libro.Titolo = request.Titolo.Trim();
        libro.Autore = request.Autore.Trim();
        libro.Anno = request.Anno;
        libro.Isbn = isbn;

        // Si toccano solo le differenze invece di svuotare e riscrivere: una
        // sostituzione in blocco genererebbe DELETE e INSERT anche per le
        // categorie rimaste identiche, rumore inutile sul database e sui log.
        var daRimuovere = libro.Categorie.Where(lc => !categorieIds.Contains(lc.CategoriaId)).ToList();
        foreach (var associazione in daRimuovere)
        {
            libro.Categorie.Remove(associazione);
        }

        var giaPresenti = libro.Categorie.Select(lc => lc.CategoriaId).ToHashSet();
        foreach (var id in categorieIds.Where(id => !giaPresenti.Contains(id)))
        {
            libro.Categorie.Add(new LibroCategoria { LibroId = libro.Id, CategoriaId = id });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await ToLibroDtoAsync(libro.Id, cancellationToken);
    }

    public async Task<CopiaLibroDto> AggiungiCopiaAsync(Guid libroId, Guid proprietarioId, CancellationToken cancellationToken = default)
    {
        var libro = await _dbContext.Libri.AnyAsync(l => l.Id == libroId, cancellationToken);
        if (!libro)
        {
            throw new NotFoundException(nameof(Libro), libroId);
        }

        var proprietario = await _dbContext.Utenti.SingleOrDefaultAsync(u => u.Id == proprietarioId, cancellationToken)
            ?? throw new NotFoundException(nameof(Utente), proprietarioId);

        var copia = new CopiaLibro
        {
            Id = Guid.NewGuid(),
            LibroId = libroId,
            ProprietarioId = proprietarioId,
            Stato = StatoCopia.Disponibile
        };

        _dbContext.CopieLibro.Add(copia);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // La nuova copia disponibile potrebbe soddisfare chi era in attesa.
        await _sottoscrizioniService.NotificaDisponibilitaAsync(libroId, cancellationToken);

        return new CopiaLibroDto(copia.Id, copia.ProprietarioId, proprietario.Nickname, copia.Stato, null, null, null);
    }

    public async Task RimuoviCopiaAsync(Guid copiaId, Guid utenteId, CancellationToken cancellationToken = default)
    {
        var copia = await _dbContext.CopieLibro.SingleOrDefaultAsync(c => c.Id == copiaId, cancellationToken)
            ?? throw new NotFoundException(nameof(CopiaLibro), copiaId);

        if (copia.ProprietarioId != utenteId)
        {
            throw new ForbiddenException("Solo il proprietario può rimuovere questa copia.");
        }

        if (copia.Stato == StatoCopia.InPrestito)
        {
            throw new ConflictException("Impossibile rimuovere una copia attualmente in prestito.");
        }

        // Una copia che e' stata chiesta o prestata non si rimuove: ai suoi
        // prestiti sono legate conversazioni, recensioni e segnalazioni, che
        // appartengono anche all'altra persona e all'eventuale istruttoria
        // dell'Admin. La chiave esterna vieta gia' la cancellazione; il
        // controllo esplicito serve a dirlo a chi ha premuto il pulsante con
        // un messaggio, invece di lasciar emergere l'errore del database.
        var haStorico = await _dbContext.Prestiti.AnyAsync(p => p.CopiaLibroId == copiaId, cancellationToken);
        if (haStorico)
        {
            throw new ConflictException(
                "Questa copia ha uno storico di richieste o prestiti e non può essere rimossa.");
        }

        var (copertina, miniatura) = (copia.PercorsoCopertina, copia.PercorsoCopertinaThumbnail);

        _dbContext.CopieLibro.Remove(copia);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // I file si cancellano dopo il salvataggio, non prima: se la rimozione
        // fallisse, la copia resterebbe in catalogo con una copertina che non
        // esiste piu', e un file eliminato non si recupera con un rollback.
        _coverImageService.EliminaCopertina(copertina, miniatura);
    }

    public async Task<CopiaLibroDto> CaricaCopertinaAsync(Guid copiaId, Guid utenteId, Stream immagine, CancellationToken cancellationToken = default)
    {
        var copia = await _dbContext.CopieLibro
            .Include(c => c.Proprietario)
            .Include(c => c.Prestiti)
            .SingleOrDefaultAsync(c => c.Id == copiaId, cancellationToken)
            ?? throw new NotFoundException(nameof(CopiaLibro), copiaId);

        if (copia.ProprietarioId != utenteId)
        {
            throw new ForbiddenException("Solo il proprietario può caricare la copertina di questa copia.");
        }

        // La copertina precedente non si elimina prima di salvare la nuova: il
        // file prende il nome dall'identificativo della copia, quindi la nuova
        // sovrascrive la vecchia, e se il file caricato non e' un'immagine
        // valida la copia conserva quella che aveva.
        (copia.PercorsoCopertina, copia.PercorsoCopertinaThumbnail) = await _coverImageService.SalvaCopertinaAsync(copiaId, immagine, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CopiaLibroDto(
            copia.Id,
            copia.ProprietarioId,
            copia.Proprietario.Nickname,
            copia.Stato,
            copia.PercorsoCopertina,
            copia.PercorsoCopertinaThumbnail,
            CalcolaDataDisponibilitaStimata(copia));
    }

    private static DateTime? CalcolaDataDisponibilitaStimata(CopiaLibro copia)
    {
        if (copia.Stato != StatoCopia.InPrestito)
        {
            return null;
        }

        var prestitoAttivo = copia.Prestiti
            .Where(p => p.Stato == StatoPrestito.Accettato)
            .OrderByDescending(p => p.DataRichiesta)
            .FirstOrDefault();

        return prestitoAttivo?.DataRichiesta.AddDays(prestitoAttivo.GiorniStimati);
    }

    /// <summary>
    /// Proietta le associazioni di un'opera nell'elenco ordinato di categorie
    /// esposto dai DTO. Richiede che <c>Categoria</c> sia gia' caricata.
    /// </summary>
    private static IReadOnlyList<CategoriaDto> ToCategorieDto(IEnumerable<LibroCategoria> associazioni) =>
        associazioni
            .Select(lc => new CategoriaDto(lc.CategoriaId, lc.Categoria.Nome))
            .OrderBy(c => c.Nome, StringComparer.CurrentCulture)
            .ToList();

    /// <summary>
    /// Rilegge l'opera appena scritta per restituirla con le categorie
    /// risolte. Serve una query perche' dopo un <c>SaveChanges</c> le
    /// associazioni appena create hanno il solo <c>CategoriaId</c>: la
    /// navigazione verso <c>Categoria</c>, e quindi il nome da mostrare, non
    /// e' popolata.
    /// </summary>
    private async Task<LibroDto> ToLibroDtoAsync(Guid libroId, CancellationToken cancellationToken)
    {
        var libro = await _dbContext.Libri
            .AsNoTracking()
            .Include(l => l.Categorie)
                .ThenInclude(lc => lc.Categoria)
            .SingleAsync(l => l.Id == libroId, cancellationToken);

        return new LibroDto(
            libro.Id,
            libro.Titolo,
            libro.Autore,
            ToCategorieDto(libro.Categorie),
            libro.Anno,
            libro.Isbn);
    }
}
