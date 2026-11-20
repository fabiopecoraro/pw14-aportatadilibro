import { Component, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { Avatar, BookCard, BookCover, EmptyState, Skeleton, StatusBadge } from '../../shared/ui';

/**
 * Vetrina del design system.
 *
 * Non fa parte del prodotto: e' uno strumento di sviluppo che raccoglie in una
 * sola pagina token, scala tipografica e componenti condivisi. Serve a
 * verificare la fedelta' al design senza dover attraversare l'applicazione
 * schermata per schermata, e a evitare che due implementazioni successive dello
 * stesso elemento divergano. La rotta e' `/design-system`.
 */
@Component({
  selector: 'app-design-system',
  standalone: true,
  imports: [
    MatButtonModule,
    MatChipsModule,
    MatIconModule,
    Avatar,
    BookCard,
    BookCover,
    EmptyState,
    Skeleton,
    StatusBadge
  ],
  templateUrl: './design-system.html',
  styleUrl: './design-system.scss'
})
export class DesignSystem {
  // Il campione dipinge `var(--apl-*)`, mentre `valore` e' solo l'etichetta
  // stampata accanto: vanno tenuti allineati a mano. Per muted-2, green e
  // danger qui compaiono i valori scuriti per il contrasto AA, non quelli di
  // partenza — altrimenti la pagina che serve a verificare la fedelta'
  // mostrerebbe un colore e ne dichiarerebbe un altro.
  readonly colori = [
    { nome: 'ink', variabile: '--apl-ink', valore: '#16201a' },
    { nome: 'ink-soft', variabile: '--apl-ink-soft', valore: '#3d453f' },
    { nome: 'muted', variabile: '--apl-muted', valore: '#4c564f' },
    { nome: 'muted-2', variabile: '--apl-muted-2', valore: '#5f665e' },
    { nome: 'muted-3', variabile: '--apl-muted-3', valore: '#8a938c' },
    { nome: 'green', variabile: '--apl-green', valore: '#3a7055' },
    { nome: 'green-dark', variabile: '--apl-green-dark', valore: '#2c5a43' },
    { nome: 'green-soft', variabile: '--apl-green-soft', valore: '#8fc0a6' },
    { nome: 'terracotta', variabile: '--apl-terracotta', valore: '#c9764a' },
    { nome: 'danger', variabile: '--apl-danger', valore: '#9d4d43' },
    { nome: 'info', variabile: '--apl-info', valore: '#4a6ec9' },
    { nome: 'paper', variabile: '--apl-paper', valore: '#fbfbf7' },
    { nome: 'paper-2', variabile: '--apl-paper-2', valore: '#f2f1ea' },
    { nome: 'paper-3', variabile: '--apl-paper-3', valore: '#ecebe2' },
    { nome: 'canvas', variabile: '--apl-canvas', valore: '#e9e5dc' },
    { nome: 'sage-100', variabile: '--apl-sage-100', valore: '#e0e8df' },
    { nome: 'sage-200', variabile: '--apl-sage-200', valore: '#d6e0d5' },
    { nome: 'sage-300', variabile: '--apl-sage-300', valore: '#cdd9cc' },
    { nome: 'sage-400', variabile: '--apl-sage-400', valore: '#b9c2bb' }
  ];

  readonly filtri = ['Tutti', 'Narrativa', 'Saggi', '< 1 km'];
  readonly filtroAttivo = signal('Tutti');
}
