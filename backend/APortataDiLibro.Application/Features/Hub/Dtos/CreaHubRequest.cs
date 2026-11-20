namespace APortataDiLibro.Application.Features.Hub.Dtos;

/// <summary>
/// Coordinate inserite dall'utente posizionando un marker su Leaflet.
/// A differenza della ricerca per prossimita', questa posizione e'
/// volutamente scelta e persistita: non si tratta della posizione GPS
/// del browser dell'utente.
/// </summary>
public record CreaHubRequest(
    string Nome,
    string Indirizzo,
    double Lat,
    double Lng);
