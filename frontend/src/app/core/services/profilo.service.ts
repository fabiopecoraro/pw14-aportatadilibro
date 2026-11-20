import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AggiornaProfiloRequest, CambiaPasswordRequest, ProfiloDto } from '../models/profilo.model';

/**
 * Client HTTP per la gestione del proprio profilo.
 *
 * Nessun metodo accetta un id utente: il backend ricava sempre l'identita' dal
 * token, quindi non esiste nemmeno la possibilita' di indirizzare per errore
 * il profilo di qualcun altro.
 */
@Injectable({ providedIn: 'root' })
export class ProfiloService {
  constructor(private readonly http: HttpClient) {}

  getProfilo(): Observable<ProfiloDto> {
    return this.http.get<ProfiloDto>(`${environment.apiUrl}/profilo`);
  }

  aggiornaProfilo(request: AggiornaProfiloRequest): Observable<ProfiloDto> {
    return this.http.put<ProfiloDto>(`${environment.apiUrl}/profilo`, request);
  }

  cambiaPassword(request: CambiaPasswordRequest): Observable<void> {
    return this.http.put<void>(`${environment.apiUrl}/profilo/password`, request);
  }
}
