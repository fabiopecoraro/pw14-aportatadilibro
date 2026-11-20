import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PrestitoDto, RichiediPrestitoRequest } from '../models/prestito.model';

/**
 * Client HTTP per il ciclo di vita delle richieste di prestito,
 * rispecchiando gli endpoint esposti da PrestitiEndpoints sul backend.
 */
@Injectable({ providedIn: 'root' })
export class PrestitoService {
  private readonly baseUrl = `${environment.apiUrl}/prestiti`;

  constructor(private readonly http: HttpClient) {}

  richiediPrestito(copiaId: string, request: RichiediPrestitoRequest): Observable<PrestitoDto> {
    return this.http.post<PrestitoDto>(`${environment.apiUrl}/copie/${copiaId}/prestiti`, request);
  }

  getInviati(): Observable<PrestitoDto[]> {
    return this.http.get<PrestitoDto[]>(`${this.baseUrl}/inviati`);
  }

  getRicevuti(): Observable<PrestitoDto[]> {
    return this.http.get<PrestitoDto[]>(`${this.baseUrl}/ricevuti`);
  }

  accetta(id: string): Observable<PrestitoDto> {
    return this.http.post<PrestitoDto>(`${this.baseUrl}/${id}/accetta`, {});
  }

  rifiuta(id: string): Observable<PrestitoDto> {
    return this.http.post<PrestitoDto>(`${this.baseUrl}/${id}/rifiuta`, {});
  }

  annulla(id: string): Observable<PrestitoDto> {
    return this.http.post<PrestitoDto>(`${this.baseUrl}/${id}/annulla`, {});
  }

  completa(id: string): Observable<PrestitoDto> {
    return this.http.post<PrestitoDto>(`${this.baseUrl}/${id}/completa`, {});
  }
}
