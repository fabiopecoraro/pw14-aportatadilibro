using APortataDiLibro.Core.Enums;

namespace APortataDiLibro.Database.Entities;

/// <summary>
/// Copia fisica di un <see cref="Libro"/> posseduta da un <see cref="Utente"/>.
/// Per default e' disponibile presso tutti gli Hub dell'utente; eventuali
/// eccezioni sono modellate da <see cref="EsclusioneHubLibro"/>.
/// </summary>
public class CopiaLibro
{
    public Guid Id { get; set; }

    public Guid LibroId { get; set; }

    public Guid ProprietarioId { get; set; }

    public string? PercorsoCopertina { get; set; }

    public string? PercorsoCopertinaThumbnail { get; set; }

    public StatoCopia Stato { get; set; } = StatoCopia.Disponibile;

    public Libro Libro { get; set; } = null!;

    public Utente Proprietario { get; set; } = null!;

    public ICollection<Prestito> Prestiti { get; set; } = new List<Prestito>();

    public ICollection<EsclusioneHubLibro> Esclusioni { get; set; } = new List<EsclusioneHubLibro>();
}
