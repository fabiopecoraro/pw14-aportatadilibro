namespace APortataDiLibro.Application.Features.Recensioni.Dtos;

/// <summary>Recensione dell'opera letteraria, indipendente dalla copia/prestito specifico.</summary>
public record CreaRecensioneLibroRequest(Guid LibroId, int Voto, string? Commento);
