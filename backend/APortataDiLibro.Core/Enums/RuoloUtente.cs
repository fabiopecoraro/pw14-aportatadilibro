namespace APortataDiLibro.Core.Enums;

/// <summary>
/// Ruoli applicativi supportati dal sistema RBAC.
/// I ruoli sono due per scelta: un ruolo intermedio di moderazione sarebbe
/// una superficie di autorizzazione in piu' da difendere su ogni endpoint, e
/// con i volumi di un prototipo non c'e' un carico che la giustifichi.
/// </summary>
public enum RuoloUtente
{
    Utente = 0,
    Admin = 1
}
