namespace APortataDiLibro.Application.Features.Ricerca.Dtos;

/// <summary>
/// Hub presso cui una copia di un libro e' disponibile, all'interno
/// dell'area di mappa richiesta dal frontend.
/// </summary>
public record HubDisponibilitaDto(
    Guid HubId,
    string Nome,
    string Indirizzo,
    double Lat,
    double Lng,
    int CopieDisponibili);
