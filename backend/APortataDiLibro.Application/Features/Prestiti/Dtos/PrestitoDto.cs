using APortataDiLibro.Core.Enums;

namespace APortataDiLibro.Application.Features.Prestiti.Dtos;

/// <summary>
/// Richiesta/prestito di una copia, cosi' come mostrata nelle liste "richieste
/// inviate" e "richieste ricevute". Espone solo i nickname dei partecipanti,
/// mai dati reali; gli Id (come in <c>CopiaLibroDto.ProprietarioId</c>) sono
/// comunque presenti perche' servono al frontend, ad esempio per identificare
/// la controparte quando si segnala un comportamento scorretto in chat.
/// </summary>
public record PrestitoDto(
    Guid Id,
    Guid CopiaLibroId,
    Guid LibroId,
    string LibroTitolo,
    Guid RichiedenteId,
    string RichiedenteNickname,
    Guid ProprietarioId,
    string ProprietarioNickname,
    StatoPrestito Stato,
    DateTime DataRichiesta,
    int GiorniStimati,
    DateTime? DataRestituzioneEffettiva,
    DateTime? DataDisponibilitaStimata,
    // Vero se chi sta leggendo l'elenco ha gia' lasciato una recensione per
    // questo prestito. Dipende da chi chiede, e sta comunque nel DTO: senza,
    // l'interfaccia non avrebbe modo di sapere se proporre ancora "Recensisci",
    // e lo scoprirebbe solo dal rifiuto del server al secondo tentativo.
    bool GiaRecensito = false);
