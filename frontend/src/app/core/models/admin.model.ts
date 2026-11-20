import { StatoReport } from './enums';

export interface DashboardStatisticheDto {
  totaleLibri: number;
  totaleCopie: number;
  copieDisponibili: number;
  copieInPrestito: number;
  durataMediaPrestitiGiorni: number | null;
  reportAperti: number;
}

/** Segnalazione di un comportamento scorretto nella chat di un prestito. */
export interface CreaReportRequest {
  prestitoId: string;
  utenteSegnalatoId: string;
  motivo: string;
}

export interface ReportDto {
  id: string;
  prestitoId: string;
  segnalanteNickname: string;
  utenteSegnalatoNickname: string;
  motivo: string;
  stato: StatoReport;
  dataCreazione: string;
}

export interface ChatMessaggioDto {
  id: string;
  prestitoId: string;
  mittenteId: string;
  mittenteNickname: string;
  testo: string;
  dataInvio: string;
}

export interface ReportDettaglioDto {
  report: ReportDto;
  messaggi: ChatMessaggioDto[];
}
