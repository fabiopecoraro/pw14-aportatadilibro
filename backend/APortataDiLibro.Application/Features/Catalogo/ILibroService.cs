using APortataDiLibro.Application.Features.Catalogo.Dtos;

namespace APortataDiLibro.Application.Features.Catalogo;

public interface ILibroService
{
    Task<LibroDto> CreaLibroAsync(CreaLibroRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LibroSummaryDto>> CercaLibriAsync(
        string? titolo,
        string? autore,
        Guid? categoriaId,
        CancellationToken cancellationToken = default);

    Task<LibroDetailDto> GetDettaglioAsync(Guid libroId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MiaCopiaLibroDto>> GetLibreriaUtenteAsync(Guid utenteId, CancellationToken cancellationToken = default);

    Task<LibroDto> AggiornaMetadatiAsync(Guid libroId, AggiornaLibroRequest request, CancellationToken cancellationToken = default);

    Task<CopiaLibroDto> AggiungiCopiaAsync(Guid libroId, Guid proprietarioId, CancellationToken cancellationToken = default);

    Task RimuoviCopiaAsync(Guid copiaId, Guid utenteId, CancellationToken cancellationToken = default);

    Task<CopiaLibroDto> CaricaCopertinaAsync(Guid copiaId, Guid utenteId, Stream immagine, CancellationToken cancellationToken = default);
}
