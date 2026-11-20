import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  CreaRecensioneCopiaRequest,
  CreaRecensioneLibroRequest,
  CreaRecensioneUtenteRequest,
  RecensioneCopiaDto,
  RecensioneLibroDto,
  RecensioneUtenteDto
} from '../models/recensione.model';

/**
 * Client HTTP per il sistema di recensioni a 3 livelli, rispecchiando gli
 * endpoint esposti da RecensioniEndpoints sul backend.
 */
@Injectable({ providedIn: 'root' })
export class RecensioneService {
  constructor(private readonly http: HttpClient) {}

  creaRecensioneUtente(request: CreaRecensioneUtenteRequest): Observable<RecensioneUtenteDto> {
    return this.http.post<RecensioneUtenteDto>(`${environment.apiUrl}/recensioni/utenti`, request);
  }

  creaRecensioneLibro(request: CreaRecensioneLibroRequest): Observable<RecensioneLibroDto> {
    return this.http.post<RecensioneLibroDto>(`${environment.apiUrl}/recensioni/libri`, request);
  }

  creaRecensioneCopia(request: CreaRecensioneCopiaRequest): Observable<RecensioneCopiaDto> {
    return this.http.post<RecensioneCopiaDto>(`${environment.apiUrl}/recensioni/copie`, request);
  }

  getRecensioniUtente(utenteId: string): Observable<RecensioneUtenteDto[]> {
    return this.http.get<RecensioneUtenteDto[]>(`${environment.apiUrl}/utenti/${utenteId}/recensioni`);
  }

  getRecensioniLibro(libroId: string): Observable<RecensioneLibroDto[]> {
    return this.http.get<RecensioneLibroDto[]>(`${environment.apiUrl}/libri/${libroId}/recensioni`);
  }

  getRecensioniCopia(prestitoId: string): Observable<RecensioneCopiaDto[]> {
    return this.http.get<RecensioneCopiaDto[]>(`${environment.apiUrl}/prestiti/${prestitoId}/recensioni`);
  }
}
