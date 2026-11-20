namespace APortataDiLibro.Core.Enums;

/// <summary>
/// Stato di una proposta di nuovo libro inviata da un utente. Una proposta in
/// attesa deve essere valutata dall'Admin: se approvata genera automaticamente
/// la scheda <see cref="Database.Entities.Libro"/> e la relativa copia nella
/// libreria personale del proponente.
/// </summary>
public enum StatoProposta
{
    InAttesa = 0,
    Approvata = 1,
    Rifiutata = 2
}
