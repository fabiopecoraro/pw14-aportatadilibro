namespace APortataDiLibro.Application.Features.Hub.Dtos;

/// <summary>
/// Punto di scambio pubblico registrato da un utente, con coordinate WGS84
/// pronte per essere visualizzate su Leaflet/OpenStreetMap.
/// </summary>
public record HubDto(
    Guid Id,
    string Nome,
    string Indirizzo,
    double Lat,
    double Lng);
