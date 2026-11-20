import { Component, computed, input } from '@angular/core';

/** Una barra: etichetta della categoria e valore misurato. */
export interface BarraGrafico {
  etichetta: string;
  valore: number;
}

/**
 * Grafico a barre orizzontali per confronti fra categorie
 * (libri più visti, prestiti per stato).
 *
 * Orizzontale e non verticale per una ragione di leggibilità: le etichette
 * sono titoli di libro, che in verticale andrebbero ruotati o troncati.
 *
 * Non è SVG ma HTML con larghezze percentuali: per barre orizzontali il
 * layout del browser fa già tutto il lavoro, e in cambio le etichette
 * restano testo reale — selezionabile, ricercabile e leggibile da uno screen
 * reader senza descrizioni alternative. La semantica è quella di una lista di
 * coppie etichetta/valore, che è esattamente ciò che il grafico rappresenta.
 */
@Component({
  selector: 'app-chart-barre',
  standalone: true,
  templateUrl: './chart-barre.html',
  styleUrl: './chart-barre.scss'
})
export class ChartBarre {
  readonly barre = input<BarraGrafico[]>([]);

  /** Messaggio mostrato quando non c'è nulla da rappresentare. */
  readonly testoVuoto = input('Nessun dato disponibile.');

  readonly vuoto = computed(() => this.barre().every((b) => b.valore === 0));

  // Il massimo determina la scala: la barra più alta occupa tutta la
  // larghezza, le altre sono proporzionate a essa. Mai zero, altrimenti la
  // percentuale sarebbe NaN.
  private readonly massimo = computed(() => Math.max(1, ...this.barre().map((b) => b.valore)));

  readonly righe = computed(() =>
    this.barre().map((barra) => ({
      ...barra,
      percentuale: (barra.valore / this.massimo()) * 100
    }))
  );
}
