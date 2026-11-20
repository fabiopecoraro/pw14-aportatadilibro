import { Component, input } from '@angular/core';

/**
 * Taglie di copertina usate dal design:
 *  - `sm`   46x64  — card compatta del pannello mappa
 *  - `md`   50x70  — riga dei risultati di ricerca
 *  - `lg`   88x124 — box di contesto (chat, richieste)
 *  - `auto` riempie il contenitore mantenendo il rapporto 2:3 (griglia libreria,
 *           dettaglio libro)
 */
export type CopertinaDimensione = 'sm' | 'md' | 'lg' | 'auto';

/**
 * Copertina di un libro.
 *
 * Quando la copia non ha un'immagine caricata si mostra il placeholder a righe
 * diagonali prescritto dal design (token --apl-cover-placeholder) invece di
 * un'icona generica: mantiene il ritmo visivo delle griglie, dove un buco
 * bianco spezzerebbe la lettura dell'elenco.
 */
@Component({
  selector: 'app-book-cover',
  standalone: true,
  templateUrl: './book-cover.html',
  styleUrl: './book-cover.scss',
  host: {
    '[class]': '"dimensione-" + dimensione()'
  }
})
export class BookCover {
  /** URL della copertina (o del thumbnail); `null` attiva il placeholder. */
  readonly url = input<string | null>(null);

  /** Titolo del libro, usato come testo alternativo dell'immagine. */
  readonly titolo = input('');

  readonly dimensione = input<CopertinaDimensione>('sm');
}
