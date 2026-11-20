import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

/**
 * Centralizza la gestione degli errori HTTP: mostra il messaggio restituito
 * da ExceptionHandlingMiddleware (payload `{ message }`) tramite uno snackbar
 * e, in caso di 401, invalida la sessione corrente riportando al login.
 */
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const snackBar = inject(MatSnackBar);
  const authService = inject(AuthService);
  const router = inject(Router);

  return next(req).pipe(
    catchError((error: unknown) => {
      if (error instanceof HttpErrorResponse) {
        const message = error.error?.message ?? 'Si è verificato un errore imprevisto.';

        if (error.status === 401) {
          authService.logout();
          router.navigate(['/login']);
        }

        snackBar.open(message, 'Chiudi', { duration: 5000 });
      }

      return throwError(() => error);
    })
  );
};
