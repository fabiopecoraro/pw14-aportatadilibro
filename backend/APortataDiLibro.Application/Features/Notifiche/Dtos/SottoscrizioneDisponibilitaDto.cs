namespace APortataDiLibro.Application.Features.Notifiche.Dtos;

/// <summary>Iscrizione attiva di un utente alle notifiche di disponibilita' di un libro.</summary>
public record SottoscrizioneDisponibilitaDto(
    Guid Id,
    Guid LibroId,
    string LibroTitolo,
    DateTime DataRichiesta);
