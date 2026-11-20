import { Component, computed, input } from '@angular/core';

/** Varianti cromatiche previste dal design per l'avatar. */
export type AvatarVariante = 'sage' | 'green';

/**
 * Avatar circolare con iniziali.
 *
 * Il design prevede l'avatar fotografico come caso normale e le iniziali su
 * fondo sage come fallback; poiche' il dominio applicativo espone solo il
 * nickname dell'utente (vincolo di privacy del progetto), le iniziali sono di
 * fatto la resa predefinita, e la foto resta opzionale per evoluzioni future.
 *
 * Le taglie ammesse dal sistema sono 32 / 40 / 56 px, piu' i 34 px usati
 * nell'header autenticato del prototipo.
 */
@Component({
  selector: 'app-avatar',
  standalone: true,
  templateUrl: './avatar.html',
  styleUrl: './avatar.scss',
  host: {
    '[style.--avatar-size.px]': 'dimensione()',
    '[class]': '"variante-" + variante()'
  }
})
export class Avatar {
  /** Nome o nickname da cui derivare le iniziali. */
  readonly nome = input.required<string>();

  /** URL di una foto profilo; se assente si ricade sulle iniziali. */
  readonly immagineUrl = input<string | null>(null);

  /** Diametro in pixel. Taglie del design: 32, 40, 56 (34 nell'header). */
  readonly dimensione = input(40);

  /** `sage` per gli avatar in elenco, `green` per quello dell'utente in header. */
  readonly variante = input<AvatarVariante>('sage');

  /**
   * Iniziali: prime lettere delle prime due parole del nome. Su nickname di una
   * sola parola si usa la sola iniziale, come nel prototipo ("SB" / "S").
   */
  readonly iniziali = computed(() => {
    const parole = this.nome().trim().split(/\s+/).filter(Boolean);

    return parole
      .slice(0, 2)
      .map((parola) => parola.charAt(0).toLocaleUpperCase('it-IT'))
      .join('');
  });

  /** La dimensione del testo segue il diametro per mantenere le proporzioni. */
  readonly dimensioneTesto = computed(() => Math.round(this.dimensione() * 0.4));
}
