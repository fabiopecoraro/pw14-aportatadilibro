import { Component, input } from '@angular/core';
import { DatePipe } from '@angular/common';
import { StarRating } from '../../star-rating/star-rating';

/**
 * Forma normalizzata di una recensione.
 *
 * Il dominio ne ha tre tipi (sul libro, sull'affidabilita' della persona,
 * sull'esperienza di scambio) con DTO diversi ma la stessa resa: si normalizza
 * qui invece di duplicare il componente per ciascuno.
 */
export interface RecensioneVista {
  id: string;
  autore: string;
  voto: number;
  commento: string | null;
  data: string;
}

/**
 * Elenco di recensioni, come nel blocco "Cosa dicono i lettori del quartiere"
 * della scheda libro. Riusato dal profilo pubblico per le recensioni ricevute.
 */
@Component({
  selector: 'app-elenco-recensioni',
  standalone: true,
  imports: [DatePipe, StarRating],
  templateUrl: './elenco-recensioni.html',
  styleUrl: './elenco-recensioni.scss'
})
export class ElencoRecensioni {
  readonly recensioni = input<RecensioneVista[]>([]);
}
