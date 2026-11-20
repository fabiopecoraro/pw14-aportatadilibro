import { Component, computed, input } from '@angular/core';

/**
 * Forme di scheletro previste dal design:
 *  - `card-compatta` — copertina 46x64 + due righe (pannello mappa, elenchi)
 *  - `card-griglia`  — copertina a piena larghezza + due righe (libreria)
 *  - `riga`          — una barra di testo
 *  - `blocco`        — un rettangolo generico (mappa, pannelli)
 */
export type SkeletonForma = 'card-compatta' | 'card-griglia' | 'riga' | 'blocco';

/**
 * Scheletro di caricamento.
 *
 * Il design prescrive lo skeleton **solo per il primo caricamento** di una
 * collezione (lo spinner resta per le azioni puntuali) e chiede che la forma
 * ricalchi quella del contenuto reale: e' cio' che evita lo scatto di layout
 * quando i dati arrivano, ed e' il motivo per cui qui le forme sono enumerate
 * invece di essere un rettangolo parametrico.
 */
@Component({
  selector: 'app-skeleton',
  standalone: true,
  templateUrl: './skeleton.html',
  styleUrl: './skeleton.scss',
  host: {
    role: 'status',
    'aria-busy': 'true',
    'aria-label': 'Caricamento in corso',
    '[class]': '"forma-" + forma()'
  }
})
export class Skeleton {
  readonly forma = input<SkeletonForma>('card-compatta');

  /** Numero di elementi da ripetere, per riempire un elenco in attesa. */
  readonly ripetizioni = input(1);

  /** Altezza in pixel, usata solo dalla forma `blocco`. */
  readonly altezza = input(120);

  readonly elementi = computed(() => Array.from({ length: this.ripetizioni() }, (_, i) => i));
}
