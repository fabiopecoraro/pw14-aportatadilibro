import { Component, computed, inject, signal } from '@angular/core';
import { forkJoin, of } from 'rxjs';
import { catchError, switchMap } from 'rxjs/operators';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog } from '@angular/material/dialog';
import { NotificaService } from '../../../core/services/notifica.service';
import { LibroService } from '../../../core/services/libro.service';
import { PrestitoService } from '../../../core/services/prestito.service';
import { CopiaLibroDto } from '../../../core/models/catalogo.model';
import { StatoCopia } from '../../../core/models/enums';
import { RichiediPrestitoDialog } from '../../catalogo/richiedi-prestito-dialog/richiedi-prestito-dialog';
import { BookCard, EmptyState, Skeleton } from '../../../shared/ui';
import { resolveFileUrl } from '../../../core/utils/files';
import { StatoCollezione } from '../../mappa/mappa.model';

/**
 * Voce della lista desideri, arricchita con lo stato reale di
 * disponibilità del titolo.
 *
 * `SottoscrizioneDisponibilitaDto` espone solo titolo e data della richiesta:
 * autore, copertina e disponibilità corrente arrivano da una chiamata al
 * dettaglio del libro per ciascuna voce. È un pattern N+1 accettabile qui
 * perché la lista dei desideri di un utente è tipicamente corta (poche decine
 * di titoli al massimo), a differenza di un elenco paginato.
 *
 * Il design mostra la distanza della copia comparsa ("a 450 m") e filtra per
 * un raggio scelto dall'utente. Il backend notifica quando *una copia
 * qualsiasi* torna disponibile, senza nozione di distanza — qui si mostra
 * quindi solo lo stato disponibile/non disponibile, non la distanza.
 */
interface Desiderio {
  sottoscrizioneId: string;
  libroId: string;
  titolo: string;
  autore: string;
  copertinaUrl: string | null;
  primaCopiaDisponibile: CopiaLibroDto | null;
}

@Component({
  selector: 'app-notifiche-list',
  standalone: true,
  imports: [MatButtonModule, MatIconModule, MatProgressSpinnerModule, BookCard, EmptyState, Skeleton],
  templateUrl: './notifiche-list.html',
  styleUrl: './notifiche-list.scss'
})
export class NotificheList {
  private readonly notificaService = inject(NotificaService);
  private readonly libroService = inject(LibroService);
  private readonly prestitoService = inject(PrestitoService);
  private readonly dialog = inject(MatDialog);

  readonly stato = signal<StatoCollezione>('idle');
  readonly desideri = signal<Desiderio[]>([]);
  readonly annullamentoInCorso = signal<string | null>(null);
  readonly richiestaInCorso = signal<string | null>(null);

  readonly disponibili = computed(() => this.desideri().filter((d) => d.primaCopiaDisponibile));
  readonly inAttesa = computed(() => this.desideri().filter((d) => !d.primaCopiaDisponibile));

  constructor() {
    this.carica();
  }

  carica(): void {
    this.stato.set(this.desideri().length === 0 ? 'loading' : this.stato());

    this.notificaService
      .getSottoscrizioni()
      .pipe(
        switchMap((sottoscrizioni) => {
          if (sottoscrizioni.length === 0) {
            return of([]);
          }

          return forkJoin(
            sottoscrizioni.map((sottoscrizione) =>
              this.libroService.getDettaglio(sottoscrizione.libroId).pipe(
                switchMap((libro) =>
                  of<Desiderio>({
                    sottoscrizioneId: sottoscrizione.id,
                    libroId: sottoscrizione.libroId,
                    titolo: libro.titolo,
                    autore: libro.autore,
                    copertinaUrl: resolveFileUrl(
                      libro.copie.find((c) => c.percorsoCopertinaThumbnail)?.percorsoCopertinaThumbnail
                    ),
                    primaCopiaDisponibile: libro.copie.find((c) => c.stato === StatoCopia.Disponibile) ?? null
                  })
                ),
                // Se il dettaglio di un libro non si riesce a leggere, la voce
                // resta in elenco con i soli dati della sottoscrizione invece
                // di far fallire l'intero caricamento.
                catchError(() =>
                  of<Desiderio>({
                    sottoscrizioneId: sottoscrizione.id,
                    libroId: sottoscrizione.libroId,
                    titolo: sottoscrizione.libroTitolo,
                    autore: '',
                    copertinaUrl: null,
                    primaCopiaDisponibile: null
                  })
                )
              )
            )
          );
        })
      )
      .subscribe({
        next: (desideri) => {
          this.desideri.set(desideri);
          this.stato.set('ready');
        },
        error: () => this.stato.set('error')
      });
  }

  chiediInPrestito(desiderio: Desiderio): void {
    const copia = desiderio.primaCopiaDisponibile;
    if (!copia) {
      return;
    }

    this.dialog
      .open(RichiediPrestitoDialog)
      .afterClosed()
      .subscribe((request) => {
        if (!request) {
          return;
        }

        this.richiestaInCorso.set(desiderio.sottoscrizioneId);

        this.prestitoService.richiediPrestito(copia.id, request).subscribe({
          next: () => {
            this.richiestaInCorso.set(null);
            this.carica();
          },
          error: () => this.richiestaInCorso.set(null)
        });
      });
  }

  rimuovi(desiderio: Desiderio): void {
    this.annullamentoInCorso.set(desiderio.sottoscrizioneId);

    this.notificaService.annullaSottoscrizione(desiderio.libroId).subscribe({
      next: () => {
        this.annullamentoInCorso.set(null);
        this.carica();
      },
      error: () => this.annullamentoInCorso.set(null)
    });
  }
}
