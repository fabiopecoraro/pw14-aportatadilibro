import { Component, input } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';

/**
 * Stato vuoto.
 *
 * Il design impone una struttura fissa: illustrazione tenue, titolo, una sola
 * riga di spiegazione e **un solo** CTA primario. La regola "mai piu' di
 * un'azione" non e' una semplificazione: uno stato vuoto
 * con due strade equivalenti non orienta l'utente nuovo. Il componente proietta
 * quindi le azioni, ma la responsabilita' di rispettare il vincolo resta a chi
 * lo usa.
 *
 * L'illustrazione predefinita e' il gruppo di tre copertine tratteggiate del
 * prototipo; in alternativa si puo' passare il nome di un'icona Material per i
 * contesti che non parlano di libri (chat vuota, nessun risultato).
 */
@Component({
  selector: 'app-empty-state',
  standalone: true,
  imports: [MatIconModule],
  templateUrl: './empty-state.html',
  styleUrl: './empty-state.scss'
})
export class EmptyState {
  readonly titolo = input.required<string>();

  /** Una sola riga di spiegazione: il design non ne prevede di piu'. */
  readonly descrizione = input('');

  /** Nome di un'icona Material da usare al posto delle copertine tratteggiate. */
  readonly icona = input<string | null>(null);

  /** Riduce i margini verticali per gli stati vuoti dentro pannelli stretti. */
  readonly compatto = input(false);
}
