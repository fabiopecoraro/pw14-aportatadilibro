using APortataDiLibro.Application.Features.Categorie.Dtos;

namespace APortataDiLibro.Application.Features.Ricerca.Dtos;

/// <summary>
/// Risultato della ricerca geografica: un libro disponibile presso uno o piu'
/// Hub compresi nell'area di mappa richiesta. Non c'e' un raggio: il filtro e'
/// il bounding box inviato dal frontend (vedi <see cref="RicercaService"/>).
/// </summary>
public record RicercaLibroDto(
    Guid LibroId,
    string Titolo,
    string Autore,
    IReadOnlyList<CategoriaDto> Categorie,
    int? Anno,
    IReadOnlyList<HubDisponibilitaDto> HubDisponibili);
