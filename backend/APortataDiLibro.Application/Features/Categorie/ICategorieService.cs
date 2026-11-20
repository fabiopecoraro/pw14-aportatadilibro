using APortataDiLibro.Application.Features.Categorie.Dtos;

namespace APortataDiLibro.Application.Features.Categorie;

/// <summary>
/// Gestione del vocabolario controllato delle categorie del catalogo.
/// La sola lettura e' aperta a chiunque, anche senza accesso (serve ai filtri
/// del catalogo oltre che ai form di inserimento); creazione, rinomina ed eliminazione sono
/// riservate all'Admin, come ogni altra scrittura sui metadati condivisi.
/// </summary>
public interface ICategorieService
{
    Task<IReadOnlyList<CategoriaDto>> GetTutteAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CategoriaAdminDto>> GetPerGestioneAsync(CancellationToken cancellationToken = default);

    Task<CategoriaDto> CreaAsync(SalvaCategoriaRequest request, CancellationToken cancellationToken = default);

    Task<CategoriaDto> RinominaAsync(Guid id, SalvaCategoriaRequest request, CancellationToken cancellationToken = default);

    Task EliminaAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica che gli identificativi ricevuti siano un insieme valido di
    /// categorie esistenti e ne restituisce l'elenco deduplicato, pronto per
    /// essere associato a un'opera o a una proposta.
    /// </summary>
    Task<IReadOnlyList<Guid>> RisolviIdsAsync(IReadOnlyList<Guid>? ids, CancellationToken cancellationToken = default);
}
