import { Component, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { LibroService } from '../../../core/services/libro.service';
import { PropostaLibroService } from '../../../core/services/proposta-libro.service';
import { LibroSummaryDto } from '../../../core/models/catalogo.model';
import { CategoriaDto } from '../../../core/models/categoria.model';
import { CategoriaService } from '../../../core/services/categoria.service';
import { ANNO_PUBBLICAZIONE_MINIMO, annoPubblicazioneMassimo } from '../../../core/utils/catalogo';
import { SelettoreCategorie } from '../../../shared/ui';

/**
 * Dialog di aggiunta di un libro alla libreria personale.
 *
 * Il design disegna una ricerca per titolo o ISBN seguita da un passo di
 * conferma con condizione della copia, durata massima del prestito e una nota
 * per chi la riceve. Nessuno di questi tre campi ha un corrispondente sul
 * backend — `CopiaLibro` espone solo stato e copertina — e la scansione ISBN
 * e' esplicitamente elencata fra le evoluzioni future del progetto. Il flusso
 * qui replica quindi solo la parte coperta dal contratto reale: ricerca per
 * titolo, aggiunta immediata di una copia se il libro esiste, proposta di un
 * nuovo titolo altrimenti. Aggiungere quei campi lato client senza un posto
 * dove salvarli sarebbe un'interfaccia che promette piu' di quanto il sistema
 * mantiene.
 */
@Component({
  selector: 'app-aggiungi-libro-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    SelettoreCategorie
  ],
  templateUrl: './aggiungi-libro-dialog.html',
  styleUrl: './aggiungi-libro-dialog.scss'
})
export class AggiungiLibroDialog {
  private readonly fb = inject(FormBuilder);
  private readonly libroService = inject(LibroService);
  private readonly propostaService = inject(PropostaLibroService);
  private readonly categoriaService = inject(CategoriaService);
  private readonly dialogRef = inject(MatDialogRef<AggiungiLibroDialog, { aggiunta?: boolean; proposta?: boolean }>);

  readonly ricercaForm = this.fb.group({
    titolo: ['', Validators.required]
  });

  // L'anno resta facoltativo, con lo stesso intervallo applicato dal backend
  // (`CatalogoGuard.EnsureAnnoValido`): validarlo anche qui evita un giro di
  // rete per un refuso, ma la guardia server-side resta l'unica autorevole.
  readonly propostaForm = this.fb.group({
    titolo: ['', Validators.required],
    autore: ['', Validators.required],
    anno: [
      null as number | null,
      [Validators.min(ANNO_PUBBLICAZIONE_MINIMO), Validators.max(annoPubblicazioneMassimo())]
    ],
    isbn: ['']
  });

  readonly annoMinimo = ANNO_PUBBLICAZIONE_MINIMO;
  readonly annoMassimo = annoPubblicazioneMassimo();

  readonly cercato = signal(false);
  readonly cercaInCorso = signal(false);
  readonly risultati = signal<LibroSummaryDto[]>([]);
  readonly aggiuntaInCorso = signal<string | null>(null);
  readonly mostraProposta = signal(false);
  readonly propostaInCorso = signal(false);

  /**
   * Le categorie non stanno nel form reattivo ma in un signal a parte: il
   * selettore è un controllo custom a selezione multipla, e avvolgerlo in un
   * `ControlValueAccessor` solo per farlo convivere con `FormBuilder` sarebbe
   * cerimonia senza guadagno — la validazione qui è "almeno una", una riga.
   */
  readonly categorie = signal<CategoriaDto[]>([]);
  readonly categorieScelte = signal<string[]>([]);
  readonly categorieMancanti = signal(false);

  cerca(): void {
    if (this.ricercaForm.invalid) {
      this.ricercaForm.markAllAsTouched();
      return;
    }

    const titolo = this.ricercaForm.controls.titolo.value!;
    this.cercaInCorso.set(true);
    this.mostraProposta.set(false);

    this.libroService.cercaLibri({ titolo }).subscribe({
      next: (risultati) => {
        this.risultati.set(risultati);
        this.cercaInCorso.set(false);
        this.cercato.set(true);

        if (risultati.length === 0) {
          this.propostaForm.patchValue({ titolo });
          this.proponiNuovoLibro();
        }
      },
      error: () => this.cercaInCorso.set(false)
    });
  }

  aggiungiCopia(libro: LibroSummaryDto): void {
    this.aggiuntaInCorso.set(libro.id);

    this.libroService.aggiungiCopia(libro.id).subscribe({
      next: () => this.dialogRef.close({ aggiunta: true }),
      error: () => this.aggiuntaInCorso.set(null)
    });
  }

  /** Nomi delle categorie in riga, gia' ordinati dal server. */
  nomiCategorie(categorie: { nome: string }[]): string {
    return categorie.map((c) => c.nome).join(' · ');
  }

  proponiNuovoLibro(): void {
    this.mostraProposta.set(true);

    // Il vocabolario si carica solo quando serve davvero: chi trova il libro
    // in catalogo e aggiunge una copia non apre mai il form di proposta.
    if (this.categorie().length === 0) {
      this.categoriaService.getTutte().subscribe((categorie) => this.categorie.set(categorie));
    }
  }

  inviaProposta(): void {
    const senzaCategorie = this.categorieScelte().length === 0;
    this.categorieMancanti.set(senzaCategorie);

    if (this.propostaForm.invalid || senzaCategorie) {
      this.propostaForm.markAllAsTouched();
      return;
    }

    this.propostaInCorso.set(true);
    const valori = this.propostaForm.getRawValue();

    this.propostaService
      .creaProposta({
        titolo: valori.titolo!,
        autore: valori.autore!,
        categorieIds: this.categorieScelte(),
        anno: valori.anno ?? null,
        isbn: valori.isbn || null
      })
      .subscribe({
        next: () => this.dialogRef.close({ proposta: true }),
        error: () => this.propostaInCorso.set(false)
      });
  }

  annulla(): void {
    this.dialogRef.close();
  }
}
