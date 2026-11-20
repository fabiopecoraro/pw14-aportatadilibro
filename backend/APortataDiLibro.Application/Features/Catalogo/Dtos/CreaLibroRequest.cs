namespace APortataDiLibro.Application.Features.Catalogo.Dtos;

/// <summary>
/// Dati per l'inserimento manuale di una nuova opera nel catalogo globale.
/// I metadati si digitano: il recupero automatico da un servizio
/// bibliografico esterno e' rimandato, perche' legherebbe l'inserimento alla
/// disponibilita' di un terzo.
/// </summary>
public record CreaLibroRequest(
    string Titolo,
    string Autore,
    IReadOnlyList<Guid> CategorieIds,
    int? Anno,
    string? Isbn);
