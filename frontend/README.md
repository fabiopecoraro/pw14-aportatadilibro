# Frontend

Interfaccia di *A portata di libro*: un'applicazione Angular a componenti
standalone, con stato locale tenuto in Signals, Angular Material per i
controlli e Leaflet per la mappa.

## Comandi

Dalla radice del repository:

```bash
npm --prefix frontend ci
```

```bash
npm --prefix frontend start
```

```bash
npm --prefix frontend test -- --watch=false
```

`start` serve l'applicazione su `http://localhost:4200` e chiama l'API su
`https://localhost:7163`, quindi presuppone l'API avviata e il certificato di
sviluppo .NET reso attendibile. Con Docker niente di tutto questo serve: nginx
serve il bundle di produzione e inoltra le chiamate all'API sulla stessa
origine (`nginx.conf`).

## Come è organizzato

| Cartella | Contenuto |
|---|---|
| `src/app/core/` | Ciò che non ha interfaccia: servizi HTTP, modelli dei dati scambiati con l'API, guard delle rotte, interceptor (token e gestione degli errori), funzioni pure di supporto. |
| `src/app/features/` | Una cartella per schermata: mappa, catalogo, libreria, Hub, prestiti, chat, lista dei desideri, profilo, area amministrativa, pagine pubbliche. Ogni rotta carica la propria in modo differito. |
| `src/app/layout/` | La cornice dell'area autenticata: intestazione, navigazione, barra inferiore su schermi piccoli. |
| `src/app/shared/` | Componenti riusati da più schermate: card del libro, copertina, stati vuoti, scheletri di caricamento, grafici SVG, selettore delle categorie. |
| `src/styles/` | Token di colore, spaziatura e tipografia, più gli stili che non possono stare dentro un componente (i marcatori di Leaflet, gli adattamenti di Angular Material). |

Due regole spiegano la maggior parte delle scelte che si incontrano leggendo
il codice.

La prima riguarda la **posizione**: non lascia mai il browser. La distanza fra
chi guarda e un Hub si calcola qui (`core/utils/geo.ts`), e al server arriva
soltanto l'area di mappa visibile, allargata a una griglia perché dal suo
centro non si possa risalire a nessuno.

La seconda riguarda gli **elenchi**: ognuno attraversa gli stessi quattro
stati — caricamento, contenuto, vuoto, errore — e li rende in modo distinto.
Un elenco vuoto e una richiesta fallita non devono sembrare la stessa cosa.

## Accessibilità

Ogni schermata ha un solo `<main id="contenuto-principale">`, bersaglio del
collegamento «Salta al contenuto», e un solo `h1`. I colori di testo
rispettano il contrasto di 4,5:1; i valori della palette scuriti per
arrivarci sono annotati in `src/styles/_tokens.scss`. Fino a 375 px di
larghezza nessuna schermata scorre in orizzontale.
