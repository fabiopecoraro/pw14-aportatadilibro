import { Component, input, output } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { Conversazione } from '../chat.model';
import { StatoPrestito } from '../../../core/models/enums';

/**
 * Pannello di contesto (colonna destra): il libro in discussione e le
 * azioni di prestito disponibili per lo stato corrente. La segnalazione non
 * fa parte del design ma vive qui perché è legata al partecipante della
 * conversazione, non al singolo messaggio.
 */
@Component({
  selector: 'app-pannello-contesto',
  standalone: true,
  imports: [DatePipe, RouterLink, MatButtonModule, MatIconModule, MatProgressSpinnerModule, MatTooltipModule],
  templateUrl: './pannello-contesto.html',
  styleUrl: './pannello-contesto.scss'
})
export class PannelloContesto {
  readonly conversazione = input.required<Conversazione>();
  readonly azioneInCorso = input(false);
  readonly segnalazioneInviata = input(false);

  readonly accetta = output<void>();
  readonly rifiuta = output<void>();
  readonly annulla = output<void>();
  readonly completa = output<void>();
  readonly recensisci = output<void>();
  readonly segnala = output<void>();

  readonly StatoPrestito = StatoPrestito;
}
