# A portata di libro

Prototipo di **software di geolocalizzazione culturale per la condivisione del
patrimonio librario privato**, realizzato come project work universitario
(CdS Informatica per le Aziende Digitali, tema 4 — *Sharing technologies*,
traccia 14).

Gli utenti pubblicano i libri che sono disposti a prestare e scelgono in quali
**punti di ritrovo pubblici** (gli *Hub*: un bar, una piazza, una biblioteca)
renderli disponibili. Chi cerca un libro lo trova su una mappa, manda una
richiesta di prestito e si accorda in chat. La posizione esatta di chi usa
l'applicazione non viene mai salvata: la ricerca lavora sul rettangolo di mappa
visibile, e lo scambio avviene sempre in un luogo pubblico.

> **Per installare e provare il software: [`INSTALLAZIONE.md`](INSTALLAZIONE.md).**
> Prerequisiti con le versioni, i due modi di avvio — con Docker oppure con
> PostgreSQL, .NET e Node installati sulla macchina — un giro guidato delle
> schermate e i problemi che si possono incontrare. Qui sotto c'è solo la
> partenza rapida, per chi ha già Docker.

## Avvio rapido

```bash
docker compose up --build
```

Applicazione su **http://localhost:8080**. Il primo avvio richiede qualche
minuto, perché compila .NET e Angular; i successivi sono immediati. Lo schema
del database e i dati di prova vengono caricati da soli.

### Credenziali di prova

| Nome utente | Password | Ruolo |
|---|---|---|
| `admin` | `Admin1234!` | Admin — dashboard, statistiche, moderazione, vocabolario categorie |
| `giulia.rossi` | `Demo1234!` | Utente con libreria, prestiti attivi e chat |
| `marco.bianchi`, `sara.conti`, `luca.ferrari`, `elena.greco` | `Demo1234!` | Utenti |

Sono credenziali pubbliche per un ambiente locale: non vanno usate su
un'istanza raggiungibile dalla rete.

## Test

```bash
dotnet test backend/APortataDiLibro.slnx
```

```bash
npm --prefix frontend test -- --watch=false
```

I test backend (xUnit, provider EF InMemory) coprono la logica di dominio:
vincolo sull'anno di pubblicazione, hashing delle password, regole del
vocabolario delle categorie, ricerca geografica con bounding box ed esclusioni,
modifica profilo e cambio password, aggregazione delle metriche. I limiti di
quel provider — e cosa resta quindi verificato solo a mano contro PostgreSQL —
sono documentati in `backend/APortataDiLibro.Tests/TestDbContextFactory.cs`.

## Struttura del repository

| Cartella | Contenuto |
|---|---|
| `backend/` | Soluzione .NET 10 in 5 progetti: `Core` (costanti, enum, eccezioni, guardie), `Database` (entità EF, configurazioni, migrazioni), `Infrastructure` (I/O esterno: copertine con Magick.NET, notifiche), `Application` (servizi e logica di dominio), `API` (Minimal API, JWT, hub SignalR) più `Tests`. Il `Dockerfile` dell'API è qui. |
| `frontend/` | Applicazione Angular: componenti standalone, Signals, Angular Material, Leaflet per la mappa. Qui stanno anche il suo `Dockerfile` e la configurazione nginx che fa da proxy verso l'API. |
| `database/` | Script SQL di creazione schema e popolamento, copertine di esempio. Vedi `database/README.md`. |

Le dipendenze fra i cinque progetti vanno in una sola direzione — `Core` non
dipende da nulla, `API` dipende da tutti — e la struttura esiste per far
rispettare tre regole: nessuna query dentro un endpoint, nessuna regola di
dominio dentro un'entità, nessuna logica nel progetto che descrive le tabelle.

## Scelte tecniche principali

- **Nessuna estensione geospaziale.** PostGIS non è disponibile sull'ambiente
  di destinazione: le coordinate degli Hub sono due colonne `double` con indice
  composito, e la ricerca è un filtro per intervallo sul rettangolo di mappa
  inviato dal frontend.
- **Privacy by design.** La posizione del browser è usata solo per centrare la
  mappa e non lascia mai il client. Nome, cognome ed email non compaiono in
  nessuna schermata pubblica: l'identità visibile è il solo nickname. Le
  metriche di visualizzazione sono aggregate per libro e giorno, senza identità
  né orario.
- **Vocabolario controllato per le categorie.** Un'opera ha da 1 a 5 categorie
  scelte da un elenco chiuso gestito dall'Admin, non testo libero: evita che il
  vocabolario si riempia di varianti della stessa voce.
- **Chat private per costruzione.** L'Admin può leggere una conversazione solo
  se su quel prestito è stata aperta una segnalazione; non esiste un percorso
  che aggiri il vincolo.
- **Il prestito lo chiude il proprietario.** È l'unico che sa di aver riavuto
  il libro: nessun passaggio di stato automatico lo anticipa.
- **Accessibilità come requisito.** Skip link, un `<main>` per schermata,
  gerarchia delle intestazioni, contrasto WCAG AA verificato su tutte le rotte,
  nessuno scorrimento orizzontale fino a 375 px. Tre colori della palette di
  partenza sono stati scuriti per raggiungere la soglia di 4,5:1, e il valore
  prima e dopo è annotato accanto a ciascuno in `frontend/src/styles/_tokens.scss`.

## Limiti noti

Il progetto è un prototipo didattico e lo dichiara apertamente.

- I segreti presenti in `appsettings*.json` (chiave JWT, password dell'Admin di
  bootstrap) e nel `docker-compose.yml` valgono **solo per l'ambiente locale**.
  In qualunque altro contesto vanno forniti da variabili d'ambiente o
  user-secrets.
- L'avvio con Docker è quello usato per sviluppare e dimostrare, ed è l'unico
  provato a ogni modifica. L'avvio manuale è documentato passo per passo in
  [`INSTALLAZIONE.md`](INSTALLAZIONE.md) ed è stato provato per intero su
  Windows prima della consegna; su Linux e macOS è documentato ma non provato,
  e può richiedere un aggiustamento.
- Le notifiche sono simulate: vengono registrate nei log, non inviate per
  email.
- L'eliminazione autonoma dell'account non è implementata; la pagina
  `/privacy` lo dichiara.
- I ruoli sono due, `Utente` e `Admin`. Un ruolo di moderazione intermedio è
  stato valutato e rimandato.
- Diverse funzionalità sono state progettate e rimandate: ricerca per ISBN con
  recupero automatico delle copertine, proroga del prestito, recupero password,
  verifica dell'email, accesso con account social, onboarding al primo
  ingresso, impostazioni di raggio e preferenze di notifica, email
  transazionali. Nessuna di queste compare nell'interfaccia come bottone che
  non fa nulla: dove il progetto non arriva, la schermata non la promette.
