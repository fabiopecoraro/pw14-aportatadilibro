/** Coppia di coordinate geografiche in gradi decimali. */
export interface Coordinate {
  lat: number;
  lng: number;
}

const RAGGIO_TERRESTRE_METRI = 6_371_000;

/**
 * Distanza in metri fra due coordinate, con la formula dell'emisenoverso
 * (haversine).
 *
 * Il calcolo sta deliberatamente sul client. Il progetto non usa PostGIS e, cosa
 * piu' importante, la posizione del browser non deve mai raggiungere il
 * database: tenendo qui la distanza, il backend continua a ricevere solo il
 * rettangolo di mappa visibile e non il punto in cui si trova l'utente.
 */
export function distanzaMetri(da: Coordinate, a: Coordinate): number {
  const radianti = (gradi: number) => (gradi * Math.PI) / 180;

  const deltaLat = radianti(a.lat - da.lat);
  const deltaLng = radianti(a.lng - da.lng);
  const latDa = radianti(da.lat);
  const latA = radianti(a.lat);

  const h =
    Math.sin(deltaLat / 2) ** 2 + Math.sin(deltaLng / 2) ** 2 * Math.cos(latDa) * Math.cos(latA);

  return 2 * RAGGIO_TERRESTRE_METRI * Math.asin(Math.sqrt(h));
}

/**
 * Formatta una distanza come nel design: metri arrotondati alla decina sotto il
 * chilometro ("350 m"), chilometri con un decimale sopra ("1,5 km").
 */
export function formattaDistanza(metri: number): string {
  if (metri < 1000) {
    return `${Math.round(metri / 10) * 10} m`;
  }

  const chilometri = metri / 1000;
  return `${chilometri.toFixed(1).replace('.', ',')} km`;
}

/** Rettangolo geografico delimitato dai quattro lati, in gradi decimali. */
export interface AreaGeografica {
  sud: number;
  ovest: number;
  nord: number;
  est: number;
}

/**
 * Passo della griglia a cui si allarga l'area di ricerca: un centesimo di
 * grado, poco piu' di un chilometro alle latitudini italiane.
 */
export const PASSO_GRIGLIA_AREA = 0.01;

/**
 * Allarga un'area fino ai multipli della griglia che la contengono.
 *
 * Al server si invia questa e non il rettangolo esatto della mappa. Il motivo
 * e' che dopo "La tua posizione" la mappa e' centrata su chi la guarda: il
 * centro del rettangolo esatto sarebbe la sua posizione, e inviarlo
 * equivarrebbe a inviarla. Un'area agganciata alla griglia e' la stessa per
 * chiunque si trovi nello stesso riquadro, quindi dal suo centro non si
 * risale a nessuno.
 *
 * Si allarga sempre verso l'esterno e mai all'interno, cosi' l'area inviata
 * contiene per intero quella visibile e nessun Hub sullo schermo va perso.
 */
export function allargaAllaGriglia(area: AreaGeografica, passo = PASSO_GRIGLIA_AREA): AreaGeografica {
  // L'arrotondamento a sei decimali toglie il rumore della virgola mobile
  // (40.67 * 100 non e' esattamente 4067), che altrimenti sposterebbe il
  // bordo di un intero passo.
  const pulisci = (valore: number) => Math.round(valore * 1e6) / 1e6;
  const giu = (valore: number) => pulisci(Math.floor(pulisci(valore / passo)) * passo);
  const su = (valore: number) => pulisci(Math.ceil(pulisci(valore / passo)) * passo);

  return {
    sud: Math.max(-90, giu(area.sud)),
    ovest: Math.max(-180, giu(area.ovest)),
    nord: Math.min(90, su(area.nord)),
    est: Math.min(180, su(area.est))
  };
}

/** Vero se il punto cade dentro l'area, bordi compresi. */
export function contenutoNellArea(punto: Coordinate, area: AreaGeografica): boolean {
  return punto.lat >= area.sud && punto.lat <= area.nord && punto.lng >= area.ovest && punto.lng <= area.est;
}
