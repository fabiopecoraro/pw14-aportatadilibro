using APortataDiLibro.Application.Features.Proposte.Dtos;

namespace APortataDiLibro.Application.Features.Proposte;

public interface IPropostaLibroService
{
    Task<PropostaLibroDto> CreaPropostaAsync(Guid proponenteId, CreaPropostaLibroRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PropostaLibroDto>> GetPropostePersonaliAsync(Guid proponenteId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PropostaLibroDto>> GetPropostInAttesaAsync(CancellationToken cancellationToken = default);

    Task<PropostaLibroDto> ApprovaPropostaAsync(Guid propostaId, CancellationToken cancellationToken = default);

    Task<PropostaLibroDto> RifiutaPropostaAsync(Guid propostaId, CancellationToken cancellationToken = default);
}
