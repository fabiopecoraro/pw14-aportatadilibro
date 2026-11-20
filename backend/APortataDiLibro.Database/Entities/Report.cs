using APortataDiLibro.Core.Enums;

namespace APortataDiLibro.Database.Entities;

/// <summary>
/// Segnalazione di un comportamento scorretto avvenuto nella chat di un
/// <see cref="Prestito"/>. Solo le chat associate a un report in stato
/// <see cref="StatoReport.Aperto"/> possono essere ispezionate dall'Admin.
/// </summary>
public class Report
{
    public Guid Id { get; set; }

    public Guid PrestitoId { get; set; }

    public Guid SegnalanteId { get; set; }

    public Guid UtenteSegnalatoId { get; set; }

    public string Motivo { get; set; } = string.Empty;

    public StatoReport Stato { get; set; } = StatoReport.Aperto;

    public DateTime DataCreazione { get; set; }

    public Prestito Prestito { get; set; } = null!;

    public Utente Segnalante { get; set; } = null!;

    public Utente UtenteSegnalato { get; set; } = null!;
}
