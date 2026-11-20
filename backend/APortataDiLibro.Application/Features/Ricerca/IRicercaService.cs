using APortataDiLibro.Application.Features.Ricerca.Dtos;

namespace APortataDiLibro.Application.Features.Ricerca;

public interface IRicercaService
{
    /// <summary>
    /// Cerca i libri disponibili presso gli Hub compresi nell'area di mappa
    /// delimitata dall'angolo sud-ovest (<paramref name="sud"/>, <paramref name="ovest"/>)
    /// e dall'angolo nord-est (<paramref name="nord"/>, <paramref name="est"/>).
    /// L'area visibile sulla mappa del frontend e' usata solo per questa query
    /// e non viene mai salvata: la posizione GPS del browser non deve mai essere persistita.
    /// </summary>
    Task<IReadOnlyList<RicercaLibroDto>> CercaLibriAsync(
        double sud,
        double ovest,
        double nord,
        double est,
        string? titolo,
        string? autore,
        Guid? categoriaId,
        CancellationToken cancellationToken = default);
}
