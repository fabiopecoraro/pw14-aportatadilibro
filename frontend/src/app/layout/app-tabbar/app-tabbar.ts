import { Component, input } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatMenu, MatMenuModule } from '@angular/material/menu';

/**
 * Tab bar di navigazione mobile (sotto i 640px).
 *
 * Segue il prototipo: fondo ink, etichette Archivo 500 11px, voce corrente in
 * paper-2 e voci inattive in un grigio verde tenue. Nel prototipo le voci sono
 * quattro (Mappa, Libreria, Chat, Profilo); qui sono Mappa, Libreria, Chat,
 * Prestiti: al posto di "Profilo" c'e' Prestiti, perche' e' la sezione che
 * richiede un'azione piu' spesso. La quinta voce raccoglie in un menu le sezioni che
 * non entrano nella navigazione primaria (Catalogo, Hub, avvisi, area
 * amministrativa).
 */
@Component({
  selector: 'app-tabbar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, MatIconModule, MatMenuModule],
  templateUrl: './app-tabbar.html',
  styleUrl: './app-tabbar.scss',
  // Il ruolo sta sull'elemento del componente e non su un <nav> interno: le
  // voci sono figli diretti del contenitore flessibile, e un elemento in piu'
  // in mezzo ne cambierebbe la disposizione. Il nome distingue questa
  // navigazione da quella dell'intestazione, che su schermi larghi la sostituisce.
  host: { role: 'navigation', 'aria-label': 'Navigazione principale' }
})
export class AppTabbar {
  /** Richieste di prestito ricevute e in attesa di risposta. */
  readonly richieste = input(0);

  /** Menu delle sezioni secondarie, condiviso con l'header desktop. */
  readonly menuUtente = input.required<MatMenu>();
}
