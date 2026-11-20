namespace APortataDiLibro.Infrastructure.Covers;

/// <summary>
/// Salvataggio su disco e generazione thumbnail delle copertine delle copie
/// libro tramite Magick.NET.
/// </summary>
public interface ICoverImageService
{
    /// <summary>
    /// Salva l'immagine caricata per la copia indicata, generando sia la
    /// versione a piena risoluzione sia la thumbnail. Restituisce i percorsi
    /// pubblici (relativi a <c>/covers</c>) da persistere su <c>CopiaLibro</c>.
    /// </summary>
    Task<(string PercorsoCopertina, string PercorsoCopertinaThumbnail)> SalvaCopertinaAsync(
        Guid copiaId,
        Stream immagine,
        CancellationToken cancellationToken = default);

    /// <summary>Elimina i file di copertina/thumbnail precedentemente salvati, se presenti.</summary>
    void EliminaCopertina(string? percorsoCopertina, string? percorsoCopertinaThumbnail);
}
