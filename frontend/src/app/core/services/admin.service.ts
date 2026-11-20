import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { DashboardStatisticheDto, ReportDettaglioDto, ReportDto } from '../models/admin.model';
import { MetricheUsoDto } from '../models/metriche.model';
import { StatoReport } from '../models/enums';

/**
 * Client HTTP per l'area Admin: statistiche aggregate, export CSV del
 * catalogo e delle metriche d'uso, moderazione dei report sulla chat.
 */
@Injectable({ providedIn: 'root' })
export class AdminService {
  constructor(private readonly http: HttpClient) {}

  getStatistiche(): Observable<DashboardStatisticheDto> {
    return this.http.get<DashboardStatisticheDto>(`${environment.apiUrl}/admin/statistiche`);
  }

  getMetriche(giorni = 30): Observable<MetricheUsoDto> {
    return this.http.get<MetricheUsoDto>(`${environment.apiUrl}/admin/metriche?giorni=${giorni}`);
  }

  esportaCatalogoCsv(): Observable<Blob> {
    return this.http.get(`${environment.apiUrl}/admin/catalogo/csv`, { responseType: 'blob' });
  }

  /**
   * Serie temporale delle metriche d'uso in CSV. Il periodo è quello scelto
   * sulla dashboard: il file scaricato deve coprire la finestra che l'Admin
   * sta guardando, non un intervallo fisso deciso qui.
   */
  esportaMetricheCsv(giorni = 30): Observable<Blob> {
    return this.http.get(`${environment.apiUrl}/admin/metriche/csv?giorni=${giorni}`, {
      responseType: 'blob'
    });
  }

  getReports(stato?: StatoReport): Observable<ReportDto[]> {
    const url = stato ? `${environment.apiUrl}/report?stato=${stato}` : `${environment.apiUrl}/report`;
    return this.http.get<ReportDto[]>(url);
  }

  getDettaglioReport(id: string): Observable<ReportDettaglioDto> {
    return this.http.get<ReportDettaglioDto>(`${environment.apiUrl}/report/${id}`);
  }

  chiudiReport(id: string): Observable<ReportDto> {
    return this.http.post<ReportDto>(`${environment.apiUrl}/report/${id}/chiudi`, {});
  }
}
