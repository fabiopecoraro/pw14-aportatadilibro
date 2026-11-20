namespace APortataDiLibro.Database.Entities;

/// <summary>
/// Tabella di giunzione che modella l'eccezione "Copia X non disponibile presso Hub Y",
/// usata per derogare alla regola di default secondo cui ogni copia e' disponibile
/// presso tutti gli Hub registrati dal proprietario.
/// </summary>
public class EsclusioneHubLibro
{
    public Guid CopiaLibroId { get; set; }

    public Guid HubId { get; set; }

    public CopiaLibro CopiaLibro { get; set; } = null!;

    public Hub Hub { get; set; } = null!;
}
