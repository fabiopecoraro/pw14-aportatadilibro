import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

/**
 * Protegge le rotte riservate agli utenti autenticati, reindirizzando al
 * login chi non possiede un token JWT valido in sessione.
 */
export const authGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAuthenticated()) {
    return true;
  }

  return router.createUrlTree(['/login']);
};

/**
 * Protegge la home pubblica dagli utenti gia' autenticati: chi ha gia' una
 * sessione valida non deve vedere la pagina di marketing, va portato
 * direttamente alla schermata chiave dell'app.
 */
export const guestGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isAuthenticated()) {
    return true;
  }

  return router.createUrlTree(['/mappa']);
};

/**
 * Protegge le rotte riservate al ruolo Admin (es. dashboard analitica).
 */
export const adminGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAdmin()) {
    return true;
  }

  return router.createUrlTree(['/']);
};
