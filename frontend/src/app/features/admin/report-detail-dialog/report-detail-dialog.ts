import { Component, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { AdminService } from '../../../core/services/admin.service';
import { ReportDettaglioDto, ReportDto } from '../../../core/models/admin.model';
import { EmptyState, Skeleton } from '../../../shared/ui';

/**
 * Dialog di moderazione: mostra il motivo del report e la cronologia della
 * chat del prestito segnalato. Disponibile solo per i report ancora Aperti
 * (vedi regola di privacy in ReportService.GetDettaglioReportAsync).
 */
@Component({
  selector: 'app-report-detail-dialog',
  standalone: true,
  imports: [DatePipe, MatDialogModule, MatButtonModule, EmptyState, Skeleton],
  templateUrl: './report-detail-dialog.html',
  styleUrl: './report-detail-dialog.scss'
})
export class ReportDetailDialog {
  private readonly adminService = inject(AdminService);
  readonly report = inject<ReportDto>(MAT_DIALOG_DATA);

  readonly dettaglio = signal<ReportDettaglioDto | null>(null);
  readonly loading = signal(false);
  readonly errore = signal(false);

  constructor() {
    this.carica();
  }

  carica(): void {
    this.loading.set(true);
    this.errore.set(false);

    this.adminService.getDettaglioReport(this.report.id).subscribe({
      next: (dettaglio) => {
        this.dettaglio.set(dettaglio);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.errore.set(true);
      }
    });
  }
}
