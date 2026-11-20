import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog } from '@angular/material/dialog';
import { LibroService } from '../../../core/services/libro.service';
import { PropostaLibroService } from '../../../core/services/proposta-libro.service';
import { MiaCopiaLibroDto } from '../../../core/models/catalogo.model';
import { PropostaLibroDto } from '../../../core/models/proposta.model';
import { StatoCopia, StatoProposta } from '../../../core/models/enums';
import { resolveFileUrl } from '../../../core/utils/files';
import { AggiungiLibroDialog } from '../aggiungi-libro-dialog/aggiungi-libro-dialog';
import { DisponibilitaHubDialog } from '../disponibilita-hub-dialog/disponibilita-hub-dialog';
import { BookCard, EmptyState, Skeleton } from '../../../shared/ui';

/** Filtro applicato alla griglia: gli stessi tre che compaiono come chip nel design. */
type FiltroLibreria = 'tutti' | 'disponibili' | 'in-prestito' | string;

/**
 * "La mia libreria": le copie possedute dall'utente, con conteggi,
 * filtro per stato e categoria, e le proposte di nuovi libri in attesa di
 * approvazione Admin.
 */
@Component({
  selector: 'app-libreria-list',
  standalone: true,
  imports: [RouterLink, MatButtonModule, MatIconModule, BookCard, EmptyState, Skeleton],
  templateUrl: './libreria-list.html',
  styleUrl: './libreria-list.scss'
})
export class LibreriaList {
  private readonly libroService = inject(LibroService);
  private readonly propostaService = inject(PropostaLibroService);
  private readonly dialog = inject(MatDialog);

  readonly StatoProposta = StatoProposta;
  readonly resolveFileUrl = resolveFileUrl;

  readonly loading = signal(false);
  readonly errore = signal(false);
  readonly copie = signal<MiaCopiaLibroDto[]>([]);
  readonly proposte = signal<PropostaLibroDto[]>([]);
  readonly filtro = signal<FiltroLibreria>('tutti');
  readonly gestioneInCorso = signal<string | null>(null);

  readonly disponibili = computed(() => this.copie().filter((c) => c.stato === StatoCopia.Disponibile).length);
  readonly inPrestito = computed(() => this.copie().length - this.disponibili());

  /** Categorie presenti in libreria, per le chip dopo Tutti/Disponibili/In prestito. */
  readonly categorieDisponibili = computed(() =>
    [...new Set(this.copie().flatMap((c) => c.categorie.map((cat) => cat.nome)))].sort((a, b) =>
      a.localeCompare(b, 'it')
    )
  );

  readonly copieFiltrate = computed(() => {
    const filtro = this.filtro();
    if (filtro === 'tutti') {
      return this.copie();
    }
    if (filtro === 'disponibili') {
      return this.copie().filter((c) => c.stato === StatoCopia.Disponibile);
    }
    if (filtro === 'in-prestito') {
      return this.copie().filter((c) => c.stato === StatoCopia.InPrestito);
    }
    // Non essendo uno dei tre stati, il filtro e' il nome di una categoria.
    return this.copie().filter((c) => c.categorie.some((cat) => cat.nome === filtro));
  });

  /** Nomi delle categorie in riga, gia' ordinati dal server. */
  nomiCategorie(categorie: { nome: string }[]): string {
    return categorie.map((c) => c.nome).join(' · ');
  }

  constructor() {
    this.carica();
  }

  carica(): void {
    this.loading.set(true);
    this.errore.set(false);

    this.libroService.getMieCopie().subscribe({
      next: (copie) => {
        this.copie.set(copie);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.errore.set(true);
      }
    });

    this.propostaService.getMieProposte().subscribe((proposte) => this.proposte.set(proposte));
  }

  aggiungiLibro(): void {
    this.dialog
      .open(AggiungiLibroDialog)
      .afterClosed()
      .subscribe((result) => {
        if (result?.aggiunta || result?.proposta) {
          this.carica();
        }
      });
  }

  /**
   * Apre la configurazione delle esclusioni Copia/Hub. Il dialog scrive gia'
   * sul server a ogni toggle: qui si ricarica solo per riallineare il
   * contatore mostrato sulla card.
   */
  gestisciDisponibilita(copia: MiaCopiaLibroDto, evento: Event): void {
    evento.stopPropagation();

    this.dialog
      .open(DisponibilitaHubDialog, {
        data: { copiaId: copia.id, titolo: copia.titolo }
      })
      .afterClosed()
      .subscribe((modificato) => {
        if (modificato) {
          this.carica();
        }
      });
  }
}
