import * as L from 'leaflet';

/**
 * Tetto allo zoom dell'inquadratura automatica. Con un solo punto `fitBounds`
 * arriverebbe allo zoom massimo, incollando la vista al marciapiede: qui si
 * resta al livello del quartiere.
 */
const ZOOM_MASSIMO = 15;

/** Respiro fra i punti inquadrati e il bordo della tela, in pixel. */
export const MARGINE_INQUADRATURA = 40;

/**
 * Stringe la vista della mappa sui punti ricevuti.
 *
 * Sta in `core/utils` perche' la usano due schermate — la mappa di ricerca e
 * "I miei Hub" — e il comportamento deve essere identico: entrambe si aprono
 * sull'Italia intera, perche' l'applicazione non presume dove si trovi chi la
 * apre, ed entrambe devono poi stringersi su cio' che hanno davvero ricevuto
 * invece di lasciare l'utente davanti a una cartina con un puntino in mezzo.
 *
 * `margineInferiore` esiste per le tele coperte in basso da un pannello:
 * `fitBounds` ragiona sull'intero contenitore, quindi senza quel margine i
 * punti finiscono centrati dietro al pannello e la parte visibile resta vuota.
 */
export function inquadraSuPunti(
  mappa: L.Map,
  punti: L.LatLngTuple[],
  margineInferiore = MARGINE_INQUADRATURA
): void {
  if (punti.length === 0) {
    return;
  }

  mappa.fitBounds(L.latLngBounds(punti), {
    paddingTopLeft: [MARGINE_INQUADRATURA, MARGINE_INQUADRATURA],
    paddingBottomRight: [MARGINE_INQUADRATURA, margineInferiore],
    maxZoom: ZOOM_MASSIMO
  });
}
