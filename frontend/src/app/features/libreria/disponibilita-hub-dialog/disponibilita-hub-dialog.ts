import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { HubService } from '../../../core/services/hub.service';
import { DisponibilitaHubCopiaDto } from '../../../core/models/hub.model';

export interface DisponibilitaHubDialogData {
  copiaId: string;
  titolo: string;
}

/**
 * Dialog di configurazione del sistema di esclusioni: mostra tutti gli Hub
 * dell'utente e consente di disattivare quelli presso cui non vuole scambiare
 * questa specifica copia.
 *
 * Ogni toggle corrisponde a una singola chiamata REST (POST per creare
 * l'esclusione, DELETE per rimuoverla) invece che a un salvataggio finale
 * cumulativo: cosi' lo stato del dialog non puo' divergere da quello del
 * server e non serve calcolare un diff. L'aggiornamento e' ottimistico e
 * viene annullato in caso di errore, che l'errorInterceptor segnala gia'
 * globalmente via snackbar.
 */
@Component({
  selector: 'app-disponibilita-hub-dialog',
  standalone: true,
  imports: [
    RouterLink,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatSlideToggleModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './disponibilita-hub-dialog.html',
  styleUrl: './disponibilita-hub-dialog.scss'
})
export class DisponibilitaHubDialog {
  private readonly hubService = inject(HubService);
  private readonly dialogRef = inject(MatDialogRef<DisponibilitaHubDialog, boolean>);
  readonly data = inject<DisponibilitaHubDialogData>(MAT_DIALOG_DATA);

  readonly loading = signal(true);
  readonly errore = signal(false);
  readonly hub = signal<DisponibilitaHubCopiaDto[]>([]);

  /** Id degli Hub con una chiamata in volo, per bloccare il singolo toggle. */
  readonly inAggiornamento = signal<ReadonlySet<string>>(new Set());

  /** Vero se almeno un'esclusione e' stata modificata: il chiamante ricarica. */
  private modificato = false;

  constructor() {
    this.carica();
  }

  carica(): void {
    this.loading.set(true);
    this.errore.set(false);

    this.hubService.getDisponibilitaCopia(this.data.copiaId).subscribe({
      next: (hub) => {
        this.hub.set(hub);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.errore.set(true);
      }
    });
  }

  disponibileIn(): number {
    return this.hub().filter((h) => !h.escluso).length;
  }

  isInAggiornamento(hubId: string): boolean {
    return this.inAggiornamento().has(hubId);
  }

  /** `disponibile` e' lo stato richiesto dal toggle: escluso ne e' il negato. */
  cambiaDisponibilita(hub: DisponibilitaHubCopiaDto, disponibile: boolean): void {
    const escluso = !disponibile;
    this.aggiornaStatoLocale(hub.hubId, escluso);
    this.segnaInAggiornamento(hub.hubId, true);

    const richiesta = escluso
      ? this.hubService.escludiCopiaDaHub(this.data.copiaId, hub.hubId)
      : this.hubService.rimuoviEsclusione(this.data.copiaId, hub.hubId);

    richiesta.subscribe({
      next: () => {
        this.modificato = true;
        this.segnaInAggiornamento(hub.hubId, false);
      },
      error: () => {
        // Ripristina lo stato precedente: la UI non deve mostrare
        // un'esclusione che il server non ha registrato.
        this.aggiornaStatoLocale(hub.hubId, !escluso);
        this.segnaInAggiornamento(hub.hubId, false);
      }
    });
  }

  chiudi(): void {
    this.dialogRef.close(this.modificato);
  }

  private aggiornaStatoLocale(hubId: string, escluso: boolean): void {
    this.hub.update((elenco) => elenco.map((h) => (h.hubId === hubId ? { ...h, escluso } : h)));
  }

  private segnaInAggiornamento(hubId: string, attivo: boolean): void {
    this.inAggiornamento.update((corrente) => {
      const aggiornato = new Set(corrente);

      if (attivo) {
        aggiornato.add(hubId);
      } else {
        aggiornato.delete(hubId);
      }

      return aggiornato;
    });
  }
}
