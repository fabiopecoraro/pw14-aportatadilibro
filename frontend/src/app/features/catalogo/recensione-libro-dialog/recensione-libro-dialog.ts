import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { StarRating } from '../../../shared/star-rating/star-rating';

export interface RecensioneLibroDialogResult {
  voto: number;
  commento: string | null;
}

/**
 * Dialog per recensire l'opera letteraria: voto 1-5 stelle e commento
 * opzionale, indipendenti dalla copia/prestito specifico.
 */
@Component({
  selector: 'app-recensione-libro-dialog',
  standalone: true,
  imports: [FormsModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatButtonModule, StarRating],
  templateUrl: './recensione-libro-dialog.html',
  styleUrl: './recensione-libro-dialog.scss'
})
export class RecensioneLibroDialog {
  private readonly dialogRef = inject(MatDialogRef<RecensioneLibroDialog, RecensioneLibroDialogResult>);

  readonly voto = signal(0);
  readonly commento = signal('');

  conferma(): void {
    if (this.voto() < 1) {
      return;
    }

    this.dialogRef.close({ voto: this.voto(), commento: this.commento().trim() || null });
  }

  annulla(): void {
    this.dialogRef.close();
  }
}
