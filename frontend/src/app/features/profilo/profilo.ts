import { Component, computed, inject, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { MatButtonModule } from '@angular/material/button';
import { AuthService } from '../../core/services/auth.service';
import { LibroService } from '../../core/services/libro.service';
import { PrestitoService } from '../../core/services/prestito.service';
import { RecensioneService } from '../../core/services/recensione.service';
import { RecensioneUtenteDto } from '../../core/models/recensione.model';
import { StatoCopia, StatoPrestito } from '../../core/models/enums';
import { Avatar, EmptyState, Skeleton } from '../../shared/ui';
import { StarRating } from '../../shared/star-rating/star-rating';
import { StatoCollezione } from '../mappa/mappa.model';

/**
 * Profilo personale.
 *
 * Il design mostra anche la zona di residenza e un badge "Lettrice
 * verificata". Nessuno dei due ha un corrispondente reale: la posizione
 * dell'utente non viene mai salvata (invariante di privacy del progetto, non
 * una mancanza da colmare) e non esiste un concetto di verifica account. Si
 * mostra quindi solo ciò che è realmente disponibile — nickname, statistiche
 * derivate dai dati dell'utente, rating e recensioni ricevute — invece di
 * inventare quei campi. La modifica dei propri dati sta nella schermata
 * Impostazioni, a cui questa rimanda: qui il profilo si guarda, lì si cambia.
 */
@Component({
  selector: 'app-profilo',
  standalone: true,
  imports: [DecimalPipe, RouterLink, MatButtonModule, Avatar, StarRating, EmptyState, Skeleton],
  templateUrl: './profilo.html',
  styleUrl: './profilo.scss'
})
export class Profilo {
  private readonly libroService = inject(LibroService);
  private readonly prestitoService = inject(PrestitoService);
  private readonly recensioneService = inject(RecensioneService);
  readonly authService = inject(AuthService);

  readonly stato = signal<StatoCollezione>('idle');
  readonly recensioni = signal<RecensioneUtenteDto[]>([]);
  readonly libriInLibreria = signal(0);
  readonly libriInPrestitoOra = signal(0);
  readonly libriPrestati = signal(0);
  readonly libriPresi = signal(0);

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

  carica(): void {
    const utente = this.authService.currentUser();
    if (!utente) {
      return;
    }

    this.stato.set('loading');

    forkJoin({
      copie: this.libroService.getMieCopie(),
      inviati: this.prestitoService.getInviati(),
      ricevuti: this.prestitoService.getRicevuti(),
      recensioni: this.recensioneService.getRecensioniUtente(utente.id)
    }).subscribe({
      next: ({ copie, inviati, ricevuti, recensioni }) => {
        this.libriInLibreria.set(copie.length);
        this.libriInPrestitoOra.set(copie.filter((c) => c.stato === StatoCopia.InPrestito).length);

        // "Prestato"/"preso" conta i prestiti effettivamente avvenuti
        // (accettati o conclusi): una richiesta rifiutata o annullata non ha
        // mai spostato un libro.
        const avvenuto = (stato: StatoPrestito) => stato === StatoPrestito.Accettato || stato === StatoPrestito.Completato;
        this.libriPrestati.set(ricevuti.filter((p) => avvenuto(p.stato)).length);
        this.libriPresi.set(inviati.filter((p) => avvenuto(p.stato)).length);

        this.recensioni.set(recensioni);
        this.stato.set('ready');
      },
      error: () => this.stato.set('error')
    });
  }
}
