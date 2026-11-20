import { Component, computed, input } from '@angular/core';

/**
 * Stati che il design rappresenta con un badge. Corrispondono agli stati di
 * dominio `StatoCopia` (Disponibile / InPrestito) piu' `richiesto`, usato dove
 * l'utente ha una richiesta di prestito aperta sulla copia.
 */
export type StatoBadge = 'disponibile' | 'in-prestito' | 'richiesto';

/**
 * `overlay` e' il badge sovrapposto alla copertina (fondo pieno, testo bianco);
 * `inline` e' il badge che vive nel flusso del testo (fondo tenue, testo
 * colorato). Il design usa entrambi e non sono intercambiabili: il primo deve
 * reggere il contrasto su un'immagine qualsiasi, il secondo su fondo paper.
 */
export type BadgeVariante = 'overlay' | 'inline';

@Component({
  selector: 'app-status-badge',
  standalone: true,
  templateUrl: './status-badge.html',
  styleUrl: './status-badge.scss',
  host: {
    '[class]': '"stato-" + stato() + " variante-" + variante()'
  }
})
export class StatusBadge {
  readonly stato = input.required<StatoBadge>();
  readonly variante = input<BadgeVariante>('inline');

  /** Etichetta sovrascrivibile, per i casi in cui il contesto chiede altro. */
  readonly etichetta = input<string | null>(null);

  readonly testo = computed(() => {
    const personalizzata = this.etichetta();
    if (personalizzata) {
      return personalizzata;
    }

    switch (this.stato()) {
      case 'disponibile':
        return 'Disponibile';
      case 'in-prestito':
        return 'In prestito';
      case 'richiesto':
        return 'Richiesto';
    }
  });
}
