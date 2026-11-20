# Guida all'installazione

Il progetto si avvia in due modi, entrambi provati dall'inizio alla fine. Docker
non è un requisito: è una comodità per chi lo ha già.

**Con Docker** serve un solo comando: un contenitore costruisce il database e
lo popola, uno compila ed esegue l'API, uno serve l'interfaccia. Non c'è nulla
da installare sulla macchina oltre a Docker, e il risultato è identico su
Windows, macOS e Linux.

**Senza Docker** servono PostgreSQL, .NET e Node installati sulla macchina, e
sei passaggi. È il modo naturale per chi ha già questi strumenti, e per leggere
il codice mentre gira con un debugger attaccato.

### In breve, senza Docker

Che cosa serve, come lo si direbbe a un collega:

- **PostgreSQL 17 o 18** in ascolto su `localhost:5432`, utente `postgres` con
  password `postgres` (oppure la propria, indicata nella stringa di
  connessione);
- **.NET SDK 10**;
- **Node.js 24 LTS** o successivo;
- le porte **4200**, **7163** e **5190** libere.

Che cosa si fa: si crea il database e vi si caricano i due script di
`database/`, si copiano le copertine di esempio, si rende attendibile il
certificato di sviluppo di .NET, si installano le dipendenze dell'interfaccia e
si avviano API e interfaccia in due terminali. I comandi, uno per uno, sono
nella [sezione 3](#3-percorso-b--avvio-manuale).

---

## 1. Prima di cominciare

### Percorso Docker

| Serve | Versione | Dove si prende |
|---|---|---|
| Docker Desktop (Windows, macOS) | 4.x | <https://www.docker.com/products/docker-desktop/> |
| oppure Docker Engine + plugin compose (Linux) | Engine 24+, compose v2 | <https://docs.docker.com/engine/install/> |

Servono inoltre **circa 3 GB di spazio su disco** per le immagini e la porta
**8080** libera. Nient'altro: né .NET, né Node, né PostgreSQL.

### Percorso manuale

Le versioni indicate sono quelle con cui il progetto è stato costruito e
provato, non il minimo teorico che potrebbe funzionare.

| Serve | Versione | Nota |
|---|---|---|
| .NET SDK | **10** | `global.json` vincola la famiglia: con un SDK 9 il ripristino si ferma subito, con un errore esplicito |
| Node.js | **24 LTS** (provata in build) o **26** | Angular 22 rifiuta tutto ciò che precede la 22.12 |
| PostgreSQL | **17** (provato anche con la **18**), in ascolto su `localhost:5432` | per caricare gli script serve `psql` raggiungibile dal `PATH`, oppure pgAdmin |

Su **Linux** l'API ha due dipendenze native che su Windows e macOS sono già
nel sistema:

```bash
sudo apt-get install libgomp1 fontconfig
```

Magick.NET, usato per ridimensionare le copertine, porta con sé le proprie
librerie ma si appoggia a OpenMP e ai font di sistema. Senza quei due
pacchetti il progetto compila e parte regolarmente, e fallisce al primo
caricamento di una copertina: è un errore che si manifesta tardi, quindi si
installano prima.

Porte usate da questo percorso: **4200** (interfaccia), **7163** e **5190**
(API), **5432** (database).

---

## 2. Percorso A — Docker

Dalla cartella del progetto:

```bash
docker compose up --build
```

Il primo avvio richiede **qualche minuto**: scarica le immagini di base,
compila i cinque progetti .NET e costruisce il bundle Angular di produzione. I
successivi partono in pochi secondi, perché Docker riusa i livelli già
costruiti. Nel terminale scorrono tre flussi di log con un prefisso per
servizio: `db`, `api`, `frontend`.

L'ambiente è pronto quando compaiono le righe di Kestrel in ascolto e nginx
non segnala errori. Per controllarlo da un altro terminale:

```bash
docker compose ps
```

Devono comparire tre servizi in stato `running`, e `db` anche `healthy`.

Poi apri **<http://localhost:8080>**.

### Che cosa è appena successo

Il database non è vuoto. Al primo avvio PostgreSQL esegue gli script montati
in `database/`: `01-schema.sql` crea le diciassette tabelle, `02-seed.sql`
carica i dati di prova — sei Hub reali a Salerno, ventisei opere, prestiti in
diversi stati, conversazioni e quarantacinque giorni di visualizzazioni. Non
serve registrare un utente per vedere l'applicazione piena.

L'interfaccia e l'API stanno **sulla stessa origine**: nginx serve il bundle
Angular e inoltra `/api`, `/covers` e `/hubs` all'API. È la ragione principale
per cui esiste il compose — fuori da lì l'interfaccia chiama l'API in HTTPS su
un'altra porta e il browser rifiuta il certificato di sviluppo .NET finché non
lo si rende attendibile sulla macchina.

### Fermare e ripartire

```bash
docker compose down
```

Ferma tutto e conserva il database. Per ripartire da un database pulito, con i
dati di prova ricaricati da zero:

```bash
docker compose down -v && docker compose up --build
```

Il `-v` elimina il volume dei dati. Senza di quello gli script di
inizializzazione **non** vengono rieseguiti, perché PostgreSQL li esegue solo
su una cartella dati vuota: è il motivo per cui una modifica al seed sembra
non avere effetto.

---

## 3. Percorso B — avvio manuale

### Passo 1 — database, schema e dati

```bash
psql -h localhost -U postgres -c "CREATE DATABASE aportatadilibro;"
```

```bash
psql -h localhost -U postgres -d aportatadilibro -v ON_ERROR_STOP=1 -f database/01-schema.sql
```

```bash
psql -h localhost -U postgres -d aportatadilibro -v ON_ERROR_STOP=1 -f database/02-seed.sql
```

La connessione predefinita è `Host=localhost;Port=5432;Database=aportatadilibro;Username=postgres;Password=postgres`
(in `backend/APortataDiLibro.API/appsettings.json`). Se la tua installazione di
PostgreSQL usa un'altra password, cambiala lì o passala con la variabile
d'ambiente `ConnectionStrings__DefaultConnection`.

### Passo 2 — copertine di esempio

```bash
mkdir -p backend/APortataDiLibro.API/wwwroot/covers && cp database/seed-covers/*.jpg backend/APortataDiLibro.API/wwwroot/covers/
```

Passaggio necessario solo in questa modalità: con Docker le copia il compose
all'avvio. Senza di esso l'applicazione funziona, ma al
posto delle copertine compaiono i segnaposto.

### Passo 3 — certificato di sviluppo

```bash
dotnet dev-certs https --trust
```

Una volta sola per macchina. In questa modalità l'interfaccia chiama l'API in
HTTPS: se il certificato non è attendibile il browser rifiuta le richieste e
l'applicazione resta vuota senza un messaggio di errore chiaro.

### Passo 4 — dipendenze del frontend

```bash
npm --prefix frontend ci
```

Qualche minuto la prima volta. `npm ci` e non `npm install`: installa le
versioni esatte del `package-lock.json`, che è quello con cui il progetto è
stato provato.

### Passo 5 — API

```bash
dotnet run --project backend/APortataDiLibro.API --launch-profile https
```

**Il profilo va indicato.** `launchSettings.json` dichiara due profili e
`dotnet run` senza argomenti prende il primo: se quello è `http`, l'API parte
sulla sola porta 5190 mentre l'interfaccia continua a chiamare la 7163, e non
trova nessuno. Il profilo `https` è il primo in elenco proprio per questo, ma
indicarlo rende il comando indipendente dall'ordine.

L'API non crea lo schema da sé: lo trova già pronto dal passo 1. All'avvio si
limita a creare l'utente Admin di bootstrap, se non esiste.

### Passo 6 — interfaccia

In un secondo terminale:

```bash
npm --prefix frontend start
```

Interfaccia su **<http://localhost:4200>**, API su
**<https://localhost:7163>**. La documentazione Swagger degli endpoint, in
questa modalità, è su **<https://localhost:7163/swagger>**.

---

## 4. Primo accesso

| Nome utente | Password | Che cosa ci si trova |
|---|---|---|
| `giulia.rossi` | `Demo1234!` | una libreria popolata, prestiti attivi in più stati, conversazioni in corso |
| `marco.bianchi`, `sara.conti`, `luca.ferrari`, `elena.greco` | `Demo1234!` | utenti con libri e prestiti, utili per guardare lo scambio dai due lati |
| `admin` | `Admin1234!` | dashboard, statistiche aggregate, moderazione, vocabolario delle categorie |

Sono credenziali pubbliche per un ambiente locale: non vanno usate su
un'istanza raggiungibile dalla rete.

## 5. Giro guidato in cinque minuti

Il percorso che mostra le parti che contano, nell'ordine in cui hanno senso.

1. **Senza autenticazione**, da `/come-funziona`: la spiegazione del servizio
   e il modello degli Hub. La pagina `/privacy` dichiara quali dati vengono
   trattati e quali no — vale la pena leggerla, è parte del progetto e non un
   adempimento.
2. **Entra come `giulia.rossi`** e apri **`/mappa`**. Gli Hub sono sei luoghi
   reali di Salerno. Muovendo o ingrandendo la mappa la ricerca si rifà sul
   rettangolo visibile: al server arriva l'area guardata, mai la posizione di
   chi guarda. I marcatori si raggiungono anche da tastiera, con `Tab`.
3. **`/catalogo`**, e da lì la scheda di un'opera. La scheda elenca le copie
   disponibili e chi le presta; il ritiro avviene in uno degli Hub del
   proprietario, che si vedono sulla mappa. I metadati dell'opera sono
   condivisi fra tutte le copie e solo l'Admin può correggerli.
4. **Chiedi un prestito** su una copia non tua. Il prestito nasce
   `Richiesto`: in `/prestiti` lo vedi dal lato di chi ha chiesto.
5. **Esci ed entra come il proprietario** di quella copia — il nickname è
   sulla scheda. Da `/prestiti` lo accetta (`Accettato`), e si apre la
   conversazione in `/chat` per accordarsi su quando trovarsi all'Hub. Con due
   finestre del browser affiancate, i messaggi arrivano da soli: la chat è su
   SignalR, non su polling.
6. **Chiudi il prestito dal lato del proprietario** (`Completato`). Solo lui
   può farlo: è l'unico che sa di aver riavuto il libro. Dopo la chiusura il
   richiedente può lasciare una recensione.
7. **Entra come `admin`** e apri **`/admin`**: statistiche aggregate,
   segnalazioni e vocabolario delle categorie. Le categorie sono un elenco
   chiuso, da 1 a 5 per opera, e si gestiscono da qui — in nessun form
   dell'applicazione si può digitare una categoria a mano.

## 6. Verificare l'ambiente

```bash
dotnet test backend/APortataDiLibro.slnx
```

```bash
npm --prefix frontend test -- --watch=false
```

Ottantatré test backend e diciotto frontend. Non toccano il database reale:
usano il provider EF in memoria, i cui limiti sono dichiarati in
`backend/APortataDiLibro.Tests/TestDbContextFactory.cs`.

---

## 7. Se qualcosa non va

**`failed to connect to the docker API` oppure `Impossibile trovare il file
specificato`** — Docker Desktop non è in esecuzione. Avvialo e aspetta che
l'icona smetta di animarsi prima di ripetere il comando.

**La porta 8080 è occupata.** Cambia la pubblicazione nel
`docker-compose.yml`, nel servizio `frontend`: `"9090:80"` al posto di
`"8080:80"`, e poi apri `http://localhost:9090`. Non serve toccare nient'altro:
il proxy lavora dentro la rete di compose.

**La porta 5432 è occupata** (percorso manuale, o un PostgreSQL già installato
che convive con quello di Docker). Nel percorso Docker il database non
pubblica alcuna porta sull'host, quindi il conflitto non esiste; nel percorso
manuale cambia la porta nella stringa di connessione.

**Il primo `docker compose up` sembra bloccato.** La compilazione di .NET e
Angular non stampa nulla per minuti interi. Si riconosce che sta lavorando dal
fatto che `docker stats` mostra la CPU occupata. Se Docker Desktop ha meno di
4 GB di memoria assegnati, la build di Angular può terminare da sola senza
spiegazioni: in quel caso alza il limite nelle impostazioni di Docker.

**L'interfaccia si apre ma è vuota, e la console del browser mostra errori di
rete** (percorso manuale). È quasi sempre il certificato: ripeti
`dotnet dev-certs https --trust` e riapri il browser. Verifica anche di aver
avviato l'API con `--launch-profile https`.

**Le copertine non compaiono** (percorso manuale). Manca il passo 2, la copia
in `wwwroot/covers/`.

**`psql` non è un comando riconosciuto.** PostgreSQL è installato ma i suoi
binari non sono nel `PATH`: su Windows stanno in
`C:\Program Files\PostgreSQL\17\bin`. In alternativa si usa pgAdmin per
eseguire i due script SQL.

**Una modifica al seed non ha effetto.** Gli script di inizializzazione
vengono eseguiti solo su un database vuoto: serve `docker compose down -v`.
