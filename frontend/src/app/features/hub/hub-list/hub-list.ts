import { AfterViewInit, Component, ElementRef, OnDestroy, ViewChild, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import * as L from 'leaflet';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { HubService } from '../../../core/services/hub.service';
import { HubDto } from '../../../core/models/hub.model';
import { configureDefaultLeafletIcon, creaPinLibro } from '../../../core/utils/leaflet-icon';
import { inquadraSuPunti } from '../../../core/utils/leaflet-inquadratura';
import { EmptyState, Skeleton } from '../../../shared/ui';
import { StatoCollezione } from '../../mappa/mappa.model';

const ITALIA_CENTRO: L.LatLngTuple = [41.9028, 12.4964];

/**
 * I miei Hub — gestione dei punti di ritrovo pubblici usati per prestare i
 * propri libri (bar, biblioteche, piazze). Non ha una schermata di
 * riferimento nel design, che non prevede Hub: il modello del design
 * geolocalizza i libri a casa del proprietario. Qui i pin sono resi con
 * `creaPinLibro`, la stessa fabbrica usata dalla mappa: un Hub, sulla mappa,
 * è visivamente lo stesso genere di punto che la mappa mostra ai vicini.
 */
@Component({
  selector: 'app-hub-list',
  standalone: true,
  imports: [ReactiveFormsModule, MatButtonModule, MatIconModule, MatProgressSpinnerModule, EmptyState, Skeleton],
  templateUrl: './hub-list.html',
  styleUrl: './hub-list.scss'
})
export class HubList implements AfterViewInit, OnDestroy {
  private readonly hubService = inject(HubService);
  private readonly fb = inject(FormBuilder);

  @ViewChild('mapContainer') private mapContainerRef!: ElementRef<HTMLDivElement>;

  readonly hubs = signal<HubDto[]>([]);
  readonly stato = signal<StatoCollezione>('idle');
  readonly salvataggioInCorso = signal(false);
  readonly eliminazioneInCorso = signal<string | null>(null);
  readonly nuovaPosizione = signal<{ lat: number; lng: number } | null>(null);

  readonly form = this.fb.group({
    nome: ['', Validators.required],
    indirizzo: ['', Validators.required]
  });

  private map?: L.Map;
  private hubMarkers: L.Marker[] = [];
  private nuovoMarker?: L.Marker;

  /**
   * La mappa parte sull'Italia intera perche' l'applicazione non presume dove
   * si trovi chi la apre; appena arrivano gli Hub dell'utente la vista si
   * stringe su di essi, altrimenti chi ne ha due si vede un puntino in mezzo
   * alla penisola. Chi non ne ha ancora nessuno resta sulla vista d'insieme: e'
   * la schermata in cui gli si chiede di cliccare un punto, e partire ingranditi
   * su un luogo scelto da noi sarebbe peggio.
   *
   * Una volta sola, come sulla mappa di ricerca: `carica()` viene richiamata
   * anche dopo un salvataggio o una cancellazione, e reinquadrare allora
   * sposterebbe la vista mentre l'utente sta ancora lavorando su quella zona.
   */
  private inquadraturaInizialeFatta = false;

  ngAfterViewInit(): void {
    configureDefaultLeafletIcon();

    this.map = L.map(this.mapContainerRef.nativeElement, { zoomControl: false }).setView(ITALIA_CENTRO, 6);
    // Leaflet etichetta i due pulsanti in inglese: i titoli sono anche il loro
    // nome per chi usa un lettore di schermo.
    L.control
      .zoom({ position: 'bottomright', zoomInTitle: 'Ingrandisci', zoomOutTitle: 'Riduci' })
      .addTo(this.map);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '&copy; OpenStreetMap contributors',
      maxZoom: 19
    }).addTo(this.map);

    this.map.on('click', (event: L.LeafletMouseEvent) => this.onMapClick(event));

    // Se l'utente tocca la mappa prima che arrivino gli Hub, la sua
    // inquadratura vince: non gliela si sposta sotto le mani.
    this.map.on('dragstart zoomstart', () => {
      this.inquadraturaInizialeFatta = true;
    });

    this.carica();
  }

  ngOnDestroy(): void {
    this.map?.remove();
  }

  carica(): void {
    this.stato.set(this.hubs().length === 0 ? 'loading' : this.stato());

    this.hubService.getHubUtente().subscribe({
      next: (hubs) => {
        this.hubs.set(hubs);
        this.stato.set('ready');
        this.aggiornaMarkerHub(hubs);
        this.inquadraSuHub(hubs);
      },
      error: () => this.stato.set('error')
    });
  }

  salva(): void {
    const posizione = this.nuovaPosizione();

    if (this.form.invalid || !posizione) {
      this.form.markAllAsTouched();
      return;
    }

    this.salvataggioInCorso.set(true);

    this.hubService
      .creaHub({
        nome: this.form.value.nome!,
        indirizzo: this.form.value.indirizzo!,
        lat: posizione.lat,
        lng: posizione.lng
      })
      .subscribe({
        next: () => {
          this.salvataggioInCorso.set(false);
          this.annulla();
          this.carica();
        },
        error: () => this.salvataggioInCorso.set(false)
      });
  }

  annulla(): void {
    this.form.reset();
    this.nuovaPosizione.set(null);
    this.nuovoMarker?.remove();
    this.nuovoMarker = undefined;
  }

  elimina(id: string): void {
    this.eliminazioneInCorso.set(id);

    this.hubService.eliminaHub(id).subscribe({
      next: () => {
        this.eliminazioneInCorso.set(null);
        this.carica();
      },
      error: () => this.eliminazioneInCorso.set(null)
    });
  }

  /** Stringe la vista sugli Hub dell'utente (vedi `inquadraturaInizialeFatta`). */
  private inquadraSuHub(hubs: HubDto[]): void {
    // Senza Hub non c'e' nulla su cui stringere: si esce senza consumare
    // l'occasione, cosi' vale ancora quando il primo Hub viene registrato.
    if (this.inquadraturaInizialeFatta || !this.map || hubs.length === 0) {
      return;
    }

    this.inquadraturaInizialeFatta = true;
    inquadraSuPunti(
      this.map,
      hubs.map((hub) => [hub.lat, hub.lng] as L.LatLngTuple)
    );
  }

  /** Sceglie come posizione del nuovo Hub il punto al centro della mappa. */
  usaCentroMappa(): void {
    const centro = this.map?.getCenter();
    if (centro) {
      this.scegliPosizione(centro.lat, centro.lng);
    }
  }

  private onMapClick(event: L.LeafletMouseEvent): void {
    const { lat, lng } = event.latlng;
    this.scegliPosizione(lat, lng);
  }

  private scegliPosizione(lat: number, lng: number): void {
    this.nuovaPosizione.set({ lat, lng });

    // Il clic e' una scelta di posizione, non una semplice navigazione: da qui
    // in avanti l'inquadratura e' dell'utente, altrimenti il salvataggio del
    // primo Hub gli strapperebbe la vista verso il pin appena piazzato.
    this.inquadraturaInizialeFatta = true;

    if (this.nuovoMarker) {
      this.nuovoMarker.setLatLng([lat, lng]);
    } else {
      this.nuovoMarker = L.marker([lat, lng], { icon: creaPinLibro('selezionato'), keyboard: false }).addTo(
        this.map!
      );
    }
  }

  private aggiornaMarkerHub(hubs: HubDto[]): void {
    this.hubMarkers.forEach((marker) => marker.remove());
    this.hubMarkers = hubs.map((hub) =>
      // `title` non e' un dettaglio estetico: Leaflet rende ogni marcatore un
      // elemento con `role="button"` raggiungibile da tastiera, e senza titolo
      // uno screen reader annuncerebbe solo "pulsante" tante volte quanti sono
      // gli Hub. E' la stessa opzione gia' usata dai pin della mappa principale.
      L.marker([hub.lat, hub.lng], { icon: creaPinLibro('normale'), title: hub.nome })
        .addTo(this.map!)
        .bindPopup(`<strong>${hub.nome}</strong><br>${hub.indirizzo}`)
    );
  }
}
