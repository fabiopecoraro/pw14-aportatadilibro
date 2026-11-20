using APortataDiLibro.Application.Features.Recensioni.Dtos;

namespace APortataDiLibro.Application.Features.Recensioni;

public interface IRecensioniService
{
    /// <summary>Recensisce l'affidabilita' della controparte di un prestito concluso.</summary>
    Task<RecensioneUtenteDto> CreaRecensioneUtenteAsync(Guid autoreId, CreaRecensioneUtenteRequest request, CancellationToken cancellationToken = default);

    /// <summary>Recensisce l'opera letteraria, una sola volta per utente.</summary>
    Task<RecensioneLibroDto> CreaRecensioneLibroAsync(Guid autoreId, CreaRecensioneLibroRequest request, CancellationToken cancellationToken = default);

    /// <summary>Recensisce l'esperienza di scambio di un prestito concluso.</summary>
    Task<RecensioneCopiaDto> CreaRecensioneCopiaAsync(Guid autoreId, CreaRecensioneCopiaRequest request, CancellationToken cancellationToken = default);

    /// <summary>Recensioni di affidabilita' ricevute da un utente (mostrate sul suo profilo pubblico).</summary>
    Task<IReadOnlyList<RecensioneUtenteDto>> GetRecensioniUtenteAsync(Guid utenteId, CancellationToken cancellationToken = default);

    /// <summary>Recensioni dell'opera letteraria.</summary>
    Task<IReadOnlyList<RecensioneLibroDto>> GetRecensioniLibroAsync(Guid libroId, CancellationToken cancellationToken = default);

    /// <summary>Recensioni dell'esperienza di scambio relative a un prestito.</summary>
    Task<IReadOnlyList<RecensioneCopiaDto>> GetRecensioniCopiaAsync(Guid prestitoId, CancellationToken cancellationToken = default);
}
