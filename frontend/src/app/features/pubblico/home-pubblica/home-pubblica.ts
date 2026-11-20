import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PublicHeader } from '../../../shared/ui';

/**
 * Home pubblica.
 *
 * Il progetto ha un principio esplicito: nessun dato reale compare senza
 * login. La sezione "Vicino a te, adesso" del design — mappa interattiva +
 * elenco di libri con distanze precise — è stata rimossa del tutto, non
 * sostituita: mostrare anche solo un esempio realistico rischierebbe di far
 * credere a un visitatore non autenticato che stia guardando dati veri.
 *
 * L'illustrazione dell'hero (copertine impilate + etichetta "a 350 m da te")
 * resta, perché nel prototipo è già chiaramente un'illustrazione — copertine
 * a righe diagonali, non una fotografia — ed è marcata qui con un'etichetta
 * "Esempio" esplicita perché non ci sia dubbio che sia una demo e non una
 * ricerca reale.
 */
@Component({
  selector: 'app-home-pubblica',
  standalone: true,
  imports: [RouterLink, PublicHeader],
  templateUrl: './home-pubblica.html',
  styleUrl: './home-pubblica.scss'
})
export class HomePubblica {}
