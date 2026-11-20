using APortataDiLibro.Core.Enums;

namespace APortataDiLibro.Application.Features.Catalogo.Dtos;

/// <summary>
/// Copia fisica posseduta da un utente, cosi' come mostrata nella pagina
/// globale del libro. Espone solo il <c>Nickname</c> del proprietario (oltre
/// al suo Id, necessario al frontend per riconoscere le copie possedute
/// dall'utente corrente), mai dati identificativi reali.
/// </summary>
public record CopiaLibroDto(
    Guid Id,
    Guid ProprietarioId,
    string ProprietarioNickname,
    StatoCopia Stato,
    string? PercorsoCopertina,
    string? PercorsoCopertinaThumbnail,
    DateTime? DataDisponibilitaStimata);
