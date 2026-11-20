import { Component, computed, input } from '@angular/core';

/** Un punto della serie: etichetta sull'asse X e valore sull'asse Y. */
export interface PuntoGrafico {
  etichetta: string;
  valore: number;
}

/**
 * Grafico a linea con area riempita per una serie temporale.
 *
 * È SVG scritto a mano invece di una libreria di charting: servono due tipi di
 * grafico su una sola schermata, e una dipendenza di charting peserebbe
 * decisamente più del codice che segue, portandosi dietro un proprio sistema
 * di temi da riconciliare con i token del design. Qui i colori escono dalle
 * custom properties come in ogni altro componente.
 *
 * Il `viewBox` è a coordinate fisse e l'SVG scala con il contenitore
 * (`width: 100%`), quindi il componente non ha bisogno di misurare il DOM né
 * di reagire al ridimensionamento.
 *
 * Accessibilità: un grafico non è leggibile da uno screen reader, quindi
 * l'SVG è `role="img"` con un `<title>`/`<desc>` che riassumono l'andamento in
 * parole, ed è marcato `aria-hidden` per la parte decorativa.
 */
@Component({
  selector: 'app-chart-linea',
  standalone: true,
  templateUrl: './chart-linea.html',
  styleUrl: './chart-linea.scss'
})
export class ChartLinea {
  readonly punti = input<PuntoGrafico[]>([]);

  /** Titolo accessibile del grafico, es. "Visualizzazioni per giorno". */
  readonly titolo = input.required<string>();

  /** Nome dell'unità misurata, usato nella descrizione testuale. */
  readonly unita = input('eventi');

  // Coordinate interne del viewBox. Non sono pixel: l'SVG viene scalato dal
  // CSS, quindi contano solo le proporzioni.
  protected readonly larghezza = 600;
  protected readonly altezza = 160;
  private readonly margineAlto = 8;
  private readonly margineBasso = 8;

  readonly vuoto = computed(() => this.punti().length === 0);

  /** Il massimo non scende mai sotto 1, altrimenti una serie tutta a zero dividerebbe per zero. */
  private readonly massimo = computed(() => Math.max(1, ...this.punti().map((p) => p.valore)));

  readonly coordinate = computed(() => {
    const punti = this.punti();
    if (punti.length === 0) {
      return [];
    }

    const massimo = this.massimo();
    const utile = this.altezza - this.margineAlto - this.margineBasso;

    // Con un solo punto il passo sarebbe una divisione per zero: lo si
    // posiziona al centro invece che a sinistra.
    const passo = punti.length > 1 ? this.larghezza / (punti.length - 1) : 0;

    return punti.map((punto, indice) => ({
      x: punti.length > 1 ? indice * passo : this.larghezza / 2,
      y: this.margineAlto + utile - (punto.valore / massimo) * utile,
      punto
    }));
  });

  readonly percorsoLinea = computed(() =>
    this.coordinate()
      .map((c, i) => `${i === 0 ? 'M' : 'L'}${c.x.toFixed(2)},${c.y.toFixed(2)}`)
      .join(' ')
  );

  /** L'area chiude la linea sulla base del grafico per dare peso visivo alla serie. */
  readonly percorsoArea = computed(() => {
    const coordinate = this.coordinate();
    if (coordinate.length === 0) {
      return '';
    }

    const base = this.altezza - this.margineBasso;
    const primo = coordinate[0];
    const ultimo = coordinate[coordinate.length - 1];

    return `${this.percorsoLinea()} L${ultimo.x.toFixed(2)},${base} L${primo.x.toFixed(2)},${base} Z`;
  });

  readonly totale = computed(() => this.punti().reduce((somma, p) => somma + p.valore, 0));

  readonly descrizione = computed(() => {
    const punti = this.punti();
    if (punti.length === 0) {
      return 'Nessun dato disponibile per il periodo.';
    }

    const massimo = punti.reduce((a, b) => (b.valore > a.valore ? b : a));

    return `${this.totale()} ${this.unita()} in ${punti.length} giorni. ` +
      `Giorno con il valore più alto: ${massimo.etichetta}, ${massimo.valore}.`;
  });

  /** Prima e ultima etichetta: bastano a datare la finestra senza affollare l'asse. */
  readonly etichettaIniziale = computed(() => this.punti()[0]?.etichetta ?? '');
  readonly etichettaFinale = computed(() => this.punti()[this.punti().length - 1]?.etichetta ?? '');
  readonly valoreMassimo = computed(() => this.massimo());
}
