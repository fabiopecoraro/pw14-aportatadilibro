import { Component, input, output } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { CopiaLibroDto } from '../../../../core/models/catalogo.model';
import { Avatar } from '../../../../shared/ui';

/**
 * Pannello delle azioni della scheda libro: la colonna di destra.
 *
 * Nel design il pannello descrive **la** persona che presta il libro, perche'
 * li' ogni libro ha un solo proprietario. Nel nostro dominio lo stesso titolo
 * puo' avere piu' copie di proprietari diversi, quindi il pannello mette in
 * evidenza la prima copia disponibile — quella su cui agisce il pulsante
 * primario — e l'elenco completo resta nella colonna centrale, dove ogni copia
 * ha la propria azione.
 *
 * Restano fuori il riquadro con la distanza e la mini-mappa presenti nel
 * disegno: richiederebbero le coordinate degli Hub del proprietario, che il
 * dettaglio del libro non espone. Sono un'estensione del contratto API, non una
 * scelta di grafica.
 */
@Component({
  selector: 'app-pannello-prestito',
  standalone: true,
  imports: [DatePipe, RouterLink, MatButtonModule, MatIconModule, MatProgressSpinnerModule, Avatar],
  templateUrl: './pannello-prestito.html',
  styleUrl: './pannello-prestito.scss'
})
export class PannelloPrestito {
  /** Prima copia disponibile di un altro utente, se esiste. */
  readonly copiaDisponibile = input<CopiaLibroDto | null>(null);

  /** Numero totale di copie disponibili nel sistema. */
  readonly copieDisponibili = input(0);

  /** Prima data di rientro stimata, quando non c'e' nessuna copia libera. */
  readonly primoRientro = input<string | null>(null);

  /** Vero se l'utente e' gia' iscritto all'avviso di disponibilita'. */
  readonly avvisoAttivo = input(false);

  readonly azioneInCorso = input(false);

  readonly chiediPrestito = output<CopiaLibroDto>();
  readonly avvisami = output<void>();
  readonly aggiungiCopia = output<void>();
}
