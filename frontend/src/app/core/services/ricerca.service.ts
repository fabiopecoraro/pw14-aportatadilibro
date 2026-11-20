import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { RicercaLibroDto } from '../models/ricerca.model';

export interface RicercaFiltri {
  sud: number;
  ovest: number;
  nord: number;
  est: number;
  titolo?: string;
  autore?: string;
  categoriaId?: string;
}

/**
 * Client HTTP per la ricerca dei libri disponibili presso gli Hub compresi
 * nell'area di mappa (bounding box) visibile sul frontend. L'area inviata
 * viene usata solo per questa richiesta: non e' mai persistita.
 */
@Injectable({ providedIn: 'root' })
export class RicercaService {
  private readonly baseUrl = `${environment.apiUrl}/ricerca`;

  constructor(private readonly http: HttpClient) {}

  cercaLibri(filtri: RicercaFiltri): Observable<RicercaLibroDto[]> {
    let params = new HttpParams()
      .set('sud', filtri.sud)
      .set('ovest', filtri.ovest)
      .set('nord', filtri.nord)
      .set('est', filtri.est);

    if (filtri.titolo) {
      params = params.set('titolo', filtri.titolo);
    }
    if (filtri.autore) {
      params = params.set('autore', filtri.autore);
    }
    if (filtri.categoriaId) {
      params = params.set('categoriaId', filtri.categoriaId);
    }

    return this.http.get<RicercaLibroDto[]>(`${this.baseUrl}/libri`, { params });
  }
}
