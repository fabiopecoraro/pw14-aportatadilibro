import { Component, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { AggiornaLibroRequest, LibroDetailDto } from '../../../core/models/catalogo.model';
import { CategoriaDto } from '../../../core/models/categoria.model';
import { CategoriaService } from '../../../core/services/categoria.service';
import { ANNO_PUBBLICAZIONE_MINIMO, annoPubblicazioneMassimo } from '../../../core/utils/catalogo';
import { SelettoreCategorie } from '../../../shared/ui';

/**
 * Dialog di correzione dei metadati di un'opera, riservata all'Admin: sono
 * condivisi da tutte le copie, quindi la correzione fatta da un singolo
 * proprietario cambierebbe la scheda anche a tutti gli altri.
 *
 * Le categorie si scelgono dal vocabolario controllato: qui non si crea una
 * voce nuova nemmeno da Admin, perché aggiungere al vocabolario mentre si
 * corregge un libro è il modo più diretto per riempirlo di sinonimi. La
 * gestione del vocabolario ha una sua sezione nella dashboard.
 */
@Component({
  selector: 'app-edit-metadati-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    SelettoreCategorie
  ],
  templateUrl: './edit-metadati-dialog.html',
  styleUrl: './edit-metadati-dialog.scss'
})
export class EditMetadatiDialog {
  private readonly fb = inject(FormBuilder);
  private readonly categoriaService = inject(CategoriaService);
  private readonly dialogRef = inject(MatDialogRef<EditMetadatiDialog, AggiornaLibroRequest>);
  readonly data = inject<LibroDetailDto>(MAT_DIALOG_DATA);

  readonly form = this.fb.group({
    titolo: [this.data.titolo, Validators.required],
    autore: [this.data.autore, Validators.required],
    anno: [
      this.data.anno,
      [Validators.min(ANNO_PUBBLICAZIONE_MINIMO), Validators.max(annoPubblicazioneMassimo())]
    ],
    isbn: [this.data.isbn ?? '']
  });

  readonly annoMinimo = ANNO_PUBBLICAZIONE_MINIMO;
  readonly annoMassimo = annoPubblicazioneMassimo();

  readonly categorie = signal<CategoriaDto[]>([]);
  readonly categorieScelte = signal<string[]>(this.data.categorie.map((c) => c.id));
  readonly categorieMancanti = signal(false);

  constructor() {
    this.categoriaService.getTutte().subscribe((categorie) => this.categorie.set(categorie));
  }

  salva(): void {
    const senzaCategorie = this.categorieScelte().length === 0;
    this.categorieMancanti.set(senzaCategorie);

    if (this.form.invalid || senzaCategorie) {
      this.form.markAllAsTouched();
      return;
    }

    const valori = this.form.getRawValue();

    this.dialogRef.close({
      titolo: valori.titolo!,
      autore: valori.autore!,
      categorieIds: this.categorieScelte(),
      anno: valori.anno ?? null,
      isbn: valori.isbn || null
    });
  }

  annulla(): void {
    this.dialogRef.close();
  }
}
