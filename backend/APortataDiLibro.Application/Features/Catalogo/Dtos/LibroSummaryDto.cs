using APortataDiLibro.Application.Features.Categorie.Dtos;

namespace APortataDiLibro.Application.Features.Catalogo.Dtos;

/// <summary>
/// Riga di risultato per la ricerca/sfoglia del catalogo: include i conteggi
/// delle copie per consentire all'utente di valutare a colpo d'occhio la
/// disponibilita' del titolo senza aprire la pagina di dettaglio.
/// </summary>
public record LibroSummaryDto(
    Guid Id,
    string Titolo,
    string Autore,
    IReadOnlyList<CategoriaDto> Categorie,
    int? Anno,
    int CopieDisponibili,
    int CopieTotali);
