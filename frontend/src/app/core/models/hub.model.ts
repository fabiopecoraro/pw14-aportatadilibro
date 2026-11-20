export interface HubDto {
  id: string;
  nome: string;
  indirizzo: string;
  lat: number;
  lng: number;
}

/**
 * Disponibilita' di una copia presso un singolo Hub del proprietario.
 * Per default una copia e' disponibile presso tutti gli Hub: `escluso`
 * segnala le sole eccezioni configurate dall'utente.
 */
export interface DisponibilitaHubCopiaDto {
  hubId: string;
  nome: string;
  indirizzo: string;
  escluso: boolean;
}

export interface CreaHubRequest {
  nome: string;
  indirizzo: string;
  lat: number;
  lng: number;
}
