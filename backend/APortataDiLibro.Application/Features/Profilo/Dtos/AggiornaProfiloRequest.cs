namespace APortataDiLibro.Application.Features.Profilo.Dtos;

/// <summary>
/// Campi modificabili del proprio profilo. <c>Username</c> non compare di
/// proposito: e' l'identificativo con cui si effettua l'accesso ed e'
/// incorporato nei token gia' emessi, quindi cambiarlo invaliderebbe le
/// sessioni attive senza un reale beneficio (l'identita' pubblica e' il
/// Nickname, che invece si puo' cambiare liberamente).
/// </summary>
public record AggiornaProfiloRequest(
    string Nickname,
    string Nome,
    string Cognome,
    string Email);
