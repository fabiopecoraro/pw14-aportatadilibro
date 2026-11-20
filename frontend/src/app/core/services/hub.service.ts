import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CreaHubRequest, DisponibilitaHubCopiaDto, HubDto } from '../models/hub.model';

/**
 * Client HTTP per gli Hub (punti di scambio pubblici) dell'utente,
 * rispecchiando gli endpoint esposti da HubEndpoints sul backend.
 */
@Injectable({ providedIn: 'root' })
export class HubService {
  private readonly baseUrl = `${environment.apiUrl}/hub`;

  // Le esclusioni sono esposte sotto /api/copie perche' appartengono alla
  // copia, non all'Hub: la risorsa padre e' la copia posseduta dall'utente.
  private readonly copieUrl = `${environment.apiUrl}/copie`;

  constructor(private readonly http: HttpClient) {}

  getHubUtente(): Observable<HubDto[]> {
    return this.http.get<HubDto[]>(this.baseUrl);
  }

  creaHub(request: CreaHubRequest): Observable<HubDto> {
    return this.http.post<HubDto>(this.baseUrl, request);
  }

  eliminaHub(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  /** Elenca gli Hub dell'utente segnalando dove la copia indicata e' esclusa. */
  getDisponibilitaCopia(copiaId: string): Observable<DisponibilitaHubCopiaDto[]> {
    return this.http.get<DisponibilitaHubCopiaDto[]>(`${this.copieUrl}/${copiaId}/esclusioni`);
  }

  /** Rende la copia non disponibile presso l'Hub indicato. */
  escludiCopiaDaHub(copiaId: string, hubId: string): Observable<void> {
    return this.http.post<void>(`${this.copieUrl}/${copiaId}/esclusioni/${hubId}`, {});
  }

  /** Ripristina la disponibilita' di default della copia presso l'Hub indicato. */
  rimuoviEsclusione(copiaId: string, hubId: string): Observable<void> {
    return this.http.delete<void>(`${this.copieUrl}/${copiaId}/esclusioni/${hubId}`);
  }
}
