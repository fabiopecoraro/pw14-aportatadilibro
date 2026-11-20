namespace APortataDiLibro.Application.Features.Recensioni.Dtos;

public record RecensioneLibroDto(
    Guid Id,
    Guid LibroId,
    string AutoreNickname,
    int Voto,
    string? Commento,
    DateTime DataCreazione);
