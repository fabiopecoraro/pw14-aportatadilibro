import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';

/**
 * Header del sito pubblico, condiviso fra le pagine non autenticate del
 * progetto.
 *
 * Il design mostra anche "Vicino a te" e "La community" come voci di nav: la
 * prima aprirebbe una mappa pubblica, che il principio "nessun dato senza
 * login" di questo progetto esclude esplicitamente; la seconda non ha una
 * pagina corrispondente. Restano solo i link verso contenuti che esistono
 * davvero.
 */
@Component({
  selector: 'app-public-header',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './public-header.html',
  styleUrl: './public-header.scss'
})
export class PublicHeader {
  /** Voce di nav corrispondente alla pagina corrente, per l'evidenza attiva. */
  readonly paginaAttiva = input<'home' | 'come-funziona' | 'privacy' | null>(null);
}
