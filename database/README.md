# Database — script di creazione e popolamento

Script SQL per PostgreSQL corrispondenti al punto 3 dell'elaborato richiesto
dalla traccia ("Database con script di creazione schema e esempi di
popolamento con dati di test").

| File | Contenuto |
|---|---|
| `01-schema.sql` | Schema completo: tabelle, chiavi, vincoli e indici, incluso l'indice composito `IX_Hub_Lat_Lng` usato dalla ricerca geografica. |
| `02-seed.sql` | Dataset di prova: 6 utenti, 6 Hub a Salerno, 9 categorie, 26 opere (quasi tutte classificate con più di una categoria), 27 copie (8 con una copertina reale, vedi `seed-covers/`), 7 prestiti in quattro stati su cinque (manca l'annullato, che si ottiene ritirando una richiesta), chat, recensioni sui 3 livelli, segnalazioni, sottoscrizioni, proposte e 45 giorni di visualizzazioni per i grafici della dashboard. |

Entrambi gli script sono **idempotenti** e possono essere rieseguiti sullo
stesso database senza duplicare nulla.

## Esecuzione

Con `psql` nel PATH:

```bash
psql -h localhost -U postgres -d aportatadilibro -v ON_ERROR_STOP=1 -f database/01-schema.sql
```

```bash
psql -h localhost -U postgres -d aportatadilibro -v ON_ERROR_STOP=1 -f database/02-seed.sql
```

Con PostgreSQL in container Docker (sostituire `postgres-db` con il nome del
container):

```bash
docker exec -i postgres-db psql -U postgres -d aportatadilibro -v ON_ERROR_STOP=1 < database/01-schema.sql
```

Il database deve esistere prima di eseguire `01-schema.sql`:

```bash
docker exec postgres-db psql -U postgres -c "CREATE DATABASE aportatadilibro;"
```

Per vedere anche le copertine di esempio (non solo il segnaposto), copiare i
file di `seed-covers/` sotto `backend/APortataDiLibro.API/wwwroot/covers/`
prima di avviare l'API: vedi [`seed-covers/README.md`](seed-covers/README.md).

## Rapporto con le migrazioni EF Core

`01-schema.sql` non è scritto a mano: è generato dalle migrazioni EF Core in
`backend/APortataDiLibro.Database/Migrations`, che restano la sorgente di
verità dello schema. Dopo aver aggiunto una migrazione va rigenerato:

```bash
dotnet ef migrations script --idempotent --project backend/APortataDiLibro.Database --startup-project backend/APortataDiLibro.API --output database/01-schema.sql
```

L'opzione `--idempotent` racchiude ogni istruzione in un controllo su
`__EFMigrationsHistory`: lo script applica solo le migrazioni mancanti, quindi
funziona sia su un database vuoto sia su uno già parzialmente aggiornato.
In alternativa allo script, in sviluppo si può usare direttamente
`dotnet ef database update`.

## Utenti di prova

Creati da `02-seed.sql`. Tutti gli hash sono PBKDF2-HMAC-SHA256 a 600.000
iterazioni nel formato prodotto da `Pbkdf2PasswordHasher`.

| Username | Password | Ruolo |
|---|---|---|
| `admin` | `Admin1234!` | Admin |
| `giulia.rossi` | `Demo1234!` | Utente |
| `marco.bianchi` | `Demo1234!` | Utente |
| `sara.conti` | `Demo1234!` | Utente |
| `luca.ferrari` | `Demo1234!` | Utente |
| `elena.greco` | `Demo1234!` | Utente |

Sono credenziali pubbliche pensate per un ambiente locale di test: non vanno
usate su un'istanza raggiungibile dalla rete.

## Primo Admin senza dati di test

La registrazione pubblica assegna sempre il ruolo `Utente` e non esiste un
endpoint di promozione: su un database creato dalle sole migrazioni non ci
sarebbe alcun Admin, e la dashboard `/admin` risulterebbe irraggiungibile.

Per questo l'API, se la sezione `AdminBootstrap` è abilitata, crea l'account
amministrativo all'avvio quando nel database non esiste ancora nessun Admin.
La sezione è attiva in `appsettings.Development.json` e disattivata in
`appsettings.json`:

```json
"AdminBootstrap": {
  "Abilitato": true,
  "Username": "admin",
  "Nickname": "Curatore",
  "Nome": "Anna",
  "Cognome": "Moretti",
  "Email": "admin@aportatadilibro.local",
  "Password": "Admin1234!"
}
```

La password viene sottoposta a hash immediatamente e mai persistita in chiaro.
Fuori dallo sviluppo locale va fornita tramite user-secrets o variabili
d'ambiente (`AdminBootstrap__Password`), non tramite `appsettings.json`.

## Azzeramento

Per ripartire da zero, il modo più pulito è ricreare il database:

```bash
docker exec postgres-db psql -U postgres -c "DROP DATABASE IF EXISTS aportatadilibro;" -c "CREATE DATABASE aportatadilibro;"
```

e rieseguire i due script.
