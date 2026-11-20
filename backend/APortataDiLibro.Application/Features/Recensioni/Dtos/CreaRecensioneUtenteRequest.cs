namespace APortataDiLibro.Application.Features.Recensioni.Dtos;

/// <summary>Recensione dell'affidabilita' della controparte di un prestito concluso.</summary>
public record CreaRecensioneUtenteRequest(Guid PrestitoId, int Voto, string? Commento);
