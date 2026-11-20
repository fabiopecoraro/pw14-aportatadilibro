/** Punto di una serie temporale giornaliera (`giorno` in formato ISO `yyyy-MM-dd`). */
export interface PuntoSerieDto {
  giorno: string;
  valore: number;
}

/** Voce di un conteggio per categoria: una barra del grafico. */
export interface ContoEtichettaDto {
  etichetta: string;
  valore: number;
}

/**
 * Metriche d'uso della piattaforma mostrate nel modulo statistiche della
 * dashboard Admin.
 */
export interface MetricheUsoDto {
  visualizzazioniTotali: number;
  visualizzazioniPeriodo: number;
  richiestePrestitoTotali: number;
  richiestePrestitoPeriodo: number;
  giorniPeriodo: number;
  serieVisualizzazioni: PuntoSerieDto[];
  serieRichiestePrestito: PuntoSerieDto[];
  libriPiuVisti: ContoEtichettaDto[];
  prestitiPerStato: ContoEtichettaDto[];
}
