namespace APortataDiLibro.Application.Features.Recensioni.Dtos;

/// <summary>Recensione dell'esperienza di scambio specifica di un prestito concluso.</summary>
public record CreaRecensioneCopiaRequest(Guid PrestitoId, int Voto, string? Commento);
