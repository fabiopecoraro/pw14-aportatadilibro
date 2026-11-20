import {
  AfterViewInit,
  Component,
  ElementRef,
  OnDestroy,
  computed,
  effect,
  inject,
  signal,
  viewChild
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Subject, debounceTime } from 'rxjs';
import * as L from 'leaflet';
import { MatIconModule } from '@angular/material/icon';
import { RicercaService } from '../../core/services/ricerca.service';
import { HubDisponibilitaDto, RicercaLibroDto } from '../../core/models/ricerca.model';
import { configureDefaultLeafletIcon, creaPinLibro, creaPinPosizione } from '../../core/utils/leaflet-icon';
import { MARGINE_INQUADRATURA, inquadraSuPunti } from '../../core/utils/leaflet-inquadratura';
import {
  allargaAllaGriglia,
  AreaGeografica,
  contenutoNellArea,
  Coordinate,
  distanzaMetri,
  formattaDistanza
} from '../../core/utils/geo';
import { FiltriMappa } from './filtri-mappa/filtri-mappa';
import { ElencoRisultati } from './elenco-risultati/elenco-risultati';
import { RisultatoLibro, StatoCollezione } from './mappa.model';

const ITALIA_CENTRO: L.LatLngTuple = [41.9028, 12.4964];
const ZOOM_INIZIALE = 6;
const ZOOM_POSIZIONE = 15;

/** Posizioni di aggancio del bottom sheet mobile, come da design. */
type AggancioSheet = 'peek' | 'meta' | 'esteso';

/**
 * Schermata mappa: i libri disponibili nell'area visibile.
 *
 * E' la schermata che definisce il prodotto, ed e' anche quella dove il design
 * e il dominio dell'applicazione divergono di piu'. Nel design ogni pin e' un
 * libro a casa di chi lo presta; qui i libri si ritirano presso gli Hub
 * pubblici del proprietario, quindi **un pin e' un Hub** e l'elenco resta un
 * elenco di libri, ciascuno con la distanza dell'Hub piu' vicino che lo espone.
 * La sincronia bidirezionale fra elenco e mappa richiesta dal design vale
 * comunque, mediata dall'Hub.
 *
 * Ripartizione del filtraggio: al server va solo la geografia (il rettangolo
 * visibile), al client testo e categoria. Il backend restituisce gia' tutti i libri
 * dell'area, quindi filtrarli qui da' lo stesso risultato senza un giro di rete
 * a ogni tasto, e permette di ricavare le chip delle categorie da cio' che c'e'
 * davvero nell'area invece che da un elenco fisso.
 */
@Component({
  selector: 'app-mappa',
  standalone: true,
  imports: [MatIconModule, FiltriMappa, ElencoRisultati],
  templateUrl: './mappa.html',
  styleUrl: './mappa.scss'
})
export class Mappa implements AfterViewInit, OnDestroy {
  private readonly ricercaService = inject(RicercaService);

  private readonly contenitoreMappa = viewChild.required<ElementRef<HTMLDivElement>>('contenitore');
  // Il foglio mobile e' nel DOM anche sul desktop (lo nasconde il CSS): serve
  // misurarlo, non sapere se la media query e' attiva.
  private readonly foglio = viewChild.required<ElementRef<HTMLElement>>('foglio');

  // --- Stato remoto ---------------------------------------------------------

  readonly stato = signal<StatoCollezione>('idle');
  private readonly risultatiGrezzi = signal<RicercaLibroDto[]>([]);

  /**
   * Rettangolo di mappa effettivamente sullo schermo. Al server ne arriva una
   * versione allargata alla griglia, quindi la risposta puo' contenere Hub
   * poco oltre il bordo: questo e' il confine con cui li si scarta, perche'
   * l'elenco deve corrispondere a cio' che si vede.
   */
  private readonly areaVisibile = signal<AreaGeografica | null>(null);

  /**
   * Risultati ristretti agli Hub dentro l'area visibile. E' il punto da cui
   * partono elenco, filtri e marcatori: nessuno di loro legge la risposta
   * grezza, che copre un'area piu' larga dello schermo.
   */
  private readonly risultatiNellArea = computed<RicercaLibroDto[]>(() => {
    const area = this.areaVisibile();
    if (!area) {
      return this.risultatiGrezzi();
    }

    return this.risultatiGrezzi()
      .map((libro) => ({
        ...libro,
        hubDisponibili: libro.hubDisponibili.filter((hub) => contenutoNellArea(hub, area))
      }))
      .filter((libro) => libro.hubDisponibili.length > 0);
  });

  // --- Filtri ---------------------------------------------------------------

  /** Valore digitato, aggiornato a ogni tasto per non far scattare il campo. */
  readonly testo = signal('');
  /** Valore effettivamente applicato, con il debounce di 300ms del design. */
  private readonly testoApplicato = signal('');
  readonly categoriaAttiva = signal<string | null>(null);

  private readonly testoDigitato = new Subject<string>();

  // --- Posizione ------------------------------------------------------------

  /**
   * Posizione del browser, richiesta solo su azione esplicita dell'utente e
   * tenuta esclusivamente in memoria: non finisce ne' su localStorage ne' verso
   * il backend, che riceve solo il rettangolo di mappa.
   */
  private readonly posizioneUtente = signal<Coordinate | null>(null);
  private readonly centroMappa = signal<Coordinate | null>(null);
  readonly posizioneNegata = signal(false);

  /**
   * Punto da cui si misurano le distanze: la posizione dell'utente se l'ha
   * concessa, altrimenti il centro della mappa. Il design chiede che la mappa
   * non resti mai muta quando il permesso viene negato, e questo e' il fallback
   * che permette di continuare a ordinare i risultati per vicinanza.
   */
  private readonly riferimento = computed(() => this.posizioneUtente() ?? this.centroMappa());

  // --- Selezione ------------------------------------------------------------

  readonly libroSelezionatoId = signal<string | null>(null);
  readonly libroDaScorrere = signal<string | null>(null);
  private readonly hubSelezionatoId = signal<string | null>(null);
  private readonly hubEvidenziatoId = signal<string | null>(null);

  // --- Bottom sheet mobile --------------------------------------------------

  readonly agganciosheet = signal<AggancioSheet>('meta');
  readonly altezzaTrascinata = signal<number | null>(null);
  private inizioTrascinamentoY = 0;
  private altezzaIniziale = 0;

  // --- Derivati -------------------------------------------------------------

  /**
   * Categorie presenti nell'area visibile, per le chip di filtro. Si ricavano
   * da cio' che c'e' davvero nell'area e non dal vocabolario completo: una
   * chip che non seleziona nulla e' peggio di una chip assente.
   */
  readonly categorieDisponibili = computed(() =>
    [...new Set(this.risultatiNellArea().flatMap((libro) => libro.categorie.map((c) => c.nome)))].sort(
      (a, b) => a.localeCompare(b, 'it')
    )
  );

  /**
   * Risultati filtrati, arricchiti con l'Hub piu' vicino e ordinati per
   * distanza crescente (per titolo quando non c'e' un punto di riferimento).
   */
  readonly risultati = computed<RisultatoLibro[]>(() => {
    const testo = this.testoApplicato().trim().toLocaleLowerCase('it');
    const categoria = this.categoriaAttiva();
    const da = this.riferimento();

    return this.risultatiNellArea()
      .filter((libro) => {
        if (categoria && !libro.categorie.some((c) => c.nome === categoria)) {
          return false;
        }
        if (!testo) {
          return true;
        }
        // Il campo del design cerca in "titolo o autore": il backend applica i
        // due filtri in AND, quindi l'alternativa si risolve qui.
        return (
          libro.titolo.toLocaleLowerCase('it').includes(testo) ||
          libro.autore.toLocaleLowerCase('it').includes(testo)
        );
      })
      .map((libro) => this.componiRisultato(libro, da))
      .filter((risultato): risultato is RisultatoLibro => risultato !== null)
      .sort((a, b) => {
        if (a.distanza !== null && b.distanza !== null) {
          return a.distanza - b.distanza;
        }
        return a.titolo.localeCompare(b.titolo, 'it');
      });
  });

  /** Hub che espongono almeno un libro fra quelli filtrati. */
  private readonly hubVisibili = computed(() => {
    const hub = new Map<string, HubDisponibilitaDto>();
    for (const risultato of this.risultati()) {
      hub.set(risultato.hubPiuVicino.hubId, risultato.hubPiuVicino);
    }
    return [...hub.values()];
  });

  private mappa?: L.Map;
  private readonly marcatori = new Map<string, L.Marker>();
  private marcatorePosizione?: L.Marker;
  private mappaPronta = signal(false);

  /**
   * La mappa parte sull'Italia intera perche' l'applicazione non presume dove
   * si trovi chi la apre: la prima ricerca copre quindi tutto il paese e
   * restituisce ogni Hub esistente. Appena quei risultati arrivano, la vista
   * si stringe su di essi, cosi' la schermata principale non si apre su un
   * puntino in mezzo a una mappa vuota.
   *
   * Si fa **una volta sola**, e questo flag e' la ragione: inquadrare gli Hub
   * sposta la mappa, lo spostamento rilancia la ricerca, i nuovi risultati
   * rilancerebbero l'inquadratura. Senza il flag sarebbe un ciclo.
   */
  private inquadraturaInizialeFatta = false;

  constructor() {
    this.testoDigitato
      .pipe(debounceTime(300), takeUntilDestroyed())
      .subscribe((testo) => this.testoApplicato.set(testo));

    // I marcatori sono un riflesso dello stato: invece di aggiungerli e
    // rimuoverli dai vari gestori di evento, un solo effetto riconcilia la mappa
    // con i risultati filtrati e con la selezione corrente.
    effect(() => this.sincronizzaMarcatori());
  }

  ngAfterViewInit(): void {
    configureDefaultLeafletIcon();

    this.mappa = L.map(this.contenitoreMappa().nativeElement, {
      // I controlli di zoom vanno in basso a destra, non in alto a sinistra.
      zoomControl: false
    }).setView(ITALIA_CENTRO, ZOOM_INIZIALE);

    // Leaflet etichetta i due pulsanti in inglese: i titoli sono anche il loro
    // nome per chi usa un lettore di schermo.
    L.control
      .zoom({ position: 'bottomright', zoomInTitle: 'Ingrandisci', zoomOutTitle: 'Riduci' })
      .addTo(this.mappa);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '&copy; OpenStreetMap contributors',
      maxZoom: 19
    }).addTo(this.mappa);

    this.mappa.on('moveend', () => {
      this.aggiornaCentro();
      this.cerca();
    });

    // Se l'utente tocca la mappa prima che arrivino i primi risultati, la sua
    // inquadratura vince: non gliela si sposta sotto le mani.
    this.mappa.on('dragstart zoomstart', () => {
      this.inquadraturaInizialeFatta = true;
    });

    this.aggiornaCentro();
    this.mappaPronta.set(true);
    this.cerca();
  }

  ngOnDestroy(): void {
    this.mappa?.remove();
  }

  // --- Azioni ---------------------------------------------------------------

  cerca(): void {
    if (!this.mappa) {
      return;
    }

    const confini = this.mappa.getBounds();
    const area: AreaGeografica = {
      sud: confini.getSouth(),
      ovest: confini.getWest(),
      nord: confini.getNorth(),
      est: confini.getEast()
    };
    this.areaVisibile.set(area);
    this.stato.set(this.risultatiGrezzi().length === 0 ? 'loading' : this.stato());

    // Al server va l'area allargata alla griglia, non quella esatta: dopo
    // "La tua posizione" la mappa e' centrata sull'utente, e il centro del
    // rettangolo esatto ne rivelerebbe la posizione.
    this.ricercaService
      .cercaLibri(allargaAllaGriglia(area))
      .subscribe({
        next: (risultati) => {
          this.risultatiGrezzi.set(risultati);
          this.stato.set('ready');
          this.inquadraSuHub(risultati);
        },
        error: () => this.stato.set('error')
      });
  }

  aggiornaTesto(testo: string): void {
    this.testo.set(testo);
    this.testoDigitato.next(testo);
  }

  aggiornaCategoria(categoria: string | null): void {
    this.categoriaAttiva.set(categoria);
  }

  /** Selezione dall'elenco: evidenzia l'Hub e ci porta sopra la mappa. */
  selezionaDaElenco(risultato: RisultatoLibro): void {
    this.libroSelezionatoId.set(risultato.libroId);
    this.hubSelezionatoId.set(risultato.hubPiuVicino.hubId);
    this.mappa?.panTo([risultato.hubPiuVicino.lat, risultato.hubPiuVicino.lng]);
  }

  /** Hover su una card: ingrandisce il pin corrispondente senza selezionarlo. */
  evidenzia(libroId: string | null): void {
    if (!libroId) {
      this.hubEvidenziatoId.set(null);
      return;
    }

    const risultato = this.risultati().find((voce) => voce.libroId === libroId);
    this.hubEvidenziatoId.set(risultato?.hubPiuVicino.hubId ?? null);
  }

  localizza(): void {
    if (!navigator.geolocation) {
      this.posizioneNegata.set(true);
      return;
    }

    navigator.geolocation.getCurrentPosition(
      (posizione) => {
        const coordinate: Coordinate = {
          lat: posizione.coords.latitude,
          lng: posizione.coords.longitude
        };

        this.posizioneNegata.set(false);
        this.posizioneUtente.set(coordinate);
        this.mostraMarcatorePosizione(coordinate);
        this.mappa?.setView([coordinate.lat, coordinate.lng], ZOOM_POSIZIONE);
      },
      () => this.posizioneNegata.set(true),
      { enableHighAccuracy: false, timeout: 10000 }
    );
  }

  // --- Bottom sheet ---------------------------------------------------------

  cambiaAggancio(): void {
    const ordine: AggancioSheet[] = ['peek', 'meta', 'esteso'];
    const successivo = ordine[(ordine.indexOf(this.agganciosheet()) + 1) % ordine.length];
    this.agganciosheet.set(successivo);
  }

  iniziaTrascinamento(evento: PointerEvent): void {
    const maniglia = evento.currentTarget as HTMLElement;
    maniglia.setPointerCapture(evento.pointerId);

    this.inizioTrascinamentoY = evento.clientY;
    this.altezzaIniziale = maniglia.closest('.mappa-sheet')?.getBoundingClientRect().height ?? 0;
    this.altezzaTrascinata.set(this.altezzaIniziale);
  }

  trascina(evento: PointerEvent): void {
    if (this.altezzaTrascinata() === null) {
      return;
    }

    // Verso invertito: trascinando verso l'alto il foglio cresce.
    const delta = this.inizioTrascinamentoY - evento.clientY;
    const massimo = window.innerHeight * 0.9;
    this.altezzaTrascinata.set(Math.min(Math.max(this.altezzaIniziale + delta, 120), massimo));
  }

  concludiTrascinamento(): void {
    const altezza = this.altezzaTrascinata();
    if (altezza === null) {
      return;
    }

    // Si aggancia alla posizione piu' vicina fra le tre previste, invece di
    // restare dove il dito ha lasciato: il design definisce tre stati, non una
    // altezza libera.
    const disponibile = window.innerHeight;
    const distanze: Array<[AggancioSheet, number]> = [
      ['peek', Math.abs(altezza - 120)],
      ['meta', Math.abs(altezza - disponibile * 0.5)],
      ['esteso', Math.abs(altezza - disponibile * 0.85)]
    ];

    this.agganciosheet.set(distanze.sort((a, b) => a[1] - b[1])[0][0]);
    this.altezzaTrascinata.set(null);
  }

  // --- Interni --------------------------------------------------------------

  private componiRisultato(libro: RicercaLibroDto, da: Coordinate | null): RisultatoLibro | null {
    if (libro.hubDisponibili.length === 0) {
      return null;
    }

    const ordinati = da
      ? [...libro.hubDisponibili].sort(
          (a, b) =>
            distanzaMetri(da, { lat: a.lat, lng: a.lng }) -
            distanzaMetri(da, { lat: b.lat, lng: b.lng })
        )
      : libro.hubDisponibili;

    const piuVicino = ordinati[0];
    const distanza = da ? distanzaMetri(da, { lat: piuVicino.lat, lng: piuVicino.lng }) : null;

    return {
      libroId: libro.libroId,
      titolo: libro.titolo,
      autore: libro.autore,
      categorie: libro.categorie,
      anno: libro.anno,
      hubPiuVicino: piuVicino,
      distanza,
      distanzaTesto: distanza === null ? '' : formattaDistanza(distanza),
      altriHub: libro.hubDisponibili.length - 1
    };
  }

  /**
   * Stringe la vista sugli Hub appena ricevuti, una volta sola per sessione
   * di schermata (vedi `inquadraturaInizialeFatta` per il perche').
   */
  private inquadraSuHub(risultati: RicercaLibroDto[]): void {
    if (this.inquadraturaInizialeFatta || !this.mappa) {
      return;
    }

    const punti = risultati
      .flatMap((libro) => libro.hubDisponibili)
      .map((hub) => [hub.lat, hub.lng] as L.LatLngTuple);

    // Area senza Hub: non c'e' nulla su cui stringere. Si esce senza alzare il
    // flag, cosi' l'inquadratura resta disponibile per i primi risultati utili
    // invece di essere consumata da una risposta vuota.
    if (punti.length === 0) {
      return;
    }

    this.inquadraturaInizialeFatta = true;
    inquadraSuPunti(this.mappa, punti, this.margineInferiore());
  }

  /**
   * Margine da riservare in basso nell'inquadratura. `fitBounds` ragiona
   * sull'intero contenitore della mappa, ma su mobile il bottom sheet ne copre
   * la parte bassa: con un margine simmetrico i pin finivano centrati dietro al
   * foglio e la porzione di mappa effettivamente visibile restava vuota.
   *
   * Si misura la sovrapposizione invece di leggere il 50% scritto nel CSS,
   * perche' l'altezza del foglio dipende dall'aggancio corrente e l'utente puo'
   * averlo trascinato a mano. Sul desktop il foglio e' `display: none`, il
   * rettangolo e' tutto a zero e resta il solo margine di respiro.
   */
  private margineInferiore(): number {
    const foglio = this.foglio().nativeElement.getBoundingClientRect();
    if (foglio.height === 0) {
      return MARGINE_INQUADRATURA;
    }

    const tela = this.contenitoreMappa().nativeElement.getBoundingClientRect();
    const sovrapposizione = Math.max(0, tela.bottom - foglio.top);
    return MARGINE_INQUADRATURA + sovrapposizione;
  }

  private aggiornaCentro(): void {
    const centro = this.mappa?.getCenter();
    if (centro) {
      this.centroMappa.set({ lat: centro.lat, lng: centro.lng });
    }
  }

  private mostraMarcatorePosizione(coordinate: Coordinate): void {
    if (!this.mappa) {
      return;
    }

    this.marcatorePosizione?.remove();
    this.marcatorePosizione = L.marker([coordinate.lat, coordinate.lng], {
      icon: creaPinPosizione(),
      // Sotto ai pin dei contenuti: e' un riferimento, non un risultato.
      zIndexOffset: -100,
      keyboard: false
    }).addTo(this.mappa);
  }

  private sincronizzaMarcatori(): void {
    const hub = this.hubVisibili();
    const selezionato = this.hubSelezionatoId();
    const evidenziato = this.hubEvidenziatoId();

    if (!this.mappaPronta() || !this.mappa) {
      return;
    }

    const attesi = new Set(hub.map((voce) => voce.hubId));

    for (const [hubId, marcatore] of this.marcatori) {
      if (!attesi.has(hubId)) {
        marcatore.remove();
        this.marcatori.delete(hubId);
      }
    }

    for (const voce of hub) {
      const stato =
        voce.hubId === selezionato
          ? 'selezionato'
          : voce.hubId === evidenziato
            ? 'evidenziato'
            : 'normale';

      const etichetta =
        voce.hubId === selezionato
          ? (this.risultati().find((r) => r.hubPiuVicino.hubId === voce.hubId)?.distanzaTesto ?? '')
          : '';

      const icona = creaPinLibro(stato, etichetta);
      const esistente = this.marcatori.get(voce.hubId);

      if (esistente) {
        esistente.setIcon(icona);
        continue;
      }

      const marcatore = L.marker([voce.lat, voce.lng], {
        icon: icona,
        title: voce.nome
      })
        .addTo(this.mappa)
        .on('click', () => this.selezionaDaMappa(voce.hubId));

      this.marcatori.set(voce.hubId, marcatore);
    }
  }

  /** Selezione dalla mappa: porta in vista la prima card di quell'Hub. */
  private selezionaDaMappa(hubId: string): void {
    this.hubSelezionatoId.set(hubId);

    const primo = this.risultati().find((voce) => voce.hubPiuVicino.hubId === hubId);
    if (primo) {
      this.libroSelezionatoId.set(primo.libroId);
      this.libroDaScorrere.set(primo.libroId);
    }

    // Sul mobile il foglio potrebbe essere ridotto al minimo: si apre a meta'
    // altezza perche' la card selezionata sia effettivamente visibile.
    if (this.agganciosheet() === 'peek') {
      this.agganciosheet.set('meta');
    }
  }
}
