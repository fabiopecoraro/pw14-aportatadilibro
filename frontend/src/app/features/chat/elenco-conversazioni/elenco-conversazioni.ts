import { Component, input, output } from '@angular/core';
import { Avatar, EmptyState, Skeleton } from '../../../shared/ui';
import { Conversazione } from '../chat.model';
import { tempoRelativo } from '../../../core/utils/tempo';
import { StatoCollezione } from '../../mappa/mappa.model';

/**
 * Elenco delle conversazioni (colonna sinistra ~320px).
 *
 * Ogni riga mostra avatar, controparte, libro in discussione e stato del
 * prestito al posto dell'anteprima dell'ultimo messaggio, che il backend non
 * espone (vedi nota di gap in chat.model.ts).
 */
@Component({
  selector: 'app-elenco-conversazioni',
  standalone: true,
  imports: [Avatar, EmptyState, Skeleton],
  templateUrl: './elenco-conversazioni.html',
  styleUrl: './elenco-conversazioni.scss'
})
export class ElencoConversazioni {
  readonly conversazioni = input<Conversazione[]>([]);
  readonly stato = input<StatoCollezione>('idle');
  readonly conversazioneSelezionataId = input<string | null>(null);

  readonly seleziona = output<Conversazione>();
  readonly riprova = output<void>();

  readonly tempoRelativo = tempoRelativo;

  statoTesto(stato: Conversazione['stato']): string {
    switch (stato) {
      case 'Richiesto':
        return 'Richiesta in attesa';
      case 'Accettato':
        return 'Prestito in corso';
      case 'Rifiutato':
        return 'Richiesta rifiutata';
      case 'Completato':
        return 'Prestito concluso';
      case 'Annullato':
        return 'Richiesta annullata';
      default:
        return '';
    }
  }
}
