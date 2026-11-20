import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CreaReportRequest, ReportDto } from '../models/admin.model';

/**
 * Client HTTP per la creazione di segnalazioni da parte di un partecipante
 * a un prestito (POST /api/report, aperto a qualunque utente autenticato).
 * Distinto da AdminService, che copre invece la moderazione (elenco,
 * dettaglio, chiusura), riservata all'Admin.
 */
@Injectable({ providedIn: 'root' })
export class ReportService {
  constructor(private readonly http: HttpClient) {}

  creaReport(request: CreaReportRequest): Observable<ReportDto> {
    return this.http.post<ReportDto>(`${environment.apiUrl}/report`, request);
  }
}
