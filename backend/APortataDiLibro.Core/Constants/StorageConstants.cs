namespace APortataDiLibro.Core.Constants;

/// <summary>
/// Percorsi e convenzioni per lo storage locale delle copertine dei libri,
/// condivisi tra Infrastructure (scrittura/lettura file) e API (static file serving).
/// </summary>
public static class StorageConstants
{
    /// <summary>Cartella radice (relativa alla content root dell'API) per le immagini caricate.</summary>
    public const string CoversRootFolder = "wwwroot/covers";

    /// <summary>Prefisso URL pubblico sotto cui vengono servite le copertine.</summary>
    public const string CoversRequestPath = "/covers";

    /// <summary>Suffisso aggiunto al nome file per la versione thumbnail della copertina.</summary>
    public const string ThumbnailSuffix = "_thumb";

    /// <summary>Larghezza massima, in pixel, delle thumbnail generate da Magick.NET.</summary>
    public const int ThumbnailMaxWidth = 200;

    /// <summary>Larghezza massima, in pixel, dell'immagine di copertina a piena risoluzione.</summary>
    public const int CoverMaxWidth = 1024;
}
