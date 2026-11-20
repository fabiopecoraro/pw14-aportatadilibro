import { Component } from '@angular/core';
import { PublicHeader } from '../../../shared/ui';

/**
 * Come funziona.
 *
 * Il testo del design descrive due meccanismi che non sono quelli reali del
 * progetto, corretti qui:
 *  - "la tua posizione resta approssimativa, gli altri vedono un'area" — il
 *    progetto non sfuma le coordinate, non le salva affatto. I libri si
 *    scambiano negli Hub pubblici che il proprietario sceglie, mai al suo
 *    indirizzo.
 *  - "chiedi una proroga prima della scadenza" — non esiste un'azione di
 *    proroga; riformulato senza promettere una funzionalità che non c'è.
 * Pubblicare una descrizione della privacy diversa da quella reale sarebbe un
 * problema più serio di un'incongruenza visiva.
 */
@Component({
  selector: 'app-come-funziona',
  standalone: true,
  imports: [PublicHeader],
  templateUrl: './come-funziona.html',
  styleUrl: './come-funziona.scss'
})
export class ComeFunziona {}
