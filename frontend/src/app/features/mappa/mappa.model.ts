import { CategoriaDto } from '../../core/models/categoria.model';
import { HubDisponibilitaDto } from '../../core/models/ricerca.model';

/**
 * Stato di una collezione remota, come previsto dal modello di stato del design:
 * ogni elenco attraversa questi quattro momenti e ognuno ha una resa dedicata
 * (scheletro, contenuto, stato vuoto, errore con "Riprova").
 */
export type StatoCollezione = 'idle' | 'loading' | 'ready' | 'error';

/**
 * Un libro come compare nell'elenco accanto alla mappa.
 *
 * Il design mostra la distanza del libro dall'utente perche' nel prodotto
 * disegnato i libri stanno a casa di chi li presta. Qui i libri sono disponibili
 * presso gli Hub pubblici del proprietario, quindi la distanza mostrata e'
 * quella dell'Hub piu' vicino fra quelli che espongono la copia: e' il punto in
 * cui l'utente andrebbe davvero a ritirarla.
 */
export interface RisultatoLibro {
  libroId: string;
  titolo: string;
  autore: string;
  categorie: CategoriaDto[];
  /** Anno di pubblicazione, se noto per l'opera. */
  anno: number | null;
  /** Hub piu' vicino fra quelli che espongono almeno una copia disponibile. */
  hubPiuVicino: HubDisponibilitaDto;
  /** Distanza dall'Hub piu' vicino; `null` se non c'e' un punto di riferimento. */
  distanza: number | null;
  /** Distanza gia' formattata ("350 m", "1,5 km"), vuota se non calcolabile. */
  distanzaTesto: string;
  /** Numero di Hub oltre al piu' vicino che espongono lo stesso libro. */
  altriHub: number;
}
