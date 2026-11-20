import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PublicHeader } from '../../../shared/ui';

/**
 * Informativa privacy e termini di utilizzo (`/privacy`).
 *
 * Pagina pubblica, raggiungibile senza login: e' il documento che la checkbox
 * di consenso in registrazione chiede di accettare, quindi deve essere
 * leggibile *prima* di avere un account.
 *
 * Il testo descrive il trattamento realmente implementato, campo per campo,
 * invece di un modello generico: le scelte privacy di questo progetto (la
 * posizione GPS mai persistita, l'anagrafica reale mai esposta, la chat
 * ispezionabile solo se segnalata) sono verificabili nel codice, e
 * un'informativa che promettesse altro sarebbe peggio che nessuna
 * informativa. Le sezioni seguono i campi dell'entita' `Utente` e le tabelle
 * effettivamente scritte dall'applicazione.
 *
 * Resta un documento di progetto universitario, non un testo legale
 * validato: e' dichiarato esplicitamente in fondo alla pagina.
 */
@Component({
  selector: 'app-privacy',
  standalone: true,
  imports: [RouterLink, PublicHeader],
  templateUrl: './privacy.html',
  styleUrl: './privacy.scss'
})
export class Privacy {}
