import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CategoriaAdminDto, CategoriaDto, SalvaCategoriaRequest } from '../models/categoria.model';

/**
 * Client HTTP per il vocabolario controllato delle categorie.
 *
 * La lettura serve ovunque (form di inserimento, filtri di catalogo e mappa)
 * ed e' aperta; creazione, rinomina ed eliminazione sono riservate all'Admin
 * come ogni altra scrittura sui metadati condivisi del catalogo.
 */
@Injectable({ providedIn: 'root' })
export class CategoriaService {
  constructor(private readonly http: HttpClient) {}

  getTutte(): Observable<CategoriaDto[]> {
    return this.http.get<CategoriaDto[]>(`${environment.apiUrl}/categorie`);
  }

  getPerGestione(): Observable<CategoriaAdminDto[]> {
    return this.http.get<CategoriaAdminDto[]>(`${environment.apiUrl}/categorie/gestione`);
  }

  crea(request: SalvaCategoriaRequest): Observable<CategoriaDto> {
    return this.http.post<CategoriaDto>(`${environment.apiUrl}/categorie`, request);
  }

  rinomina(id: string, request: SalvaCategoriaRequest): Observable<CategoriaDto> {
    return this.http.put<CategoriaDto>(`${environment.apiUrl}/categorie/${id}`, request);
  }

  elimina(id: string): Observable<void> {
    return this.http.delete<void>(`${environment.apiUrl}/categorie/${id}`);
  }
}
