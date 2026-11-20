using APortataDiLibro.Core.Enums;

namespace APortataDiLibro.Database.Entities;

/// <summary>
/// Proposta di inserimento di un nuovo libro nel catalogo, presentata da un
/// utente dalla propria libreria personale quando il titolo cercato non e'
/// presente. Resta in stato <see cref="StatoProposta.InAttesa"/> finche'
/// l'Admin non la valuta: l'approvazione crea automaticamente la scheda
/// <see cref="Libro"/> condivisa e una <see cref="CopiaLibro"/> per il
/// proponente, garantendo la tracciabilita' di chi ha inserito il libro.
/// </summary>
public class PropostaLibro
{
    public Guid Id { get; set; }

    public string Titolo { get; set; } = string.Empty;

    public string Autore { get; set; } = string.Empty;

    /// <summary>
    /// Anno di pubblicazione proposto. Nullable come sul <see cref="Libro"/>
    /// di destinazione: un anno incerto va lasciato vuoto invece che
    /// inventato, e l'Admin puo' comunque correggerlo dopo l'approvazione.
    /// </summary>
    public int? Anno { get; set; }

    public string? Isbn { get; set; }

    public Guid ProponenteId { get; set; }

    public StatoProposta Stato { get; set; } = StatoProposta.InAttesa;

    public DateTime DataProposta { get; set; }

    public DateTime? DataGestione { get; set; }

    public Guid? LibroCreatoId { get; set; }

    public Utente Proponente { get; set; } = null!;

    public Libro? LibroCreato { get; set; }

    public ICollection<PropostaLibroCategoria> Categorie { get; set; } = new List<PropostaLibroCategoria>();
}
