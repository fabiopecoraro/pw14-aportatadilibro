using APortataDiLibro.Core.Enums;

namespace APortataDiLibro.Database.Entities;

/// <summary>
/// Richiesta/attivazione di prestito di una <see cref="CopiaLibro"/> da parte
/// di un utente diverso dal proprietario. La data di restituzione effettiva
/// e' valorizzata solo alla chiusura del workflow da parte del proprietario.
/// </summary>
public class Prestito
{
    public Guid Id { get; set; }

    public Guid CopiaLibroId { get; set; }

    public Guid RichiedenteId { get; set; }

    public StatoPrestito Stato { get; set; } = StatoPrestito.Richiesto;

    public DateTime DataRichiesta { get; set; }

    public int GiorniStimati { get; set; }

    public DateTime? DataRestituzioneEffettiva { get; set; }

    public CopiaLibro CopiaLibro { get; set; } = null!;

    public Utente Richiedente { get; set; } = null!;
}
