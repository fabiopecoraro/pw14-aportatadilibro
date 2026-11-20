namespace APortataDiLibro.Core.Enums;

/// <summary>
/// Ciclo di vita di una richiesta di prestito tra due utenti.
/// La transizione a Completato e' permessa solo al proprietario della copia
/// fisica: e' l'unico che sa di aver riavuto il libro.
/// </summary>
public enum StatoPrestito
{
    Richiesto = 0,
    Accettato = 1,
    Rifiutato = 2,
    Completato = 3,
    Annullato = 4
}
