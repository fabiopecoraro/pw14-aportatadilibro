import { Component, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AuthService } from '../../../core/services/auth.service';
import { RegisterRequest } from '../../../core/models/auth.model';
import { AuthPanel } from '../../../shared/ui';

/**
 * Registrazione.
 *
 * Il design mostra anche "Zona/quartiere" con "Usa la mia posizione
 * attuale": `Utente` non ha nessun campo per salvarla (la posizione non è
 * mai persistita) e `RegisterRequest` non la accetta, quindi il campo non
 * compare — chiederla per poi scartarla sarebbe peggio che non chiederla.
 * Il campo "Nickname pubblico" invece non è nel design ma è obbligatorio sul
 * backend reale: resta, con una nota che ne spiega lo scopo.
 *
 * Il consenso a informativa privacy e termini è una **checkbox obbligatoria**,
 * non la riga di testo del prototipo HTML: la traccia del project work chiede
 * esplicitamente di considerare "privacy e consenso", e un consenso che non si
 * può negare non è un consenso. Il backend applica lo stesso vincolo
 * (`AuthService.RegisterAsync` rifiuta la registrazione senza accettazione) e
 * registra il momento in `Utente.DataAccettazioneTermini`, così il consenso
 * resta dimostrabile e non solo dichiarato. È l'unico punto in cui il
 * prototipo viene deliberatamente disatteso in senso più restrittivo.
 */
@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, MatButtonModule, MatProgressSpinnerModule, AuthPanel],
  templateUrl: './register.html',
  styleUrl: './register.scss'
})
export class Register {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly loading = signal(false);
  readonly mostraPassword = signal(false);

  readonly form = this.fb.group({
    username: ['', Validators.required],
    nickname: ['', Validators.required],
    nome: ['', Validators.required],
    cognome: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
    // `requiredTrue` e non `required`: su una checkbox `required` e' gia'
    // soddisfatto dal valore `false`, che qui significa esattamente il
    // contrario di quello che serve.
    accettazioneTermini: [false, Validators.requiredTrue]
  });

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading.set(true);

    this.authService.register(this.form.getRawValue() as RegisterRequest).subscribe({
      next: () => this.router.navigateByUrl('/'),
      error: () => this.loading.set(false)
    });
  }
}
