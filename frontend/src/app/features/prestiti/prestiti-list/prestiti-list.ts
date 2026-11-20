import { Component, inject, signal } from '@angular/core';
import { forkJoin, Observable } from 'rxjs';
import { MatTabsModule } from '@angular/material/tabs';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog } from '@angular/material/dialog';
import { PrestitoService } from '../../../core/services/prestito.service';
import { RecensioneService } from '../../../core/services/recensione.service';
import { PrestitoDto } from '../../../core/models/prestito.model';
import { StatoPrestito } from '../../../core/models/enums';
import { RecensionePrestitoDialog } from '../recensione-prestito-dialog/recensione-prestito-dialog';
import { EmptyState, Skeleton } from '../../../shared/ui';
import { RichiestaCard } from '../richiesta-card/richiesta-card';

/**
 * Richieste e prestiti.
 *
 * Il design disegna solo il lato ricevente, con le sole richieste in attesa.
 * Qui la stessa card copre l'intero ciclo di vita del prestito (accettato,
 * restituito, rifiutato, annullato) su entrambe le direzioni — inviate e
 * ricevute — perche' e' il modello dati del progetto: ogni prestito e' una riga
 * sola che attraversa questi stati, non schermate separate. Le sezioni "Da
 * restituire" e "Fuori casa" del design non hanno quindi una schermata
 * propria: i prestiti attivi si riconoscono dallo stato della card.
 */
@Component({
  selector: 'app-prestiti-list',
  standalone: true,
  imports: [
    MatTabsModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    EmptyState,
    Skeleton,
    RichiestaCard
  ],
  templateUrl: './prestiti-list.html',
  styleUrl: './prestiti-list.scss'
})
export class PrestitiList {
  private readonly prestitoService = inject(PrestitoService);
  private readonly recensioneService = inject(RecensioneService);
  private readonly dialog = inject(MatDialog);

  readonly StatoPrestito = StatoPrestito;

  readonly inviati = signal<PrestitoDto[]>([]);
  readonly ricevuti = signal<PrestitoDto[]>([]);
  readonly loading = signal(false);
  readonly errore = signal(false);
  readonly azioneInCorso = signal<string | null>(null);

  constructor() {
    this.carica();
  }

  /**
   * Ricarica i due elenchi. Dopo un'azione il ricaricamento e' silenzioso:
   * mostrare lo scheletro toglierebbe dal DOM le schede e, ricreandole,
   * riporterebbe chi ha appena accettato una richiesta ricevuta sulla scheda
   * delle richieste inviate.
   */
  carica(silenzioso = false): void {
    if (!silenzioso) {
      this.loading.set(true);
    }
    this.errore.set(false);

    this.prestitoService.getInviati().subscribe({
      next: (inviati) => {
        this.inviati.set(inviati);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.errore.set(true);
      }
    });

    this.prestitoService.getRicevuti().subscribe((ricevuti) => this.ricevuti.set(ricevuti));
  }

  annulla(prestito: PrestitoDto): void {
    this.eseguiAzione(prestito.id, this.prestitoService.annulla(prestito.id));
  }

  accetta(prestito: PrestitoDto): void {
    this.eseguiAzione(prestito.id, this.prestitoService.accetta(prestito.id));
  }

  rifiuta(prestito: PrestitoDto): void {
    this.eseguiAzione(prestito.id, this.prestitoService.rifiuta(prestito.id));
  }

  completa(prestito: PrestitoDto): void {
    this.eseguiAzione(prestito.id, this.prestitoService.completa(prestito.id));
  }

  recensisci(prestito: PrestitoDto, controparteNickname: string): void {
    this.dialog
      .open(RecensionePrestitoDialog, { data: { controparteNickname }, width: '480px' })
      .afterClosed()
      .subscribe((result) => {
        if (!result) {
          return;
        }

        const invii: Observable<unknown>[] = [];

        if (result.controparte) {
          invii.push(
            this.recensioneService.creaRecensioneUtente({
              prestitoId: prestito.id,
              voto: result.controparte.voto,
              commento: result.controparte.commento
            })
          );
        }

        if (result.esperienza) {
          invii.push(
            this.recensioneService.creaRecensioneCopia({
              prestitoId: prestito.id,
              voto: result.esperienza.voto,
              commento: result.esperienza.commento
            })
          );
        }

        if (invii.length === 0) {
          return;
        }

        // Si ricarica quando gli invii sono conclusi: e' il server a dire che
        // il prestito e' recensito, e con questo il pulsante sparisce.
        forkJoin(invii).subscribe({ next: () => this.carica(true), error: () => this.carica(true) });
      });
  }

  statoLabel(stato: StatoPrestito): string {
    switch (stato) {
      case StatoPrestito.Richiesto:
        return 'In attesa';
      case StatoPrestito.Accettato:
        return 'In corso';
      case StatoPrestito.Rifiutato:
        return 'Rifiutato';
      case StatoPrestito.Completato:
        return 'Restituito';
      case StatoPrestito.Annullato:
        return 'Annullato';
    }
  }

  private eseguiAzione(prestitoId: string, observable: Observable<PrestitoDto>): void {
    this.azioneInCorso.set(prestitoId);

    observable.subscribe({
      next: () => {
        this.azioneInCorso.set(null);
        this.carica(true);
      },
      error: () => this.azioneInCorso.set(null)
    });
  }
}
