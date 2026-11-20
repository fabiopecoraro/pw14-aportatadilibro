import { Component, computed, input, model } from '@angular/core';
import { CategoriaDto } from '../../../core/models/categoria.model';

/**
 * Selettore multiplo di categorie dal vocabolario controllato.
 *
 * È una griglia di chip a due stati e non una `<select multiple>` o un
 * autocomplete: il vocabolario è un elenco chiuso e corto (poche decine di
 * voci), quindi mostrarlo per intero è più veloce che farlo cercare, e rende
 * evidente a colpo d'occhio che *non* si può inventare una voce nuova — che è
 * esattamente il punto del vocabolario controllato.
 *
 * Il valore è un array di identificativi e non di oggetti: è la forma che le
 * richieste al backend usano, e tenerla qui evita a ogni form di ricavarla.
 *
 * Accessibilità: le chip sono `<button aria-pressed>` dentro un gruppo
 * etichettato, cioè la semantica di un insieme di interruttori. Un
 * `role="listbox"` richiederebbe la gestione delle frecce direzionali senza
 * dare nulla in cambio qui, dove Tab attraversa un elenco breve.
 */
@Component({
  selector: 'app-selettore-categorie',
  standalone: true,
  templateUrl: './selettore-categorie.html',
  styleUrl: './selettore-categorie.scss'
})
export class SelettoreCategorie {
  readonly categorie = input<CategoriaDto[]>([]);

  /** Identificativi selezionati. Modello bidirezionale. */
  readonly selezione = model<string[]>([]);

  /** Oltre questo numero di voci le chip smettono di rispondere. */
  readonly massimo = input(5);

  /** Mostra il conteggio e disabilita le chip non ancora scelte al limite. */
  readonly limiteRaggiunto = computed(() => this.selezione().length >= this.massimo());

  readonly vuoto = computed(() => this.categorie().length === 0);

  selezionata(id: string): boolean {
    return this.selezione().includes(id);
  }

  /** Disabilitata solo se aggiungerne un'altra sforerebbe: le già scelte restano sempre togliibili. */
  disabilitata(id: string): boolean {
    return this.limiteRaggiunto() && !this.selezionata(id);
  }

  commuta(id: string): void {
    const corrente = this.selezione();

    if (corrente.includes(id)) {
      this.selezione.set(corrente.filter((x) => x !== id));
      return;
    }

    if (corrente.length >= this.massimo()) {
      return;
    }

    this.selezione.set([...corrente, id]);
  }
}
