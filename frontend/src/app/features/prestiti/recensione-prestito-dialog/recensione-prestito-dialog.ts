import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { StarRating } from '../../../shared/star-rating/star-rating';

export interface RecensionePrestitoDialogData {
  controparteNickname: string;
}

export interface RecensionePrestitoDialogResult {
  controparte: { voto: number; commento: string | null } | null;
  esperienza: { voto: number; commento: string | null } | null;
}

/**
 * Dialog di recensione di un prestito concluso: valutazione separata per
 * l'affidabilita' della controparte e per l'esperienza di scambio. Ogni
 * sezione e' opzionale, indipendente dall'altra.
 */
@Component({
  selector: 'app-recensione-prestito-dialog',
  standalone: true,
  imports: [FormsModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatButtonModule, StarRating],
  templateUrl: './recensione-prestito-dialog.html',
  styleUrl: './recensione-prestito-dialog.scss'
})
export class RecensionePrestitoDialog {
  private readonly dialogRef = inject(MatDialogRef<RecensionePrestitoDialog, RecensionePrestitoDialogResult>);
  readonly data = inject<RecensionePrestitoDialogData>(MAT_DIALOG_DATA);

  readonly votoControparte = signal(0);
  readonly commentoControparte = signal('');
  readonly votoEsperienza = signal(0);
  readonly commentoEsperienza = signal('');

  conferma(): void {
    this.dialogRef.close({
      controparte:
        this.votoControparte() > 0
          ? { voto: this.votoControparte(), commento: this.commentoControparte().trim() || null }
          : null,
      esperienza:
        this.votoEsperienza() > 0
          ? { voto: this.votoEsperienza(), commento: this.commentoEsperienza().trim() || null }
          : null
    });
  }

  annulla(): void {
    this.dialogRef.close();
  }
}
