namespace APortataDiLibro.Application.Features.Hub.Dtos;

/// <summary>
/// Stato di disponibilita' di una copia presso un singolo Hub del proprietario.
/// Il flag <c>Escluso</c> rende esplicita al frontend una regola che sul
/// database e' implicita: la copia e' disponibile presso tutti gli Hub del
/// proprietario tranne quelli per cui esiste una riga di esclusione. Esporre
/// lo stato "risolto" evita al client di dover ricostruire la differenza tra
/// l'elenco degli Hub e quello delle esclusioni.
/// </summary>
public record DisponibilitaHubCopiaDto(
    Guid HubId,
    string Nome,
    string Indirizzo,
    bool Escluso);
