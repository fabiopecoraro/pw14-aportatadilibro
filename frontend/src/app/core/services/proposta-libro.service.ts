import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CreaPropostaLibroRequest, PropostaLibroDto } from '../models/proposta.model';

/**
 * Client HTTP per le proposte di nuovi libri inviate dagli utenti dalla
 * propria libreria personale e per la relativa moderazione Admin
 * (approvazione/rifiuto), rispecchiando ProposteLibroEndpoints sul backend.
 */
@Injectable({ providedIn: 'root' })
export class PropostaLibroService {
  private readonly baseUrl = `${environment.apiUrl}/proposte-libro`;

  constructor(private readonly http: HttpClient) {}

  creaProposta(request: CreaPropostaLibroRequest): Observable<PropostaLibroDto> {
    return this.http.post<PropostaLibroDto>(this.baseUrl, request);
  }

  getMieProposte(): Observable<PropostaLibroDto[]> {
    return this.http.get<PropostaLibroDto[]>(`${this.baseUrl}/mie`);
  }

  getPropostInAttesa(): Observable<PropostaLibroDto[]> {
    return this.http.get<PropostaLibroDto[]>(this.baseUrl);
  }

  approva(id: string): Observable<PropostaLibroDto> {
    return this.http.post<PropostaLibroDto>(`${this.baseUrl}/${id}/approva`, {});
  }

  rifiuta(id: string): Observable<PropostaLibroDto> {
    return this.http.post<PropostaLibroDto>(`${this.baseUrl}/${id}/rifiuta`, {});
  }
}
