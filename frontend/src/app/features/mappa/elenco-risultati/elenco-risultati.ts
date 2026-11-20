import { Component, ElementRef, effect, input, output, viewChild } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { BookCard, EmptyState, Skeleton } from '../../../shared/ui';
import { RisultatoLibro, StatoCollezione } from '../mappa.model';

/**
 * Elenco dei libri disponibili nell'area di mappa visibile.
 *
 * Copre da solo i quattro stati della collezione: scheletro al primo
 * caricamento, elenco, stato vuoto e stato di errore con "Riprova". Il design
 * chiede che questi stati vengano costruiti insieme alla schermata e non
 * aggiunti alla fine, ed e' il motivo per cui vivono qui dentro invece che
 * essere condizioni sparse nella pagina.
 */
@Component({
  selector: 'app-elenco-risultati',
  standalone: true,
  imports: [MatButtonModule, BookCard, EmptyState, Skeleton],
  templateUrl: './elenco-risultati.html',
  styleUrl: './elenco-risultati.scss'
})
export class ElencoRisultati {
  readonly risultati = input<RisultatoLibro[]>([]);
  readonly stato = input<StatoCollezione>('idle');

  /** Libro correntemente selezionato: riceve il bordo verde. */
  readonly libroSelezionatoId = input<string | null>(null);

  /**
   * Libro da portare in vista. Cambia quando la selezione arriva dalla mappa:
   * il click su un pin deve far comparire la card corrispondente nell'elenco.
   */
  readonly libroDaScorrere = input<string | null>(null);

  readonly libroSelezionato = output<RisultatoLibro>();
  readonly libroEvidenziato = output<string | null>();
  readonly riprova = output<void>();

  private readonly contenitore = viewChild<ElementRef<HTMLElement>>('elenco');

  /**
   * Riga secondaria della card compatta: autore, anno se noto, e Hub di
   * ritiro. Vive qui e non nel template perche' la concatenazione
   * condizionale dell'anno renderebbe l'espressione inline illeggibile.
   */
  descrizione(risultato: RisultatoLibro): string {
    const autore = risultato.anno ? `${risultato.autore} (${risultato.anno})` : risultato.autore;
    return `${autore} · ${risultato.hubPiuVicino.nome}`;
  }

  constructor() {
    // Lo scorrimento e' un effetto sul cambio di `libroDaScorrere` e non una
    // chiamata dal genitore: cosi' resta valido anche quando la card entra nel
    // DOM dopo la selezione, per esempio subito dopo un cambio di filtro.
    effect(() => {
      const id = this.libroDaScorrere();
      const elemento = this.contenitore()?.nativeElement;
      if (!id || !elemento) {
        return;
      }

      const card = elemento.querySelector<HTMLElement>(`[data-libro-id="${id}"]`);
      card?.scrollIntoView({ block: 'nearest', behavior: 'smooth' });
    });
  }
}
