using APortataDiLibro.Application.Features.Categorie.Dtos;

namespace APortataDiLibro.Application.Features.Catalogo.Dtos;

/// <summary>
/// Scheda anagrafica di un'opera del catalogo globale.
/// </summary>
public record LibroDto(
    Guid Id,
    string Titolo,
    string Autore,
    IReadOnlyList<CategoriaDto> Categorie,
    int? Anno,
    string? Isbn);
