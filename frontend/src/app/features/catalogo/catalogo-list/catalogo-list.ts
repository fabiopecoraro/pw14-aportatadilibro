import { Component, computed, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { LibroService } from '../../../core/services/libro.service';
import { LibroSummaryDto } from '../../../core/models/catalogo.model';
import { BookCard, EmptyState, Skeleton } from '../../../shared/ui';
import { StatoCollezione } from '../../mappa/mappa.model';

/**
 * Catalogo condiviso, dietro login: la ricerca pubblica prevista dal
 * design non è stata costruita, questa è la ricerca reale del progetto.
 * Il design mostra un unico campo di ricerca generico ("calvino") che
 * trova sia per titolo sia per autore: il backend filtra i due campi in AND,
 * non in OR (`GET /api/libri?titolo=&autore=&categoriaId=`), quindi un solo campo
 * che promettesse una ricerca combinata mentirebbe sul suo comportamento.
 * Restano due campi distinti, entrambi opzionali.
 *
 * Filtri di distanza e valutazione del prestatore del design non hanno
 * corrispondenza: questo è il catalogo globale, senza nozione di posizione
 * (quella vive solo in `/mappa`, per lo stesso vincolo di privacy di sempre).
 * Il filtro "solo disponibili ora" è invece reale, applicato lato client sui
 * risultati già caricati.
 */
@Component({
  selector: 'app-catalogo-list',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, MatButtonModule, BookCard, EmptyState, Skeleton],
  templateUrl: './catalogo-list.html',
  styleUrl: './catalogo-list.scss'
})
export class CatalogoList {
  private readonly fb = inject(FormBuilder);
  private readonly libroService = inject(LibroService);

  readonly stato = signal<StatoCollezione>('idle');
  readonly risultati = signal<LibroSummaryDto[]>([]);
  readonly categoriaAttiva = signal<string | null>(null);
  readonly soloDisponibili = signal(false);

  readonly form = this.fb.group({
    titolo: [''],
    autore: ['']
  });

  // Le chip si ricavano dalle categorie effettivamente presenti nei risultati,
  // non dal vocabolario completo: offrire un filtro che non seleziona nulla e'
  // peggio che non offrirlo. `flatMap` perche' ogni opera ne ha piu' di una.
  readonly categorieDisponibili = computed(() =>
    [...new Set(this.risultati().flatMap((l) => l.categorie.map((c) => c.nome)))].sort((a, b) =>
      a.localeCompare(b, 'it')
    )
  );

  readonly risultatiFiltrati = computed(() => {
    const categoria = this.categoriaAttiva();
    const soloDisp = this.soloDisponibili();

    return this.risultati().filter((libro) => {
      if (categoria && !libro.categorie.some((c) => c.nome === categoria)) {
        return false;
      }
      if (soloDisp && libro.copieDisponibili === 0) {
        return false;
      }
      return true;
    });
  });

  /**
   * Riga di metadati della card: categorie e anno.
   *
   * Sta qui e non nel template perché con più categorie la concatenazione
   * condizionale diventerebbe un'espressione inline illeggibile. Le categorie
   * sono già ordinate dal server.
   */
  metaLibro(libro: LibroSummaryDto): string {
    const categorie = libro.categorie.map((c) => c.nome).join(' · ');
    return libro.anno ? `${categorie} · ${libro.anno}` : categorie;
  }

  constructor() {
    this.cerca();
  }

  cerca(): void {
    this.stato.set(this.risultati().length === 0 ? 'loading' : this.stato());
    const filtri = this.form.getRawValue();

    this.libroService
      .cercaLibri({
        titolo: filtri.titolo || undefined,
        autore: filtri.autore || undefined
      })
      .subscribe({
        next: (risultati) => {
          this.risultati.set(risultati);
          this.stato.set('ready');
        },
        error: () => this.stato.set('error')
      });
  }

  reset(): void {
    this.form.reset({ titolo: '', autore: '' });
    this.categoriaAttiva.set(null);
    this.soloDisponibili.set(false);
    this.cerca();
  }
}
