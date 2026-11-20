import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';

export interface SegnalaDialogData {
  controparteNickname: string;
}

/**
 * Dialog di conferma per la segnalazione di un comportamento scorretto in
 * chat: raccoglie solo il motivo, obbligatorio lato backend
 * (ReportService.CreaReportAsync). L'invio effettivo resta al chiamante, che
 * conosce prestitoId e Id della controparte.
 */
@Component({
  selector: 'app-segnala-dialog',
  standalone: true,
  imports: [FormsModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatButtonModule],
  templateUrl: './segnala-dialog.html',
  styleUrl: './segnala-dialog.scss'
})
export class SegnalaDialog {
  private readonly dialogRef = inject(MatDialogRef<SegnalaDialog, string>);
  readonly data = inject<SegnalaDialogData>(MAT_DIALOG_DATA);

  readonly motivo = signal('');

  conferma(): void {
    const motivo = this.motivo().trim();
    if (!motivo) {
      return;
    }

    this.dialogRef.close(motivo);
  }

  annulla(): void {
    this.dialogRef.close();
  }
}
