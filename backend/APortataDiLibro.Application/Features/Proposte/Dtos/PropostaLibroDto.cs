using APortataDiLibro.Core.Enums;

using APortataDiLibro.Application.Features.Categorie.Dtos;

namespace APortataDiLibro.Application.Features.Proposte.Dtos;

/// <summary>
/// Proposta di nuovo libro con i dati del proponente: l'Admin vede il
/// <c>Nickname</c> di chi l'ha inoltrata, perche' un'opera entrata nel
/// catalogo condiviso deve poter essere ricondotta a chi l'ha chiesta.
/// </summary>
public record PropostaLibroDto(
    Guid Id,
    string Titolo,
    string Autore,
    IReadOnlyList<CategoriaDto> Categorie,
    int? Anno,
    string? Isbn,
    Guid ProponenteId,
    string ProponenteNickname,
    StatoProposta Stato,
    DateTime DataProposta,
    DateTime? DataGestione,
    Guid? LibroCreatoId);
