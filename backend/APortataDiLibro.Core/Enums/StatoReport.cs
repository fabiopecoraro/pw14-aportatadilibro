namespace APortataDiLibro.Core.Enums;

/// <summary>
/// Stato di una segnalazione di comportamento scorretto in chat.
/// L'Admin puo' ispezionare la cronologia della chat solo per i report
/// in stato <see cref="Aperto"/>: le chat sono private, e l'accesso finisce
/// quando l'istruttoria che lo giustifica e' conclusa.
/// </summary>
public enum StatoReport
{
    Aperto = 0,
    Chiuso = 1
}
