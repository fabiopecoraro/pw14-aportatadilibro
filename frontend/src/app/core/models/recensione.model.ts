export interface RecensioneUtenteDto {
  id: string;
  prestitoId: string;
  autoreNickname: string;
  utenteRecensitoNickname: string;
  voto: number;
  commento: string | null;
  dataCreazione: string;
}

export interface RecensioneLibroDto {
  id: string;
  libroId: string;
  autoreNickname: string;
  voto: number;
  commento: string | null;
  dataCreazione: string;
}

export interface RecensioneCopiaDto {
  id: string;
  prestitoId: string;
  autoreNickname: string;
  voto: number;
  commento: string | null;
  dataCreazione: string;
}

export interface CreaRecensioneUtenteRequest {
  prestitoId: string;
  voto: number;
  commento: string | null;
}

export interface CreaRecensioneLibroRequest {
  libroId: string;
  voto: number;
  commento: string | null;
}

export interface CreaRecensioneCopiaRequest {
  prestitoId: string;
  voto: number;
  commento: string | null;
}
