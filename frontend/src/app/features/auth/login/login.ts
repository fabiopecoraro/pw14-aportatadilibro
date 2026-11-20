import { Component, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AuthService } from '../../../core/services/auth.service';
import { AuthPanel } from '../../../shared/ui';

/**
 * Login.
 *
 * Il design usa "Email" come campo di accesso; il contratto reale
 * (`LoginRequest`) è `{ username, password }` — username e email sono
 * concetti distinti nel dominio e l'email non è mai esposta pubblicamente,
 * quindi il campo qui si chiama onestamente "Nome utente" invece di far finta
 * che l'email autentichi.
 *
 * Il design mostra anche "Password dimenticata?" e "Continua con Google":
 * nessuno dei due ha un endpoint dietro, quindi non compaiono — un link che
 * porta a un flusso che non esiste sarebbe peggio che non mostrarlo.
 */
@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, MatButtonModule, MatProgressSpinnerModule, AuthPanel],
  templateUrl: './login.html',
  styleUrl: './login.scss'
})
export class Login {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly loading = signal(false);
  readonly mostraPassword = signal(false);

  readonly form = this.fb.group({
    username: ['', Validators.required],
    password: ['', Validators.required]
  });

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading.set(true);

    // Il messaggio di errore (credenziali non valide, ecc.) e' mostrato dallo
    // snackbar globale di errorInterceptor: qui basta sbloccare il form.
    this.authService.login(this.form.getRawValue() as { username: string; password: string }).subscribe({
      next: () => this.router.navigateByUrl('/'),
      error: () => this.loading.set(false)
    });
  }
}
