using APortataDiLibro.Core.Constants;
using APortataDiLibro.Core.Exceptions;

namespace APortataDiLibro.Core.Validation;

/// <summary>
/// Guardie di dominio del catalogo. Vivono in Core, e non in uno dei servizi
/// applicativi, perche' lo stesso vincolo deve valere su tre percorsi di
/// scrittura distinti — creazione diretta, correzione dei metadati e proposta
/// utente — gestiti da servizi diversi: duplicare il controllo in ciascuno
/// vorrebbe dire poterlo aggiornare in uno solo.
/// </summary>
public static class CatalogoGuard
{
    /// <summary>
    /// L'anno di pubblicazione e' facoltativo, ma se valorizzato deve essere
    /// plausibile: il campo viene digitato a mano e un refuso sporcherebbe in
    /// modo permanente una scheda condivisa da tutte le copie degli utenti.
    /// </summary>
    public static void EnsureAnnoValido(int? anno)
    {
        if (anno is { } valore &&
            (valore < CatalogoConstants.AnnoPubblicazioneMinimo || valore > CatalogoConstants.AnnoPubblicazioneMassimo))
        {
            throw new ValidationException(
                $"L'anno di pubblicazione deve essere compreso fra {CatalogoConstants.AnnoPubblicazioneMinimo} e {CatalogoConstants.AnnoPubblicazioneMassimo}.");
        }
    }

    /// <summary>
    /// Titolo e autore sono obbligatori e limitati in lunghezza. Il controllo
    /// avviene qui, prima della scrittura, perche' un valore troppo lungo
    /// fermato solo dalla colonna arriverebbe a chi usa l'applicazione come
    /// errore generico del server e non come campo da correggere.
    /// </summary>
    public static void EnsureTitoloEAutoreValidi(string? titolo, string? autore)
    {
        if (string.IsNullOrWhiteSpace(titolo) || string.IsNullOrWhiteSpace(autore))
        {
            throw new ValidationException("Titolo e autore sono obbligatori.");
        }

        if (titolo.Trim().Length > CatalogoConstants.LunghezzaMassimaTitolo)
        {
            throw new ValidationException(
                $"Il titolo non può superare i {CatalogoConstants.LunghezzaMassimaTitolo} caratteri.");
        }

        if (autore.Trim().Length > CatalogoConstants.LunghezzaMassimaAutore)
        {
            throw new ValidationException(
                $"L'autore non può superare i {CatalogoConstants.LunghezzaMassimaAutore} caratteri.");
        }
    }

    /// <summary>
    /// Riporta l'ISBN alla forma in cui viene salvato: sole cifre, senza
    /// trattini ne' spazi. Sulla quarta di copertina il codice e' stampato con
    /// i trattini ed e' cosi' che viene ricopiato; salvandolo com'e' scritto,
    /// lo stesso libro inserito con e senza trattini sfuggirebbe al controllo
    /// sui duplicati, che confronta le stringhe. Un valore vuoto diventa
    /// <c>null</c>, perche' l'ISBN e' facoltativo.
    /// </summary>
    public static string? NormalizzaIsbn(string? isbn)
    {
        if (string.IsNullOrWhiteSpace(isbn))
        {
            return null;
        }

        var compatto = isbn.Replace("-", string.Empty).Replace(" ", string.Empty).ToUpperInvariant();

        // ISBN-13: tredici cifre. ISBN-10: nove cifre e un carattere di
        // controllo, che puo' essere la lettera X.
        var valido = compatto.Length switch
        {
            13 => compatto.All(char.IsAsciiDigit),
            10 => compatto[..9].All(char.IsAsciiDigit) && (char.IsAsciiDigit(compatto[9]) || compatto[9] == 'X'),
            _ => false
        };

        if (!valido)
        {
            throw new ValidationException("L'ISBN deve avere 10 o 13 cifre, con o senza trattini.");
        }

        return compatto;
    }
}
