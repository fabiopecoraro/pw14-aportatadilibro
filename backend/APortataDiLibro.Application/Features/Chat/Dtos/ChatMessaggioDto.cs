namespace APortataDiLibro.Application.Features.Chat.Dtos;

/// <summary>
/// Messaggio della chat privata di un prestito, cosi' come mostrato ai due
/// partecipanti (e all'Admin in fase di ispezione di un report). Espone solo
/// il nickname del mittente.
/// </summary>
public record ChatMessaggioDto(
    Guid Id,
    Guid PrestitoId,
    Guid MittenteId,
    string MittenteNickname,
    string Testo,
    DateTime DataInvio);
