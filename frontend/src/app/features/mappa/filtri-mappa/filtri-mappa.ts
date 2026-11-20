import { Component, input, output } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';

/**
 * Campo di ricerca e chip di filtro della schermata mappa.
 *
 * E' separato dall'elenco perche' le due parti non stanno insieme in tutte le
 * forme: sul desktop condividono la colonna di sinistra, mentre sul mobile i
 * filtri vivono nella barra sopra la mappa e l'elenco nel bottom sheet.
 *
 * Il componente non filtra nulla: emette testo e categoria e lascia alla schermata
 * il compito di applicarli, cosi' la stessa coppia di valori alimenta sia
 * l'elenco sia i pin senza doverli sincronizzare.
 */
@Component({
  selector: 'app-filtri-mappa',
  standalone: true,
  imports: [MatIconModule],
  templateUrl: './filtri-mappa.html',
  styleUrl: './filtri-mappa.scss'
})
export class FiltriMappa {
  /** Testo corrente della ricerca (titolo o autore). */
  readonly testo = input('');

  /** Categorie presenti fra i risultati dell'area visibile. */
  readonly categorieDisponibili = input<string[]>([]);

  /** Categoria selezionata; `null` corrisponde alla chip "Tutti". */
  readonly categoriaAttiva = input<string | null>(null);

  readonly testoChange = output<string>();
  readonly categoriaChange = output<string | null>();

  aggiornaTesto(evento: Event): void {
    this.testoChange.emit((evento.target as HTMLInputElement).value);
  }
}
