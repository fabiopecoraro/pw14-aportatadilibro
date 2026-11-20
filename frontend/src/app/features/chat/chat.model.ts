import { PrestitoDto } from '../../core/models/prestito.model';
import { StatoPrestito } from '../../core/models/enums';

/**
 * Una conversazione, vista della schermata chat.
 *
 * Il dominio non ha un concetto di "conversazione" indipendente: la chat è
 * legata 1:1 a un prestito (`ChatService.UtenteAutorizzatoAsync` la apre a
 * richiedente e proprietario fin dallo stato `Richiesto`). Questo tipo
 * normalizza un `PrestitoDto` — che arriva da due endpoint diversi, inviati e
 * ricevuti, con nomi di campo speculari — nell'unica forma che serve
 * all'elenco e al thread, così le due sorgenti non trapelano nei componenti.
 *
 * Non esiste un endpoint che restituisca l'ultimo messaggio per prestito,
 * quindi l'elenco non può mostrare l'anteprima del design. Il sottotitolo
 * mostra lo stato del prestito al suo posto, e l'ordinamento usa la data
 * della richiesta come proxy dell'attività più recente.
 */
export interface Conversazione {
  prestitoId: string;
  libroId: string;
  libroTitolo: string;
  controparteId: string;
  controparteNickname: string;
  stato: StatoPrestito;
  dataRichiesta: string;
  giorniStimati: number;
  dataDisponibilitaStimata: string | null;
  dataRestituzioneEffettiva: string | null;
  /** Vero se l'utente corrente è il richiedente (falso: è il proprietario). */
  sonoRichiedente: boolean;
  /** Vero se l'utente corrente ha già recensito questo prestito. */
  giaRecensito: boolean;
}

/** Deriva una `Conversazione` da un prestito inviato (l'utente è il richiedente). */
export function conversazioneDaInviato(prestito: PrestitoDto): Conversazione {
  return {
    prestitoId: prestito.id,
    libroId: prestito.libroId,
    libroTitolo: prestito.libroTitolo,
    controparteId: prestito.proprietarioId,
    controparteNickname: prestito.proprietarioNickname,
    stato: prestito.stato,
    dataRichiesta: prestito.dataRichiesta,
    giorniStimati: prestito.giorniStimati,
    dataDisponibilitaStimata: prestito.dataDisponibilitaStimata,
    dataRestituzioneEffettiva: prestito.dataRestituzioneEffettiva,
    sonoRichiedente: true,
    giaRecensito: prestito.giaRecensito
  };
}

/** Deriva una `Conversazione` da un prestito ricevuto (l'utente è il proprietario). */
export function conversazioneDaRicevuto(prestito: PrestitoDto): Conversazione {
  return {
    prestitoId: prestito.id,
    libroId: prestito.libroId,
    libroTitolo: prestito.libroTitolo,
    controparteId: prestito.richiedenteId,
    controparteNickname: prestito.richiedenteNickname,
    stato: prestito.stato,
    dataRichiesta: prestito.dataRichiesta,
    giorniStimati: prestito.giorniStimati,
    dataDisponibilitaStimata: prestito.dataDisponibilitaStimata,
    dataRestituzioneEffettiva: prestito.dataRestituzioneEffettiva,
    sonoRichiedente: false,
    giaRecensito: prestito.giaRecensito
  };
}
