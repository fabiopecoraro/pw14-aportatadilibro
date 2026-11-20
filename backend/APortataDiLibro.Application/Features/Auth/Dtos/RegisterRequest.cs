namespace APortataDiLibro.Application.Features.Auth.Dtos;

/// <summary>
/// Dati di registrazione. Nome, Cognome ed Email sono "dati reali" raccolti
/// per finalita' amministrative/di sicurezza ma non vengono mai esposti
/// pubblicamente: la UI mostra solo <see cref="Nickname"/>.
/// </summary>
/// <param name="AccettazioneTermini">
/// Consenso esplicito all'informativa privacy e ai termini di utilizzo.
/// Senza consenso l'account non viene creato: il consenso e' un presupposto
/// del trattamento, non un'impostazione modificabile a posteriori, ed e'
/// per questo un campo della richiesta e non un endpoint separato.
/// </param>
public record RegisterRequest(
    string Username,
    string Nickname,
    string Nome,
    string Cognome,
    string Email,
    string Password,
    bool AccettazioneTermini);
