import { Component, computed, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog } from '@angular/material/dialog';
import { AdminService } from '../../../core/services/admin.service';
import { CategoriaService } from '../../../core/services/categoria.service';
import { PropostaLibroService } from '../../../core/services/proposta-libro.service';
import { DashboardStatisticheDto, ReportDto } from '../../../core/models/admin.model';
import { CategoriaAdminDto } from '../../../core/models/categoria.model';
import { MetricheUsoDto, PuntoSerieDto } from '../../../core/models/metriche.model';
import { PropostaLibroDto } from '../../../core/models/proposta.model';
import { StatoReport } from '../../../core/models/enums';
import { BarraGrafico, ChartBarre, ChartLinea, EmptyState, PuntoGrafico, Skeleton } from '../../../shared/ui';
import { StatoCollezione } from '../../mappa/mappa.model';
import { ReportDetailDialog } from '../report-detail-dialog/report-detail-dialog';

/**
 * Gli stati del prestito arrivano dal backend con il nome dell'enum C#
 * (`Richiesto`, `Accettato`, ...). La legenda del grafico li mostra nella
 * forma usata dal resto dell'interfaccia.
 */
const ETICHETTE_STATO_PRESTITO: Record<string, string> = {
  Richiesto: 'Richiesti',
  Accettato: 'Accettati',
  Rifiutato: 'Rifiutati',
  Completato: 'Completati',
  Annullato: 'Annullati'
};

/**
 * Dashboard Admin: non prevista dal design, quindi costruita da zero.
 * Le collezioni della schermata, ciascuna caricata per conto proprio,
 * seguono comunque lo stesso pattern di stato `StatoCollezione` usato altrove
 * (skeleton coerente con la forma reale, errore distinto dal vuoto con
 * "Riprova"), invece di un solo booleano `loading`, che lascerebbe un errore
 * di caricamento senza alcun feedback.
 */
@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [
    DatePipe,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    FormsModule,
    ChartBarre,
    ChartLinea,
    EmptyState,
    Skeleton
  ],
  templateUrl: './admin-dashboard.html',
  styleUrl: './admin-dashboard.scss'
})
export class AdminDashboard {
  private readonly adminService = inject(AdminService);
  private readonly propostaService = inject(PropostaLibroService);
  private readonly categoriaService = inject(CategoriaService);
  private readonly dialog = inject(MatDialog);

  readonly StatoReport = StatoReport;

  readonly statoStatistiche = signal<StatoCollezione>('idle');
  readonly statistiche = signal<DashboardStatisticheDto | null>(null);
  readonly esportazioneInCorso = signal(false);
  /**
   * Un segnale per ogni export e non uno condiviso: con un solo flag i due
   * pulsanti si spegnerebbero a vicenda, e chi scarica il catalogo sembrerebbe
   * aver perso anche l'altro comando.
   */
  readonly esportazioneMetricheInCorso = signal(false);

  readonly statoMetriche = signal<StatoCollezione>('idle');
  readonly metriche = signal<MetricheUsoDto | null>(null);
  readonly giorniPeriodo = signal(30);

  /**
   * Le serie arrivano dal server come `{ giorno: '2026-08-24', valore: n }`.
   * I grafici vogliono un'etichetta già leggibile: la conversione sta qui e
   * non nei componenti, che restano ignari del dominio e riusabili.
   */
  readonly puntiVisualizzazioni = computed(() => this.toPunti(this.metriche()?.serieVisualizzazioni));
  readonly puntiRichieste = computed(() => this.toPunti(this.metriche()?.serieRichiestePrestito));

  readonly barreLibriPiuVisti = computed<BarraGrafico[]>(() =>
    (this.metriche()?.libriPiuVisti ?? []).map((v) => ({ etichetta: v.etichetta, valore: v.valore }))
  );

  readonly barrePrestitiPerStato = computed<BarraGrafico[]>(() =>
    (this.metriche()?.prestitiPerStato ?? []).map((v) => ({
      etichetta: ETICHETTE_STATO_PRESTITO[v.etichetta] ?? v.etichetta,
      valore: v.valore
    }))
  );

  readonly statoCategorie = signal<StatoCollezione>('idle');
  readonly categorie = signal<CategoriaAdminDto[]>([]);
  readonly categoriaInCorso = signal<string | null>(null);
  readonly nuovaCategoria = signal('');
  /** Id della categoria in rinomina, con il testo in corso di modifica. */
  readonly rinominaId = signal<string | null>(null);
  readonly rinominaNome = signal('');

  readonly statoProposte = signal<StatoCollezione>('idle');
  readonly proposte = signal<PropostaLibroDto[]>([]);
  readonly gestionePropostaInCorso = signal<string | null>(null);

  readonly statoReports = signal<StatoCollezione>('idle');
  readonly reports = signal<ReportDto[]>([]);
  readonly chiusuraInCorso = signal<string | null>(null);
  readonly mostraChiusi = signal(false);

  constructor() {
    this.caricaStatistiche();
    this.caricaMetriche();
    this.caricaCategorie();
    this.caricaProposte();
    this.caricaReports();
  }

  caricaStatistiche(): void {
    this.statoStatistiche.set('loading');

    this.adminService.getStatistiche().subscribe({
      next: (statistiche) => {
        this.statistiche.set(statistiche);
        this.statoStatistiche.set('ready');
      },
      error: () => this.statoStatistiche.set('error')
    });
  }

  caricaMetriche(): void {
    this.statoMetriche.set('loading');

    this.adminService.getMetriche(this.giorniPeriodo()).subscribe({
      next: (metriche) => {
        this.metriche.set(metriche);
        this.statoMetriche.set('ready');
      },
      error: () => this.statoMetriche.set('error')
    });
  }

  impostaPeriodo(giorni: number): void {
    if (this.giorniPeriodo() === giorni) {
      return;
    }
    this.giorniPeriodo.set(giorni);
    this.caricaMetriche();
  }

  /** Nomi delle categorie in riga, gia' ordinati dal server. */
  nomiCategorie(categorie: { nome: string }[]): string {
    return categorie.map((c) => c.nome).join(' · ');
  }

  // --- Vocabolario delle categorie -------------------------------------------

  /**
   * Una voce si elimina solo se non la usa nessuno. Le proposte contano quanto
   * le opere: il server rifiuta comunque, e un pulsante abilitato che porta a
   * un errore è peggio di un pulsante spento con la ragione nel tooltip.
   */
  eliminabile(categoria: CategoriaAdminDto): boolean {
    return categoria.numeroLibri === 0 && categoria.numeroProposte === 0;
  }

  motivoNonEliminabile(categoria: CategoriaAdminDto): string | null {
    if (categoria.numeroLibri > 0) {
      return `Usata da ${categoria.numeroLibri} ${categoria.numeroLibri === 1 ? 'opera' : 'opere'}`;
    }
    if (categoria.numeroProposte > 0) {
      return 'Usata da una proposta ancora da valutare';
    }
    return null;
  }

  usoCategoria(categoria: CategoriaAdminDto): string {
    const parti: string[] = [];

    if (categoria.numeroLibri > 0) {
      parti.push(`${categoria.numeroLibri} ${categoria.numeroLibri === 1 ? 'opera' : 'opere'}`);
    }
    if (categoria.numeroProposte > 0) {
      parti.push(
        `${categoria.numeroProposte} ${categoria.numeroProposte === 1 ? 'proposta' : 'proposte'}`
      );
    }

    return parti.length === 0 ? 'Non ancora usata' : parti.join(' · ');
  }

  caricaCategorie(): void {
    this.statoCategorie.set('loading');

    this.categoriaService.getPerGestione().subscribe({
      next: (categorie) => {
        this.categorie.set(categorie);
        this.statoCategorie.set('ready');
      },
      error: () => this.statoCategorie.set('error')
    });
  }

  creaCategoria(): void {
    const nome = this.nuovaCategoria().trim();
    if (!nome) {
      return;
    }

    this.categoriaInCorso.set('nuova');

    this.categoriaService.crea({ nome }).subscribe({
      next: () => {
        this.nuovaCategoria.set('');
        this.categoriaInCorso.set(null);
        this.caricaCategorie();
      },
      error: () => this.categoriaInCorso.set(null)
    });
  }

  iniziaRinomina(categoria: CategoriaAdminDto): void {
    this.rinominaId.set(categoria.id);
    this.rinominaNome.set(categoria.nome);
  }

  annullaRinomina(): void {
    this.rinominaId.set(null);
    this.rinominaNome.set('');
  }

  confermaRinomina(id: string): void {
    const nome = this.rinominaNome().trim();
    if (!nome) {
      return;
    }

    this.categoriaInCorso.set(id);

    // La rinomina si propaga da sola a ogni opera collegata: e' il guadagno
    // concreto del vocabolario rispetto al testo libero, dove correggere un
    // refuso avrebbe voluto dire aggiornare ogni riga che lo conteneva.
    this.categoriaService.rinomina(id, { nome }).subscribe({
      next: () => {
        this.annullaRinomina();
        this.categoriaInCorso.set(null);
        this.caricaCategorie();
      },
      error: () => this.categoriaInCorso.set(null)
    });
  }

  eliminaCategoria(categoria: CategoriaAdminDto): void {
    this.categoriaInCorso.set(categoria.id);

    // Il backend rifiuta comunque l'eliminazione di una categoria in uso: qui
    // il pulsante e' gia' disabilitato, ma l'errore resta gestito perche' fra
    // il caricamento della pagina e il click qualcuno puo' averla assegnata.
    this.categoriaService.elimina(categoria.id).subscribe({
      next: () => {
        this.categoriaInCorso.set(null);
        this.caricaCategorie();
      },
      error: () => this.categoriaInCorso.set(null)
    });
  }

  aggiornaNuovaCategoria(evento: Event): void {
    this.nuovaCategoria.set((evento.target as HTMLInputElement).value);
  }

  aggiornaRinominaNome(evento: Event): void {
    this.rinominaNome.set((evento.target as HTMLInputElement).value);
  }

  private toPunti(serie: PuntoSerieDto[] | undefined): PuntoGrafico[] {
    return (serie ?? []).map((p) => ({
      // `giorno` è una data pura (`yyyy-MM-dd`): la si spezza a mano invece di
      // passare per `new Date()`, che la interpreterebbe come UTC e su fusi a
      // ovest di Greenwich mostrerebbe il giorno precedente.
      etichetta: `${p.giorno.slice(8, 10)}/${p.giorno.slice(5, 7)}`,
      valore: p.valore
    }));
  }

  caricaProposte(): void {
    this.statoProposte.set('loading');

    this.propostaService.getPropostInAttesa().subscribe({
      next: (proposte) => {
        this.proposte.set(proposte);
        this.statoProposte.set('ready');
      },
      error: () => this.statoProposte.set('error')
    });
  }

  approvaProposta(proposta: PropostaLibroDto): void {
    this.gestionePropostaInCorso.set(proposta.id);

    this.propostaService.approva(proposta.id).subscribe({
      next: () => {
        this.gestionePropostaInCorso.set(null);
        this.caricaProposte();
      },
      error: () => this.gestionePropostaInCorso.set(null)
    });
  }

  rifiutaProposta(proposta: PropostaLibroDto): void {
    this.gestionePropostaInCorso.set(proposta.id);

    this.propostaService.rifiuta(proposta.id).subscribe({
      next: () => {
        this.gestionePropostaInCorso.set(null);
        this.caricaProposte();
      },
      error: () => this.gestionePropostaInCorso.set(null)
    });
  }

  caricaReports(): void {
    this.statoReports.set('loading');
    const stato = this.mostraChiusi() ? StatoReport.Chiuso : StatoReport.Aperto;

    this.adminService.getReports(stato).subscribe({
      next: (reports) => {
        this.reports.set(reports);
        this.statoReports.set('ready');
      },
      error: () => this.statoReports.set('error')
    });
  }

  impostaMostraChiusi(valore: boolean): void {
    if (this.mostraChiusi() === valore) {
      return;
    }
    this.mostraChiusi.set(valore);
    this.caricaReports();
  }

  esportaCsv(): void {
    this.esportazioneInCorso.set(true);

    this.adminService.esportaCatalogoCsv().subscribe({
      next: (blob) => {
        this.esportazioneInCorso.set(false);
        this.scarica(blob, 'catalogo.csv');
      },
      error: () => this.esportazioneInCorso.set(false)
    });
  }

  esportaMetricheCsv(): void {
    const giorni = this.giorniPeriodo();
    this.esportazioneMetricheInCorso.set(true);

    // Si esporta il periodo selezionato con i chip, non un intervallo fisso:
    // il file deve contenere la stessa finestra dei grafici qui sopra. Il
    // numero di giorni finisce anche nel nome, altrimenti tre scaricamenti di
    // periodi diversi si sovrascrivono nella cartella dei download.
    this.adminService.esportaMetricheCsv(giorni).subscribe({
      next: (blob) => {
        this.esportazioneMetricheInCorso.set(false);
        this.scarica(blob, `metriche-${giorni}-giorni.csv`);
      },
      error: () => this.esportazioneMetricheInCorso.set(false)
    });
  }

  /**
   * Il browser non espone un "salva questo blob": l'unico modo e' un anchor
   * temporaneo da cliccare via codice. L'URL va revocato subito dopo, o il
   * blob resta in memoria per tutta la vita della pagina.
   */
  private scarica(blob: Blob, nomeFile: string): void {
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = nomeFile;
    link.click();
    URL.revokeObjectURL(url);
  }

  apriDettaglio(report: ReportDto): void {
    this.dialog.open(ReportDetailDialog, { data: report, width: '600px' });
  }

  chiudiReport(report: ReportDto): void {
    this.chiusuraInCorso.set(report.id);

    this.adminService.chiudiReport(report.id).subscribe({
      next: () => {
        this.chiusuraInCorso.set(null);
        this.caricaReports();
      },
      error: () => this.chiusuraInCorso.set(null)
    });
  }
}
