import { HttpClient } from '@angular/common/http';
import { Injectable, computed, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthResponse, LoginRequest, RegisterRequest } from '../models/auth.model';
import { RuoloUtente } from '../models/enums';

const TOKEN_STORAGE_KEY = 'aportatadilibro_token';
const USER_STORAGE_KEY = 'aportatadilibro_user';

/**
 * Gestisce lo stato di autenticazione lato client tramite Signals.
 * Il token JWT e i dati pubblici dell'utente (Username/Nickname/Ruolo)
 * sono persistiti in localStorage per sopravvivere al refresh della pagina;
 * nessun dato sensibile (password, email) viene mai conservato qui.
 */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly currentUserSignal = signal<AuthResponse | null>(this.readStoredUser());

  readonly currentUser = this.currentUserSignal.asReadonly();
  readonly isAuthenticated = computed(() => this.currentUserSignal() !== null);
  readonly isAdmin = computed(() => this.currentUserSignal()?.ruolo === RuoloUtente.Admin);

  constructor(private readonly http: HttpClient) {}

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${environment.apiUrl}/auth/login`, request).pipe(
      tap((response) => this.setSession(response))
    );
  }

  register(request: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${environment.apiUrl}/auth/register`, request).pipe(
      tap((response) => this.setSession(response))
    );
  }

  logout(): void {
    localStorage.removeItem(TOKEN_STORAGE_KEY);
    localStorage.removeItem(USER_STORAGE_KEY);
    this.currentUserSignal.set(null);
  }

  getToken(): string | null {
    return localStorage.getItem(TOKEN_STORAGE_KEY);
  }

  /**
   * Allinea il nickname della sessione dopo una modifica del profilo.
   *
   * Il nickname e' l'identita' pubblica mostrata nell'header e nel menu: senza
   * questo aggiornamento resterebbe quello vecchio fino al logout, perche' e'
   * copiato dal token al momento del login e non riletto a ogni navigazione.
   * Il token resta valido — porta id e ruolo, non il nickname — quindi non
   * serve rinnovarlo.
   */
  aggiornaNickname(nickname: string): void {
    const utente = this.currentUserSignal();
    if (!utente) {
      return;
    }

    const aggiornato = { ...utente, nickname };
    localStorage.setItem(USER_STORAGE_KEY, JSON.stringify(aggiornato));
    this.currentUserSignal.set(aggiornato);
  }

  private setSession(response: AuthResponse): void {
    localStorage.setItem(TOKEN_STORAGE_KEY, response.token);
    localStorage.setItem(USER_STORAGE_KEY, JSON.stringify(response));
    this.currentUserSignal.set(response);
  }

  private readStoredUser(): AuthResponse | null {
    const raw = localStorage.getItem(USER_STORAGE_KEY);
    return raw ? (JSON.parse(raw) as AuthResponse) : null;
  }
}
