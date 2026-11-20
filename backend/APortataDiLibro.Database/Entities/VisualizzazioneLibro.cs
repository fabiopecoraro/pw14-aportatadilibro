namespace APortataDiLibro.Database.Entities;

/// <summary>
/// Conteggio giornaliero delle aperture della scheda di un <see cref="Libro"/>,
/// usato per la metrica d'uso "visualizzazioni" richiesta dalla traccia.
/// <para>
/// Il modello e' volutamente aggregato per (Libro, Giorno) e non una riga per
/// singola visita: non conserva ne' l'identita' di chi ha guardato la scheda
/// ne' l'orario esatto, coerentemente con l'impostazione privacy-first del
/// progetto, e mantiene la tabella limitata a libri x giorni invece di
/// crescere con il traffico. L'aggregazione per giorno e' comunque
/// sufficiente a ricavare sia il totale sia l'andamento nel tempo.
/// </para>
/// </summary>
public class VisualizzazioneLibro
{
    public Guid LibroId { get; set; }

    /// <summary>Giorno di riferimento (UTC), granularita' della metrica.</summary>
    public DateOnly Giorno { get; set; }

    public int Conteggio { get; set; }

    public Libro Libro { get; set; } = null!;
}
