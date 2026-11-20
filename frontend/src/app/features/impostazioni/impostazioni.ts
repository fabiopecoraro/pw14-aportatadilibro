import { Component, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AuthService } from '../../core/services/auth.service';
import { ProfiloService } from '../../core/services/profilo.service';
import { ProfiloDto } from '../../core/models/profilo.model';
import { EmptyState, Skeleton } from '../../shared/ui';
import { StatoCollezione } from '../mappa/mappa.model';

/** Lunghezza minima della password, allineata ad `AuthConstants` sul backend. */
const LUNGHEZZA_MINIMA_PASSWORD = 8;

/**
 * Impostazioni.
 *
 * Contiene le tre cose che il backend sa davvero fare sul proprio account:
 * leggere e modificare i dati del profilo, cambiare la password, uscire dalla
 * sessione. Il design prevede anche raggio di ricerca, preferenze di notifica
 * ed eliminazione account: nessuno dei tre ha un endpoint corrispondente, e
 * continuano a non comparire perché un controllo che non salva nulla sembra
 * funzionante senza esserlo.
 *
 * `username` è mostrato ma non modificabile: è l'identificativo di accesso ed
 * è incorporato nei token già emessi. L'identità pubblica è il nickname, che
 * invece si cambia liberamente — e quando cambia va riallineata anche la
 * sessione locale, altrimenti l'header continuerebbe a mostrare il vecchio
 * valore fino al logout.
 */
@Component({
  selector: 'app-impostazioni',
  standalone: true,
  imports: [
    DatePipe,
    RouterLink,
    ReactiveFormsModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    EmptyState,
    Skeleton
  ],
  templateUrl: './impostazioni.html',
  styleUrl: './impostazioni.scss'
})
export class Impostazioni {
  private readonly fb = inject(FormBuilder);
  private readonly profiloService = inject(ProfiloService);
  private readonly router = inject(Router);
  readonly authService = inject(AuthService);

  readonly lunghezzaMinimaPassword = LUNGHEZZA_MINIMA_PASSWORD;

  readonly stato = signal<StatoCollezione>('idle');
  readonly profilo = signal<ProfiloDto | null>(null);

  readonly salvataggioInCorso = signal(false);
  readonly profiloSalvato = signal(false);

  readonly cambioPasswordInCorso = signal(false);
  readonly passwordCambiata = signal(false);

  readonly formProfilo = this.fb.group({
    nickname: ['', Validators.required],
    nome: ['', Validators.required],
    cognome: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]]
  });

  readonly formPassword = this.fb.group({
    passwordAttuale: ['', Validators.required],
    nuovaPassword: ['', [Validators.required, Validators.minLength(LUNGHEZZA_MINIMA_PASSWORD)]]
  });

  constructor() {
    this.carica();
  }

  carica(): void {
    this.stato.set('loading');

    this.profiloService.getProfilo().subscribe({
      next: (profilo) => {
        this.profilo.set(profilo);
        this.formProfilo.patchValue({
          nickname: profilo.nickname,
          nome: profilo.nome,
          cognome: profilo.cognome,
          email: profilo.email
        });
        this.stato.set('ready');
      },
      error: () => this.stato.set('error')
    });
  }

  salvaProfilo(): void {
    if (this.formProfilo.invalid) {
      this.formProfilo.markAllAsTouched();
      return;
    }

    this.salvataggioInCorso.set(true);
    this.profiloSalvato.set(false);

    const valori = this.formProfilo.getRawValue();

    this.profiloService
      .aggiornaProfilo({
        nickname: valori.nickname!,
        nome: valori.nome!,
        cognome: valori.cognome!,
        email: valori.email!
      })
      .subscribe({
        next: (profilo) => {
          this.profilo.set(profilo);
          this.authService.aggiornaNickname(profilo.nickname);
          this.formProfilo.markAsPristine();
          this.salvataggioInCorso.set(false);
          this.profiloSalvato.set(true);
        },
        // Il messaggio d'errore (es. email già in uso) lo mostra
        // `errorInterceptor` nello snackbar, come in ogni altro form dell'app:
        // ripeterlo anche qui darebbe due volte la stessa informazione.
        error: () => this.salvataggioInCorso.set(false)
      });
  }

  cambiaPassword(): void {
    if (this.formPassword.invalid) {
      this.formPassword.markAllAsTouched();
      return;
    }

    this.cambioPasswordInCorso.set(true);
    this.passwordCambiata.set(false);

    const valori = this.formPassword.getRawValue();

    this.profiloService
      .cambiaPassword({
        passwordAttuale: valori.passwordAttuale!,
        nuovaPassword: valori.nuovaPassword!
      })
      .subscribe({
        next: () => {
          this.cambioPasswordInCorso.set(false);
          this.passwordCambiata.set(true);
          this.formPassword.reset();
        },
        error: () => this.cambioPasswordInCorso.set(false)
      });
  }

  logout(): void {
    this.authService.logout();
    this.router.navigateByUrl('/login');
  }
}
