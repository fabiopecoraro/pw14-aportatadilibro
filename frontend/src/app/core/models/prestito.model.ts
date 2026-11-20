import { StatoPrestito } from './enums';

export interface PrestitoDto {
  id: string;
  copiaLibroId: string;
  libroId: string;
  libroTitolo: string;
  richiedenteId: string;
  richiedenteNickname: string;
  proprietarioId: string;
  proprietarioNickname: string;
  stato: StatoPrestito;
  dataRichiesta: string;
  giorniStimati: number;
  dataRestituzioneEffettiva: string | null;
  dataDisponibilitaStimata: string | null;
  /** Vero se l'utente corrente ha già lasciato una recensione per questo prestito. */
  giaRecensito: boolean;
}

export interface RichiediPrestitoRequest {
  giorniStimati: number;
}
