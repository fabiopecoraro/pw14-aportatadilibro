import { Component, input, output } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';

/**
 * Visualizza un voto da 1 a 5 stelle. In modalita' interattiva permette di
 * selezionare il voto cliccando su una stella, emettendo `votoChange`.
 */
@Component({
  selector: 'app-star-rating',
  standalone: true,
  imports: [MatIconModule],
  templateUrl: './star-rating.html',
  styleUrl: './star-rating.scss'
})
export class StarRating {
  readonly voto = input(0);
  readonly interattivo = input(false);

  /**
   * Nome del gruppo di stelle per le tecnologie assistive. Dove nella stessa
   * schermata i voti sono due va indicato, altrimenti si chiamerebbero entrambi
   * "Voto" e non si capirebbe quale si sta assegnando.
   */
  readonly etichetta = input('Voto');
  readonly votoChange = output<number>();

  readonly stelle = [1, 2, 3, 4, 5];

  seleziona(stella: number): void {
    if (this.interattivo()) {
      this.votoChange.emit(stella);
    }
  }
}
