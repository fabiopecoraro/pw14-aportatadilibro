import { Component, computed, effect, ElementRef, input, output, viewChild } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ChatMessaggioDto } from '../../../core/models/chat.model';
import { EmptyState, Skeleton } from '../../../shared/ui';

/** Un separatore di data o un messaggio, nell'ordine in cui vanno mostrati. */
type VoceThread = { tipo: 'data'; etichetta: string } | { tipo: 'messaggio'; messaggio: ChatMessaggioDto };

/**
 * Thread di messaggi: bolle in uscita/entrata con separatori di
 * data, composer fisso in basso. Puramente presentazionale — la connessione
 * SignalR e il caricamento della cronologia restano nel componente che
 * orchestra la selezione della conversazione, perché cambiano insieme a
 * quale prestito è selezionato, non insieme al thread in sé.
 */
@Component({
  selector: 'app-thread-messaggi',
  standalone: true,
  imports: [DatePipe, FormsModule, MatButtonModule, MatIconModule, MatProgressSpinnerModule, EmptyState, Skeleton],
  templateUrl: './thread-messaggi.html',
  styleUrl: './thread-messaggi.scss'
})
export class ThreadMessaggi {
  readonly messaggi = input<ChatMessaggioDto[]>([]);
  readonly loading = input(false);

  /**
   * Vero se il caricamento della cronologia è fallito. Distinto da "nessun
   * messaggio": senza questo segnale, una richiesta caduta e una
   * conversazione appena iniziata sarebbero indistinguibili — l'utente
   * leggerebbe "scrivi per iniziare" quando in realtà la rete ha fallito.
   */
  readonly errore = input(false);

  readonly utenteId = input.required<string>();
  readonly invioInCorso = input(false);

  readonly invia = output<string>();
  readonly riprova = output<void>();

  private readonly contenitore = viewChild<ElementRef<HTMLElement>>('contenitore');

  protected testoBozza = '';

  readonly vociThread = computed<VoceThread[]>(() => {
    const voci: VoceThread[] = [];
    let ultimaData = '';

    for (const messaggio of this.messaggi()) {
      const etichetta = this.formattaGiorno(messaggio.dataInvio);
      if (etichetta !== ultimaData) {
        voci.push({ tipo: 'data', etichetta });
        ultimaData = etichetta;
      }
      voci.push({ tipo: 'messaggio', messaggio });
    }

    return voci;
  });

  constructor() {
    // Scorre in fondo a ogni nuovo messaggio (arrivato o inviato). L'effetto
    // reagisce alla lunghezza dell'elenco piuttosto che a un evento esplicito,
    // cosi' copre sia i messaggi ricevuti via SignalR sia quelli inviati.
    effect(() => {
      this.messaggi().length;
      const elemento = this.contenitore()?.nativeElement;
      if (elemento) {
        queueMicrotask(() => (elemento.scrollTop = elemento.scrollHeight));
      }
    });
  }

  invioMessaggio(): void {
    const testo = this.testoBozza.trim();
    if (!testo) {
      return;
    }

    this.invia.emit(testo);
    this.testoBozza = '';
  }

  private formattaGiorno(dataIso: string): string {
    const data = new Date(dataIso);
    const oggi = new Date();
    const ieri = new Date(oggi);
    ieri.setDate(oggi.getDate() - 1);

    const stessoGiorno = (a: Date, b: Date) =>
      a.getFullYear() === b.getFullYear() && a.getMonth() === b.getMonth() && a.getDate() === b.getDate();

    if (stessoGiorno(data, oggi)) {
      return 'Oggi';
    }
    if (stessoGiorno(data, ieri)) {
      return 'Ieri';
    }

    return data.toLocaleDateString('it-IT', { day: 'numeric', month: 'long' });
  }
}
