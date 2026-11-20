import { Component, computed, inject, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { AuthService } from '../../core/services/auth.service';
import { RecensioneService } from '../../core/services/recensione.service';
import { RecensioneUtenteDto } from '../../core/models/recensione.model';
import { Avatar, EmptyState, Skeleton } from '../../shared/ui';
import { StarRating } from '../../shared/star-rating/star-rating';
import { StatoCollezione } from '../mappa/mappa.model';

/**
 * Profilo pubblico di un altro utente — dietro login, non una pagina
 * del sito pubblico: il design la chiama "pubblica" nel senso di "visibile
 * agli altri utenti dell'app", non "raggiungibile senza autenticarsi". Resta
 * dentro la shell autenticata.
 *
 * Rispetto al design mancano zona, data di iscrizione, badge di verifica,
 * conteggio prestiti e griglia dei libri disponibili: nessuno di questi ha
 * un endpoint che risolva un id utente arbitrario in quei dati — l'unico dato
 * pubblico reale su un altro utente sono le recensioni ricevute
 * (`GET /api/utenti/{id}/recensioni`).
 * Il nickname stesso non ha un endpoint dedicato: si ricava dal campo
 * `utenteRecensitoNickname` già presente su ogni recensione. Anche la
 * distanza è esclusa qui: la geolocalizzazione resta confinata alla
 * sola schermata mappa, per lo stesso vincolo di privacy che vale ovunque nel
 * progetto.
 */
@Component({
  selector: 'app-profilo-pubblico',
  standalone: true,
  imports: [DecimalPipe, MatButtonModule, Avatar, StarRating, EmptyState, Skeleton],
  templateUrl: './profilo-pubblico.html',
  styleUrl: './profilo-pubblico.scss'
})
export class ProfiloPubblico {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly recensioneService = inject(RecensioneService);
  private readonly authService = inject(AuthService);

  readonly stato = signal<StatoCollezione>('idle');
  readonly recensioni = signal<RecensioneUtenteDto[]>([]);

  /** Il proprio profilo ha una pagina dedicata con dati aggiuntivi: qui si redirige. */
  private readonly propriaId = computed(() => this.authService.currentUser()?.id);

  readonly nickname = computed(() => this.recensioni()[0]?.utenteRecensitoNickname ?? null);

  readonly ratingMedio = computed(() => {
    const voti = this.recensioni();
    if (voti.length === 0) {
      return null;
    }
    return voti.reduce((somma, r) => somma + r.voto, 0) / voti.length;
  });

  constructor() {
    this.carica();
  }

  private get utenteId(): string {
    return this.route.snapshot.paramMap.get('userId')!;
  }

  carica(): void {
    if (this.utenteId === this.propriaId()) {
      this.router.navigateByUrl('/profilo');
      return;
    }

    this.stato.set('loading');

    this.recensioneService.getRecensioniUtente(this.utenteId).subscribe({
      next: (recensioni) => {
        this.recensioni.set(recensioni);
        this.stato.set('ready');
      },
      error: () => this.stato.set('error')
    });
  }
}
