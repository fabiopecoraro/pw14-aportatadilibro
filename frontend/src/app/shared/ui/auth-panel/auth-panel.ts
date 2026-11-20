import { Component, input } from '@angular/core';

/** Le due varianti di colore del pannello editoriale, una per schermata. */
export type AuthPanelVariante = 'ink' | 'green';

/**
 * Pannello editoriale delle schermate di autenticazione: metà
 * colonna a tinta piena con wordmark, titolo e sottotitolo, più un'area
 * proiettata per la decorazione propria di ciascuna schermata (le due
 * "costole" di libro nel login, la lista puntata nella registrazione — non
 * hanno abbastanza in comune da diventare anch'esse parte del componente).
 */
@Component({
  selector: 'app-auth-panel',
  standalone: true,
  templateUrl: './auth-panel.html',
  styleUrl: './auth-panel.scss',
  host: {
    '[class]': '"variante-" + variante()'
  }
})
export class AuthPanel {
  readonly variante = input<AuthPanelVariante>('ink');
  readonly titolo = input.required<string>();
  readonly sottotitolo = input('');
}
