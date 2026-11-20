import { Component, OnDestroy, signal } from '@angular/core';
import { NavigationEnd, Router, RouterLink, RouterOutlet } from '@angular/router';
import { Subscription, filter } from 'rxjs';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { AuthService } from '../../core/services/auth.service';
import { PrestitoService } from '../../core/services/prestito.service';
import { StatoPrestito } from '../../core/models/enums';
import { AppHeader } from '../app-header/app-header';
import { AppTabbar } from '../app-tabbar/app-tabbar';

/**
 * Shell dell'applicazione autenticata.
 *
 * Il design prevede due chrome distinti e non una sidenav: un header
 * orizzontale da 640px in su e una tab bar fissa in basso sotto quella soglia.
 * La shell non li disegna: li orchestra, tenendo per se' lo stato condiviso
 * (il conteggio delle richieste) e il menu delle sezioni secondarie, che
 * entrambe le barre riusano.
 */
@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [RouterOutlet, RouterLink, MatIconModule, MatMenuModule, AppHeader, AppTabbar],
  templateUrl: './shell.html',
  styleUrl: './shell.scss'
})
export class Shell implements OnDestroy {
  /**
   * Richieste di prestito ricevute e ancora in attesa di risposta: e' il numero
   * mostrato dal badge terracotta. Si usa questo dato, e non un totale di
   * notifiche, perche' e' l'unico che corrisponde a qualcosa da fare: un badge
   * che non rimanda a un'azione pendente smette presto di essere guardato.
   */
  readonly richiesteInAttesa = signal(0);

  /**
   * Vero quando la schermata corrente chiede l'intera area disponibile invece
   * della colonna centrata a 1180px. La rotta lo dichiara con
   * `data: { pienaLarghezza: true }`: cosi' la scelta resta accanto alla
   * definizione della schermata e la shell non deve conoscere un elenco di
   * percorsi speciali.
   */
  readonly paginaPiena = signal(false);

  private readonly sottoscrizioneNavigazione: Subscription;

  constructor(
    readonly authService: AuthService,
    private readonly prestitoService: PrestitoService,
    private readonly router: Router
  ) {
    this.aggiornaRichieste();

    // Il conteggio viene riletto a ogni navigazione completata: e' una chiamata
    // leggera e garantisce che il badge non resti indietro dopo che l'utente ha
    // accettato o rifiutato una richiesta.
    this.sottoscrizioneNavigazione = this.router.events
      .pipe(filter((evento) => evento instanceof NavigationEnd))
      .subscribe(() => {
        this.aggiornaRichieste();
        this.paginaPiena.set(this.leggiPienaLarghezza());
      });

    this.paginaPiena.set(this.leggiPienaLarghezza());
  }

  ngOnDestroy(): void {
    this.sottoscrizioneNavigazione.unsubscribe();
  }

  logout(): void {
    this.authService.logout();
    this.router.navigateByUrl('/login');
  }

  /**
   * Legge il flag sulla rotta piu' profonda attualmente attiva.
   *
   * Lo snapshot puo' non esistere ancora quando il metodo viene chiamato dal
   * costruttore, cioe' mentre la prima navigazione e' in corso: in quel caso si
   * resta sul valore predefinito e il primo NavigationEnd corregge subito.
   */
  private leggiPienaLarghezza(): boolean {
    let rotta = this.router.routerState.root;
    while (rotta.firstChild) {
      rotta = rotta.firstChild;
    }

    return rotta.snapshot?.data?.['pienaLarghezza'] === true;
  }

  private aggiornaRichieste(): void {
    this.prestitoService.getRicevuti().subscribe({
      next: (prestiti) =>
        this.richiesteInAttesa.set(
          prestiti.filter((prestito) => prestito.stato === StatoPrestito.Richiesto).length
        ),
      // Il badge e' un'informazione accessoria: se la chiamata fallisce si
      // azzera in silenzio, senza disturbare l'utente con un errore su una
      // barra di navigazione.
      error: () => this.richiesteInAttesa.set(0)
    });
  }
}
