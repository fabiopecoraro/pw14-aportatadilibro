import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { RichiediPrestitoRequest } from '../../../core/models/prestito.model';

/**
 * Dialog per la richiesta di prestito di una copia altrui: chiede solo la
 * durata stimata in giorni, come previsto da RichiediPrestitoRequest.
 *
 * Usa la classe `.apl-field` invece di `mat-form-field`: e' il pattern di campo
 * con label sopra e fondo paper del design, piu' semplice da riprodurre
 * fedelmente qui che riconfigurando l'outline di Material per un solo campo.
 */
@Component({
  selector: 'app-richiedi-prestito-dialog',
  standalone: true,
  imports: [ReactiveFormsModule, MatDialogModule, MatButtonModule],
  templateUrl: './richiedi-prestito-dialog.html',
  styleUrl: './richiedi-prestito-dialog.scss'
})
export class RichiediPrestitoDialog {
  private readonly fb = inject(FormBuilder);
  private readonly dialogRef = inject(MatDialogRef<RichiediPrestitoDialog, RichiediPrestitoRequest>);

  readonly form = this.fb.group({
    giorniStimati: [7, [Validators.required, Validators.min(1)]]
  });

  conferma(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.dialogRef.close({ giorniStimati: this.form.value.giorniStimati! });
  }

  annulla(): void {
    this.dialogRef.close();
  }
}
