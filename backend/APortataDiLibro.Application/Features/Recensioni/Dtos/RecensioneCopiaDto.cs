namespace APortataDiLibro.Application.Features.Recensioni.Dtos;

public record RecensioneCopiaDto(
    Guid Id,
    Guid PrestitoId,
    string AutoreNickname,
    int Voto,
    string? Commento,
    DateTime DataCreazione);
