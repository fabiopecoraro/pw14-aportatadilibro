import { RuoloUtente } from './enums';

export interface LoginRequest {
  username: string;
  password: string;
}

export interface RegisterRequest {
  username: string;
  nickname: string;
  nome: string;
  cognome: string;
  email: string;
  password: string;
  /** Consenso esplicito a informativa privacy e termini: senza, il backend rifiuta la registrazione. */
  accettazioneTermini: boolean;
}

export interface AuthResponse {
  id: string;
  token: string;
  username: string;
  nickname: string;
  ruolo: RuoloUtente;
}
