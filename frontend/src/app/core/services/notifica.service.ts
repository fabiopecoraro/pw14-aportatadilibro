import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { SottoscrizioneDisponibilitaDto } from '../models/notifica.model';

/**
 * Client HTTP per le sottoscrizioni "avvisami quando disponibile",
 * rispecchiando gli endpoint esposti da NotificheEndpoints sul backend.
 */
@Injectable({ providedIn: 'root' })
export class NotificaService {
  constructor(private readonly http: HttpClient) {}

  getSottoscrizioni(): Observable<SottoscrizioneDisponibilitaDto[]> {
    return this.http.get<SottoscrizioneDisponibilitaDto[]>(`${environment.apiUrl}/sottoscrizioni`);
  }

  sottoscrivi(libroId: string): Observable<SottoscrizioneDisponibilitaDto> {
    return this.http.post<SottoscrizioneDisponibilitaDto>(`${environment.apiUrl}/libri/${libroId}/sottoscrizioni`, {});
  }

  annullaSottoscrizione(libroId: string): Observable<void> {
    return this.http.delete<void>(`${environment.apiUrl}/libri/${libroId}/sottoscrizioni`);
  }
}
