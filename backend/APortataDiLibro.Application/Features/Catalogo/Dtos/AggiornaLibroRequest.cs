namespace APortataDiLibro.Application.Features.Catalogo.Dtos;

/// <summary>
/// Correzione dei metadati core di un'opera, operazione riservata all'Admin
/// una volta che il libro e' stato inserito nel catalogo.
/// </summary>
public record AggiornaLibroRequest(
    string Titolo,
    string Autore,
    IReadOnlyList<Guid> CategorieIds,
    int? Anno,
    string? Isbn);
