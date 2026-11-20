import { Component, input } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { PrestitoDto } from '../../../core/models/prestito.model';
import { StatoPrestito } from '../../../core/models/enums';
import { Avatar } from '../../../shared/ui';
import { tempoRelativo } from '../../../core/utils/tempo';

/**
 * Card di una richiesta di prestito, condivisa fra "richieste inviate" e
 * "richieste ricevute". Nel design la card esiste solo lato ricevente,
 * con azioni Accetta/Rifiuta; qui la stessa forma vale anche per le richieste
 * che l'utente ha inviato, dove le azioni disponibili sono altre (Annulla,
 * Segna come restituito). La differenza fra i due casi e' interamente
 * nell'`<ng-content>` delle azioni, cosi' la forma della card — bordo verde per
 * lo stato "in attesa", riquadro del libro, controparte — resta un solo posto.
 */
@Component({
  selector: 'app-richiesta-card',
  standalone: true,
  imports: [DatePipe, RouterLink, MatButtonModule, MatIconModule, MatProgressSpinnerModule, Avatar],
  templateUrl: './richiesta-card.html',
  styleUrl: './richiesta-card.scss',
  host: {
    '[class.in-attesa]': 'prestito().stato === StatoPrestito.Richiesto'
  }
})
export class RichiestaCard {
  readonly prestito = input.required<PrestitoDto>();

  /** Nickname della controparte (proprietario per chi chiede, richiedente per chi presta). */
  readonly controparteNickname = input.required<string>();

  /** Id della controparte, per il link al suo profilo. */
  readonly controparteId = input.required<string>();

  readonly StatoPrestito = StatoPrestito;
  readonly tempoRelativo = tempoRelativo;
}
