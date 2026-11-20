import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  AggiornaLibroRequest,
  CopiaLibroDto,
  CreaLibroRequest,
  LibroDetailDto,
  LibroDto,
  LibroSummaryDto,
  MiaCopiaLibroDto
} from '../models/catalogo.model';

/**
 * Client HTTP per il catalogo globale dei libri e le copie possedute dagli
 * utenti, rispecchiando gli endpoint esposti da LibriEndpoints sul backend.
 */
@Injectable({ providedIn: 'root' })
export class LibroService {
  private readonly baseUrl = `${environment.apiUrl}/libri`;

  constructor(private readonly http: HttpClient) {}

  cercaLibri(filtri: { titolo?: string; autore?: string; categoriaId?: string }): Observable<LibroSummaryDto[]> {
    let params = new HttpParams();

    if (filtri.titolo) {
      params = params.set('titolo', filtri.titolo);
    }
    if (filtri.autore) {
      params = params.set('autore', filtri.autore);
    }
    if (filtri.categoriaId) {
      params = params.set('categoriaId', filtri.categoriaId);
    }

    return this.http.get<LibroSummaryDto[]>(this.baseUrl, { params });
  }

  getDettaglio(id: string): Observable<LibroDetailDto> {
    return this.http.get<LibroDetailDto>(`${this.baseUrl}/${id}`);
  }

  getMieCopie(): Observable<MiaCopiaLibroDto[]> {
    return this.http.get<MiaCopiaLibroDto[]>(`${this.baseUrl}/mie-copie`);
  }

  creaLibro(request: CreaLibroRequest): Observable<LibroDto> {
    return this.http.post<LibroDto>(this.baseUrl, request);
  }

  aggiornaMetadati(id: string, request: AggiornaLibroRequest): Observable<LibroDto> {
    return this.http.put<LibroDto>(`${this.baseUrl}/${id}`, request);
  }

  aggiungiCopia(libroId: string): Observable<CopiaLibroDto> {
    return this.http.post<CopiaLibroDto>(`${this.baseUrl}/${libroId}/copie`, {});
  }

  rimuoviCopia(copiaId: string): Observable<void> {
    return this.http.delete<void>(`${environment.apiUrl}/copie/${copiaId}`);
  }

  caricaCopertina(copiaId: string, file: File): Observable<CopiaLibroDto> {
    const formData = new FormData();
    formData.append('file', file);

    return this.http.post<CopiaLibroDto>(`${environment.apiUrl}/copie/${copiaId}/copertina`, formData);
  }
}
