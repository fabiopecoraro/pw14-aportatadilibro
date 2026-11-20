using APortataDiLibro.Core.Enums;

using APortataDiLibro.Application.Features.Categorie.Dtos;

namespace APortataDiLibro.Application.Features.Catalogo.Dtos;

/// <summary>
/// Copia posseduta dall'utente corrente, cosi' come mostrata nella sezione
/// "La mia libreria": riporta i dati dell'opera condivisa e lo stato della
/// singola copia (libera o in prestito), con l'eventuale data di rientro
/// stimata.
/// <para>
/// <c>HubDisponibili</c> e <c>HubTotali</c> riassumono l'effetto del sistema
/// di esclusioni sulla copia, cosi' che la card di libreria possa segnalare
/// a colpo d'occhio una disponibilita' ridotta senza una chiamata aggiuntiva.
/// </para>
/// </summary>
public record MiaCopiaLibroDto(
    Guid Id,
    Guid LibroId,
    string Titolo,
    string Autore,
    IReadOnlyList<CategoriaDto> Categorie,
    int? Anno,
    StatoCopia Stato,
    string? PercorsoCopertina,
    string? PercorsoCopertinaThumbnail,
    DateTime? DataDisponibilitaStimata,
    int HubDisponibili,
    int HubTotali);
