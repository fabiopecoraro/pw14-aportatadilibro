import { Component, input } from '@angular/core';
import { BookCover } from '../book-cover/book-cover';
import { StatoBadge, StatusBadge } from '../status-badge/status-badge';

/**
 * Densita' della card libro. Il design ne usa tre, non intercambiabili:
 *  - `compatta` — pannello elenco della mappa: copertina 46x64, titolo 15.5px
 *  - `riga`     — risultati di ricerca: copertina 50x70, titolo 16.5px, area
 *                 azioni a destra
 *  - `griglia`  — libreria personale: copertina a piena larghezza con badge
 *                 sovrapposto, titolo 14px sotto
 */
export type BookCardVariante = 'compatta' | 'riga' | 'griglia';

/**
 * Card libro, il componente piu' riusato del sistema.
 *
 * Nota di fedelta': il titolo e' in Spectral 600 e non in Playfair. Il design
 * lo descrive in Playfair 700, ma nel prototipo tutte le card libro (mappa,
 * ricerca, libreria) usano il serif di testo; le occorrenze di Playfair a
 * 17-19px sono wordmark e titoli di sezione, non titoli di card. Si segue il
 * prototipo HTML, che e' il riferimento per i dettagli.
 *
 * La card non gestisce la navigazione: espone solo l'aspetto e lo stato di
 * selezione, cosi' il contenitore resta libero di avvolgerla in un link, in un
 * bottone o in un elemento di lista a seconda della semantica della schermata.
 */
@Component({
  selector: 'app-book-card',
  standalone: true,
  imports: [BookCover, StatusBadge],
  templateUrl: './book-card.html',
  styleUrl: './book-card.scss',
  host: {
    '[class]': '"variante-" + variante()',
    '[class.selezionata]': 'selezionata()',
    '[class.interattiva]': 'interattiva()'
  }
})
export class BookCard {
  readonly titolo = input.required<string>();
  readonly autore = input('');

  /** Riga di metadati aggiuntiva: proprietario, distanza, genere, valutazione. */
  readonly meta = input<string | null>(null);

  readonly copertinaUrl = input<string | null>(null);
  readonly variante = input<BookCardVariante>('compatta');

  /** Se valorizzato mostra il badge di stato nella posizione prevista dalla variante. */
  readonly stato = input<StatoBadge | null>(null);

  /** Evidenza di selezione: bordo verde, usata dalla sincronia mappa/elenco. */
  readonly selezionata = input(false);

  /** Applica gli stati hover; da disattivare per le card puramente informative. */
  readonly interattiva = input(true);
}
