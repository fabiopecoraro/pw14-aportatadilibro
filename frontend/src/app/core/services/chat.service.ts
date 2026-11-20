import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ChatMessaggioDto } from '../models/chat.model';
import { AuthService } from './auth.service';

/**
 * Client per la chat privata di un prestito: cronologia via HTTP e messaggi
 * in tempo reale via SignalR (ChatHub), disponibile fin dal momento della
 * richiesta di prestito per i suoi due partecipanti.
 */
@Injectable({ providedIn: 'root' })
export class ChatService {
  private readonly http = inject(HttpClient);
  private readonly authService = inject(AuthService);

  private connection: signalR.HubConnection | null = null;

  getCronologia(prestitoId: string): Observable<ChatMessaggioDto[]> {
    return this.http.get<ChatMessaggioDto[]>(`${environment.apiUrl}/prestiti/${prestitoId}/messaggi`);
  }

  /**
   * Apre la connessione per un prestito, chiudendo prima quella eventualmente
   * aperta: la connessione e' una per conversazione, cosi' i messaggi di una
   * chat non piu' in vista smettono di arrivare.
   *
   * Dopo ogni attesa si ricontrolla di essere ancora la connessione corrente.
   * Chi scorre in fretta l'elenco delle conversazioni fa partire una seconda
   * connessione mentre la prima sta ancora negoziando: senza il controllo, la
   * prima finirebbe per iscriversi al gruppo di una chat gia' abbandonata.
   */
  async connetti(prestitoId: string, onMessaggio: (messaggio: ChatMessaggioDto) => void): Promise<void> {
    void this.disconnetti();

    const connessione = new signalR.HubConnectionBuilder()
      .withUrl(environment.chatHubUrl, {
        accessTokenFactory: () => this.authService.getToken() ?? ''
      })
      .withAutomaticReconnect()
      .build();

    this.connection = connessione;
    connessione.on('ReceiveMessage', onMessaggio);

    try {
      await connessione.start();
      if (this.connection !== connessione) {
        return;
      }
      await connessione.invoke('JoinPrestitoGroup', prestitoId);
    } catch (errore) {
      // Un avvio interrotto perche' nel frattempo si e' passati a un'altra
      // conversazione non e' un guasto e non va mostrato come tale.
      if (this.connection === connessione) {
        this.connection = null;
        throw errore;
      }
    }
  }

  /**
   * Senza connessione l'invio fallisce in modo esplicito: ignorarlo in
   * silenzio farebbe sparire il testo dal campo di scrittura lasciando
   * credere a chi scrive che il messaggio sia partito.
   */
  async inviaMessaggio(prestitoId: string, testo: string): Promise<void> {
    if (!this.connection) {
      throw new Error('Connessione alla chat non disponibile.');
    }

    await this.connection.invoke('SendMessage', prestitoId, testo);
  }

  /**
   * Il riferimento si prende e si azzera prima di attendere la chiusura: chi
   * chiama non aspetta, e subito dopo apre la connessione successiva. Leggendo
   * il campo dopo l'attesa si chiuderebbe quella nuova al posto della vecchia.
   * Chiudere la connessione basta a uscire dal gruppo del prestito, perche' il
   * server scioglie da se' le iscrizioni di una connessione terminata.
   */
  async disconnetti(): Promise<void> {
    const connessione = this.connection;
    if (!connessione) {
      return;
    }

    this.connection = null;
    await connessione.stop();
  }
}
