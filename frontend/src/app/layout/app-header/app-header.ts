import { Component, input } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatMenu, MatMenuModule } from '@angular/material/menu';
import { Avatar } from '../../shared/ui';

/**
 * Header dell'applicazione autenticata (da 640px in su).
 *
 * Wordmark a sinistra, navigazione primaria con sottolineatura verde sulla voce
 * corrente, campanella delle richieste con badge e avatar dell'utente.
 *
 * Il menu del profilo arriva dall'esterno come riferimento a `MatMenu`: header e
 * tab bar mostrano lo stesso elenco di sezioni secondarie, e passare il menu
 * invece di duplicarne il markup garantisce che le due barre non possano
 * divergere.
 */
@Component({
  selector: 'app-header',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, MatIconModule, MatMenuModule, Avatar],
  templateUrl: './app-header.html',
  styleUrl: './app-header.scss'
})
export class AppHeader {
  /** Nickname dell'utente: e' l'unico dato personale che il progetto espone. */
  readonly nickname = input.required<string>();

  /** Richieste di prestito ricevute e in attesa di risposta. */
  readonly richieste = input(0);

  /** Menu delle sezioni secondarie, condiviso con la tab bar. */
  readonly menuUtente = input.required<MatMenu>();
}
