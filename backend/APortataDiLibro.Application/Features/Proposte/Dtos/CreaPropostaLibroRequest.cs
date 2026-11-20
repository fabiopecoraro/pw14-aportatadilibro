namespace APortataDiLibro.Application.Features.Proposte.Dtos;

/// <summary>
/// Dati proposti da un utente per un nuovo libro non presente nel catalogo,
/// inviati dalla sezione "La mia libreria" in attesa di approvazione Admin.
/// </summary>
public record CreaPropostaLibroRequest(
    string Titolo,
    string Autore,
    IReadOnlyList<Guid> CategorieIds,
    int? Anno,
    string? Isbn);
