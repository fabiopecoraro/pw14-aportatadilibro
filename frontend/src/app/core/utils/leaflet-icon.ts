import * as L from 'leaflet';

/**
 * Le icone di default di Leaflet sono referenziate tramite percorsi relativi
 * che il bundler di Angular non risolve automaticamente. Le immagini vengono
 * copiate in `/leaflet/` (vedi angular.json) e qui si riconfigura l'icona
 * di default per puntare a quella posizione.
 */
export function configureDefaultLeafletIcon(): void {
  const defaultPrototype = L.Icon.Default.prototype as unknown as { _getIconUrl?: unknown };
  delete defaultPrototype._getIconUrl;

  L.Icon.Default.mergeOptions({
    iconRetinaUrl: 'leaflet/marker-icon-2x.png',
    iconUrl: 'leaflet/marker-icon.png',
    shadowUrl: 'leaflet/marker-shadow.png'
  });
}

/** Stato visivo di un pin nella sincronia mappa/elenco. */
export type StatoPin = 'normale' | 'evidenziato' | 'selezionato';

/**
 * Crea il pin del design come `divIcon`.
 *
 * Si usa un `divIcon` e non un'immagine perche' il pin cambia dimensione e
 * colore in tre stati e, quando e' selezionato, porta un anello pulsante e
 * l'etichetta della distanza: sono transizioni CSS, che su una PNG
 * richiederebbero piu' asset e un cambio di icona a ogni interazione.
 *
 * @param stato stato visivo del pin
 * @param etichetta testo mostrato sopra il pin selezionato (tipicamente la distanza)
 */
export function creaPinLibro(stato: StatoPin = 'normale', etichetta = ''): L.DivIcon {
  const classi = ['apl-pin'];
  if (stato === 'evidenziato') {
    classi.push('apl-pin-evidenziato');
  } else if (stato === 'selezionato') {
    classi.push('apl-pin-selezionato');
  }

  // L'anello pulsante e l'etichetta appartengono al solo stato selezionato: sono
  // gli unici elementi animati della mappa, e il movimento viene sospeso da
  // prefers-reduced-motion (vedi _map.scss).
  const anello = stato === 'selezionato' ? '<span class="apl-pin-anello"></span>' : '';
  const testo =
    stato === 'selezionato' && etichetta
      ? `<span class="apl-pin-etichetta">${etichetta}</span>`
      : '';

  const dimensione = stato === 'normale' ? 22 : 26;

  return L.divIcon({
    html: `<span class="${classi.join(' ')}">${anello}${testo}</span>`,
    // La classe del contenitore Leaflet resta vuota per non ereditare lo stile
    // del marker di default.
    className: '',
    iconSize: [dimensione, dimensione],
    iconAnchor: [dimensione / 2, dimensione / 2]
  });
}

/** Marker della posizione corrente del browser (dato transitorio, mai salvato). */
export function creaPinPosizione(): L.DivIcon {
  return L.divIcon({
    html: '<span class="apl-pin-posizione"></span>',
    className: '',
    iconSize: [16, 16],
    iconAnchor: [8, 8]
  });
}
