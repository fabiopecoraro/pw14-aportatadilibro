namespace APortataDiLibro.Core.Constants;

/// <summary>
/// Vincoli di dominio del catalogo condiviso, centralizzati qui perche' devono
/// valere in modo identico su tutti i percorsi che scrivono un'opera: creazione
/// diretta da parte dell'Admin, correzione dei metadati e approvazione di una
/// proposta utente.
/// </summary>
public static class CatalogoConstants
{
    /// <summary>
    /// Anno di pubblicazione piu' antico accettato. Corrisponde all'avvio della
    /// stampa a caratteri mobili: sotto questa soglia un valore inserito a mano
    /// e' quasi certamente un errore di digitazione, non un incunabolo.
    /// </summary>
    public const int AnnoPubblicazioneMinimo = 1450;

    /// <summary>
    /// Anno massimo accettato, calcolato rispetto all'anno corrente. Si concede
    /// un anno di margine perche' le edizioni in uscita riportano spesso l'anno
    /// successivo in copertina.
    /// </summary>
    public static int AnnoPubblicazioneMassimo => DateTime.UtcNow.Year + 1;

    /// <summary>
    /// Un'opera deve avere almeno una categoria: un catalogo in cui si puo'
    /// inserire senza classificare degenera in un elenco piatto, e la
    /// navigazione per categoria smette di coprire il catalogo.
    /// </summary>
    public const int CategorieMinimePerLibro = 1;

    /// <summary>
    /// Tetto alle categorie per opera. Non e' un limite tecnico ma editoriale:
    /// oltre una manciata di voci la classificazione smette di discriminare e
    /// il filtro per categoria perde utilita'.
    /// </summary>
    public const int CategorieMassimePerLibro = 5;

    /// <summary>
    /// Lunghezze massime di titolo e autore, uguali per l'opera e per la
    /// proposta. Il vincolo sta qui e non solo nelle colonne perche' una
    /// proposta diventa un'opera all'approvazione: se la proposta ammettesse
    /// un testo piu' lungo di quello che l'opera puo' contenere, l'Admin si
    /// troverebbe davanti una richiesta che non puo' approvare e che il
    /// proponente non puo' piu' correggere.
    /// </summary>
    public const int LunghezzaMassimaTitolo = 200;

    /// <inheritdoc cref="LunghezzaMassimaTitolo"/>
    public const int LunghezzaMassimaAutore = 150;

    /// <summary>Lunghezza massima del nome di una categoria.</summary>
    public const int LunghezzaMassimaNomeCategoria = 50;
}
