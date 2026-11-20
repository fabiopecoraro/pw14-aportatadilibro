using APortataDiLibro.Application.Features.Categorie.Dtos;

namespace APortataDiLibro.Application.Features.Catalogo.Dtos;

/// <summary>
/// Pagina globale di un libro: metadati, valutazione media dell'opera
/// (secondo livello del sistema di recensioni) e lo stato di tutte le
/// copie possedute dagli utenti, con relativa data di disponibilita' stimata.
/// </summary>
public record LibroDetailDto(
    Guid Id,
    string Titolo,
    string Autore,
    IReadOnlyList<CategoriaDto> Categorie,
    int? Anno,
    string? Isbn,
    double? MediaVotoRecensioni,
    int NumeroRecensioni,
    int NumeroVisualizzazioni,
    IReadOnlyList<CopiaLibroDto> Copie);
