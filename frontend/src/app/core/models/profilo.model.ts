/** Profilo completo dell'utente autenticato, leggibile solo da lui stesso. */
export interface ProfiloDto {
  id: string;
  username: string;
  nickname: string;
  nome: string;
  cognome: string;
  email: string;
  dataRegistrazione: string;
  /** Momento dell'accettazione di informativa privacy e termini. */
  dataAccettazioneTermini: string | null;
}

/** `username` non e' modificabile: e' l'identificativo di accesso. */
export interface AggiornaProfiloRequest {
  nickname: string;
  nome: string;
  cognome: string;
  email: string;
}

export interface CambiaPasswordRequest {
  passwordAttuale: string;
  nuovaPassword: string;
}
