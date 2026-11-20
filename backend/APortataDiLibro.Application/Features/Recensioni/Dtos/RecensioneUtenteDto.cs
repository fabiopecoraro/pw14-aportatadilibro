namespace APortataDiLibro.Application.Features.Recensioni.Dtos;

public record RecensioneUtenteDto(
    Guid Id,
    Guid PrestitoId,
    string AutoreNickname,
    string UtenteRecensitoNickname,
    int Voto,
    string? Commento,
    DateTime DataCreazione);
