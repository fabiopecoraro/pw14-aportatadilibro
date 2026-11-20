import { Component, OnDestroy, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog } from '@angular/material/dialog';
import { forkJoin } from 'rxjs';
import { PrestitoService } from '../../../core/services/prestito.service';
import { ChatService } from '../../../core/services/chat.service';
import { RecensioneService } from '../../../core/services/recensione.service';
import { ReportService } from '../../../core/services/report.service';
import { AuthService } from '../../../core/services/auth.service';
import { ChatMessaggioDto } from '../../../core/models/chat.model';
import { StatoPrestito } from '../../../core/models/enums';
import { SegnalaDialog } from '../../prestiti/segnala-dialog/segnala-dialog';
import { RecensionePrestitoDialog } from '../../prestiti/recensione-prestito-dialog/recensione-prestito-dialog';
import { StatoCollezione } from '../../mappa/mappa.model';
import { Conversazione, conversazioneDaInviato, conversazioneDaRicevuto } from '../chat.model';
import { ElencoConversazioni } from '../elenco-conversazioni/elenco-conversazioni';
import { ThreadMessaggi } from '../thread-messaggi/thread-messaggi';
import { PannelloContesto } from '../pannello-contesto/pannello-contesto';

/**
 * Chat: tre colonne su desktop — elenco conversazioni, thread,
 * contesto — colonna singola su mobile, dove la selezione di una
 * conversazione sostituisce l'elenco con thread e contesto insieme (il
 * pannello contesto compare come intestazione compatta, non come colonna).
 *
 * Ogni conversazione è un prestito (vedi chat.model.ts): non esiste un
 * concetto di conversazione indipendente nel dominio. La selezione vive nel
 * query param `?p=<prestitoId>` della rotta `/chat`, che rende la
 * conversazione condivisibile via URL senza far ricreare il componente a ogni cambio — due
 * path distinti per lo stesso componente lo farebbero, perdendo l'elenco
 * gia' caricato e la connessione SignalR attiva.
 */
@Component({
  selector: 'app-chat-shell',
  standalone: true,
  imports: [MatIconModule, ElencoConversazioni, ThreadMessaggi, PannelloContesto],
  templateUrl: './chat-shell.html',
  styleUrl: './chat-shell.scss'
})
export class ChatShell implements OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly prestitoService = inject(PrestitoService);
  private readonly chatService = inject(ChatService);
  private readonly recensioneService = inject(RecensioneService);
  private readonly reportService = inject(ReportService);
  private readonly dialog = inject(MatDialog);
  private readonly authService = inject(AuthService);

  readonly stato = signal<StatoCollezione>('idle');
  readonly conversazioni = signal<Conversazione[]>([]);
  readonly prestitoSelezionatoId = signal<string | null>(null);

  readonly messaggi = signal<ChatMessaggioDto[]>([]);
  readonly caricamentoThread = signal(false);
  readonly erroreThread = signal(false);
  readonly invioInCorso = signal(false);
  readonly azioneInCorso = signal(false);
  readonly segnalazioneInviata = signal(false);

  readonly utenteId = computed(() => this.authService.currentUser()?.id ?? '');

  readonly conversazioneSelezionata = computed(
    () => this.conversazioni().find((c) => c.prestitoId === this.prestitoSelezionatoId()) ?? null
  );

  private prestitoConnesso: string | null = null;

  constructor() {
    this.carica();

    // La selezione segue il query param `p` (non un path param, vedi
    // app.routes.ts): permette il link diretto a una conversazione, mantiene
    // sincronizzati back/forward del browser, e non fa ricreare il
    // componente a ogni cambio.
    this.route.queryParamMap.pipe(takeUntilDestroyed()).subscribe((params) => {
      const id = params.get('p');
      this.prestitoSelezionatoId.set(id);
      this.gestisciSelezione(id);
    });
  }

  ngOnDestroy(): void {
    if (this.prestitoConnesso) {
      void this.chatService.disconnetti();
    }
  }

  carica(): void {
    this.stato.set(this.conversazioni().length === 0 ? 'loading' : this.stato());

    forkJoin({
      inviati: this.prestitoService.getInviati(),
      ricevuti: this.prestitoService.getRicevuti()
    }).subscribe({
      next: ({ inviati, ricevuti }) => {
        const tutte = [...inviati.map(conversazioneDaInviato), ...ricevuti.map(conversazioneDaRicevuto)].sort(
          (a, b) => new Date(b.dataRichiesta).getTime() - new Date(a.dataRichiesta).getTime()
        );

        this.conversazioni.set(tutte);
        this.stato.set('ready');
      },
      error: () => this.stato.set('error')
    });
  }

  seleziona(conversazione: Conversazione): void {
    this.router.navigate(['/chat'], { queryParams: { p: conversazione.prestitoId } });
  }

  tornaAllElenco(): void {
    this.router.navigate(['/chat'], { queryParams: {} });
  }

  riprovaThread(): void {
    this.gestisciSelezione(this.prestitoSelezionatoId());
  }

  inviaMessaggio(testo: string): void {
    const prestitoId = this.prestitoSelezionatoId();
    if (!prestitoId) {
      return;
    }

    this.invioInCorso.set(true);
    this.chatService
      .inviaMessaggio(prestitoId, testo)
      .then(() => this.invioInCorso.set(false))
      .catch(() => {
        // Un invio fallito porta il thread nello stato di errore, che offre
        // di riprovare e con questo riapre la connessione: limitarsi a
        // riabilitare il pulsante lascerebbe credere che il messaggio sia
        // partito.
        this.invioInCorso.set(false);
        this.erroreThread.set(true);
      });
  }

  accetta(): void {
    this.eseguiAzionePrestito((id) => this.prestitoService.accetta(id));
  }

  rifiuta(): void {
    this.eseguiAzionePrestito((id) => this.prestitoService.rifiuta(id));
  }

  annulla(): void {
    this.eseguiAzionePrestito((id) => this.prestitoService.annulla(id));
  }

  completa(): void {
    this.eseguiAzionePrestito((id) => this.prestitoService.completa(id));
  }

  segnala(): void {
    const conversazione = this.conversazioneSelezionata();
    if (!conversazione) {
      return;
    }

    this.dialog
      .open(SegnalaDialog, {
        data: { controparteNickname: conversazione.controparteNickname },
        width: '420px'
      })
      .afterClosed()
      .subscribe((motivo?: string) => {
        if (!motivo) {
          return;
        }

        this.reportService
          .creaReport({
            prestitoId: conversazione.prestitoId,
            utenteSegnalatoId: conversazione.controparteId,
            motivo
          })
          .subscribe(() => this.segnalazioneInviata.set(true));
      });
  }

  recensisci(): void {
    const conversazione = this.conversazioneSelezionata();
    if (!conversazione) {
      return;
    }

    this.dialog
      .open(RecensionePrestitoDialog, {
        data: { controparteNickname: conversazione.controparteNickname },
        width: '480px'
      })
      .afterClosed()
      .subscribe((result) => {
        if (!result) {
          return;
        }

        const invii = [];

        if (result.controparte) {
          invii.push(
            this.recensioneService.creaRecensioneUtente({
              prestitoId: conversazione.prestitoId,
              voto: result.controparte.voto,
              commento: result.controparte.commento
            })
          );
        }

        if (result.esperienza) {
          invii.push(
            this.recensioneService.creaRecensioneCopia({
              prestitoId: conversazione.prestitoId,
              voto: result.esperienza.voto,
              commento: result.esperienza.commento
            })
          );
        }

        if (invii.length === 0) {
          return;
        }

        // Si ricarica a invii conclusi: e' il server a dire che il prestito
        // e' recensito, e con questo il pulsante lascia il posto alla nota.
        forkJoin(invii).subscribe({ next: () => this.carica(), error: () => this.carica() });
      });
  }

  private eseguiAzionePrestito(azione: (prestitoId: string) => ReturnType<PrestitoService['accetta']>): void {
    const prestitoId = this.prestitoSelezionatoId();
    if (!prestitoId) {
      return;
    }

    this.azioneInCorso.set(true);
    azione(prestitoId).subscribe({
      next: () => {
        this.azioneInCorso.set(false);
        this.carica();
      },
      error: () => this.azioneInCorso.set(false)
    });
  }

  private gestisciSelezione(prestitoId: string | null): void {
    // Cambiare conversazione lascia il gruppo SignalR precedente e ne apre
    // uno nuovo: la chat e' segmentata per prestito lato hub (JoinPrestitoGroup),
    // quindi restare iscritti al gruppo vecchio continuerebbe a consegnare
    // messaggi per una chat che non e' piu' in vista.
    if (this.prestitoConnesso) {
      void this.chatService.disconnetti();
      this.prestitoConnesso = null;
    }

    this.messaggi.set([]);
    this.erroreThread.set(false);
    this.segnalazioneInviata.set(false);

    if (!prestitoId) {
      return;
    }

    this.caricamentoThread.set(true);

    this.chatService.getCronologia(prestitoId).subscribe({
      next: (cronologia) => {
        this.messaggi.set(cronologia);
        this.caricamentoThread.set(false);
      },
      error: () => {
        this.caricamentoThread.set(false);
        this.erroreThread.set(true);
      }
    });

    this.chatService
      .connetti(prestitoId, (messaggio) => {
        this.messaggi.update((correnti) => [...correnti, messaggio]);
      })
      .catch(() => this.erroreThread.set(true));

    this.prestitoConnesso = prestitoId;
  }
}
