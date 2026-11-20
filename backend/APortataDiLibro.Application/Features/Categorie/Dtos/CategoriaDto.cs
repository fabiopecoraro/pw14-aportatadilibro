namespace APortataDiLibro.Application.Features.Categorie.Dtos;

/// <summary>
/// Categoria come compare accanto a un'opera: solo identificativo e nome.
/// </summary>
public record CategoriaDto(Guid Id, string Nome);

/// <summary>
/// Categoria nella schermata di gestione del vocabolario.
/// <para>
/// Espone entrambi i conteggi d'uso, e non le sole opere: una voce puo'
/// essere referenziata solo da una proposta ancora da valutare, e mostrando
/// il solo <c>NumeroLibri</c> l'interfaccia offrirebbe un pulsante Elimina
/// che il server rifiuta sistematicamente.
/// </para>
/// </summary>
public record CategoriaAdminDto(Guid Id, string Nome, int NumeroLibri, int NumeroProposte);

/// <summary>Creazione o rinomina di una voce del vocabolario.</summary>
public record SalvaCategoriaRequest(string Nome);
