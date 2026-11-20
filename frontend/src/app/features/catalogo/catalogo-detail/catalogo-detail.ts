import { Component, ElementRef, computed, inject, signal, ViewChild } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog } from '@angular/material/dialog';
import { LibroService } from '../../../core/services/libro.service';
import { AuthService } from '../../../core/services/auth.service';
import { PrestitoService } from '../../../core/services/prestito.service';
import { NotificaService } from '../../../core/services/notifica.service';
import { RecensioneService } from '../../../core/services/recensione.service';
import { LibroDetailDto } from '../../../core/models/catalogo.model';
import { RecensioneLibroDto } from '../../../core/models/recensione.model';
import { StatoCopia } from '../../../core/models/enums';
import { resolveFileUrl } from '../../../core/utils/files';
import { EditMetadatiDialog } from '../edit-metadati-dialog/edit-metadati-dialog';
import { RichiediPrestitoDialog } from '../richiedi-prestito-dialog/richiedi-prestito-dialog';
import { RecensioneLibroDialog } from '../recensione-libro-dialog/recensione-libro-dialog';
import { BookCover, EmptyState, Skeleton } from '../../../shared/ui';
import { ElencoRecensioni, RecensioneVista } from '../../../shared/ui/elenco-recensioni/elenco-recensioni';
import { PannelloPrestito } from './pannello-prestito/pannello-prestito';
import { ElencoCopie } from './elenco-copie/elenco-copie';

/**
 * Scheda dettaglio libro.
 *
 * Nel design questa schermata parla di **un** libro con **un** proprietario:
 * copertina, metadati, recensioni della community e un pannello laterale con
 * la persona che lo presta. Il dominio dell'applicazione ha un catalogo
 * condiviso — piu' copie dello stesso titolo, di piu' proprietari — quindi il
 * layout resta quello del design (copertina | contenuto | pannello) ma il
 * pannello mostra solo la prima copia disponibile, mentre l'elenco completo
 * delle copie vive come sezione propria fra il testo e le recensioni. E' la
 * stessa estensione applicata ai pin della mappa: il
 * linguaggio visivo del design applicato a un modello di dati diverso.
 */
@Component({
  selector: 'app-catalogo-detail',
  standalone: true,
  imports: [
    DecimalPipe,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    BookCover,
    EmptyState,
    Skeleton,
    ElencoRecensioni,
    PannelloPrestito,
    ElencoCopie
  ],
  templateUrl: './catalogo-detail.html',
  styleUrl: './catalogo-detail.scss'
})
export class CatalogoDetail {
  private readonly route = inject(ActivatedRoute);
  private readonly libroService = inject(LibroService);
  private readonly prestitoService = inject(PrestitoService);
  private readonly notificaService = inject(NotificaService);
  private readonly recensioneService = inject(RecensioneService);
  private readonly dialog = inject(MatDialog);
  readonly authService = inject(AuthService);

  readonly resolveFileUrl = resolveFileUrl;

  readonly libro = signal<LibroDetailDto | null>(null);
  readonly loading = signal(false);
  readonly errore = signal(false);
  readonly copertinaInCaricamento = signal<string | null>(null);
  readonly azioneInCorso = signal<string | null>(null);
  readonly sottoscrizioneAttiva = signal(false);
  readonly recensioniLibro = signal<RecensioneLibroDto[]>([]);

  /** Prima copia disponibile di un altro utente: alimenta il pannello laterale. */
  readonly copiaDisponibile = computed(() => {
    const libro = this.libro();
    const utenteId = this.authService.currentUser()?.id;
    return (
      libro?.copie.find((c) => c.stato === StatoCopia.Disponibile && c.proprietarioId !== utenteId) ?? null
    );
  });

  readonly copieDisponibiliCount = computed(
    () => this.libro()?.copie.filter((c) => c.stato === StatoCopia.Disponibile).length ?? 0
  );

  /** Prima data di rientro fra le copie in prestito, per il messaggio di attesa. */
  readonly primoRientro = computed(() => {
    const date = (this.libro()?.copie ?? [])
      .map((c) => c.dataDisponibilitaStimata)
      .filter((d): d is string => !!d)
      .sort();
    return date[0] ?? null;
  });

  readonly recensioniVista = computed<RecensioneVista[]>(() =>
    this.recensioniLibro().map((r) => ({
      id: r.id,
      autore: r.autoreNickname,
      voto: r.voto,
      commento: r.commento,
      data: r.dataCreazione
    }))
  );

  @ViewChild('fileInput') fileInputRef?: ElementRef<HTMLInputElement>;
  private copiaPerCopertina: string | null = null;

  constructor() {
    this.carica();
  }

  private get libroId(): string {
    return this.route.snapshot.paramMap.get('id')!;
  }

  carica(): void {
    this.loading.set(true);
    this.errore.set(false);

    this.libroService.getDettaglio(this.libroId).subscribe({
      next: (libro) => {
        this.libro.set(libro);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.errore.set(true);
      }
    });

    this.recensioneService.getRecensioniLibro(this.libroId).subscribe((recensioni) => this.recensioniLibro.set(recensioni));
  }

  recensisciLibro(): void {
    this.dialog
      .open(RecensioneLibroDialog)
      .afterClosed()
      .subscribe((result) => {
        if (!result) {
          return;
        }

        this.recensioneService
          .creaRecensioneLibro({ libroId: this.libroId, voto: result.voto, commento: result.commento })
          .subscribe(() => this.carica());
      });
  }

  modificaMetadati(): void {
    const libro = this.libro();
    if (!libro) {
      return;
    }

    this.dialog
      .open(EditMetadatiDialog, { data: libro, width: '480px' })
      .afterClosed()
      .subscribe((request) => {
        if (!request) {
          return;
        }

        this.libroService.aggiornaMetadati(libro.id, request).subscribe(() => this.carica());
      });
  }

  aggiungiCopia(): void {
    this.libroService.aggiungiCopia(this.libroId).subscribe(() => this.carica());
  }

  rimuoviCopia(copiaId: string): void {
    this.libroService.rimuoviCopia(copiaId).subscribe(() => this.carica());
  }

  richiediPrestito(copiaId: string): void {
    this.dialog
      .open(RichiediPrestitoDialog)
      .afterClosed()
      .subscribe((request) => {
        if (!request) {
          return;
        }

        this.azioneInCorso.set(copiaId);

        this.prestitoService.richiediPrestito(copiaId, request).subscribe({
          next: () => {
            this.azioneInCorso.set(null);
            this.carica();
          },
          error: () => this.azioneInCorso.set(null)
        });
      });
  }

  sottoscrivi(): void {
    this.azioneInCorso.set('sottoscrizione');

    this.notificaService.sottoscrivi(this.libroId).subscribe({
      next: () => {
        this.azioneInCorso.set(null);
        this.sottoscrizioneAttiva.set(true);
      },
      error: () => this.azioneInCorso.set(null)
    });
  }

  apriSelezioneCopertina(copiaId: string): void {
    this.copiaPerCopertina = copiaId;
    this.fileInputRef?.nativeElement.click();
  }

  onFileSelezionato(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    const copiaId = this.copiaPerCopertina;

    if (!file || !copiaId) {
      return;
    }

    this.copertinaInCaricamento.set(copiaId);

    this.libroService.caricaCopertina(copiaId, file).subscribe({
      next: () => {
        this.copertinaInCaricamento.set(null);
        input.value = '';
        this.carica();
      },
      error: () => {
        this.copertinaInCaricamento.set(null);
        input.value = '';
      }
    });
  }
}
