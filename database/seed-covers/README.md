# Copertine di esempio per il seed

16 file JPG (8 copertine a piena risoluzione + le rispettive thumbnail)
associati alle copie inserite da `02-seed.sql`. Sono immagini generate
localmente (sfondo colorato, titolo/autore scritti a mano) e non copertine
editoriali reali: servono solo a mostrare come appare la UI quando una copia
ha una copertina caricata, senza scaricare o redistribuire materiale coperto
da copyright.

Il nome di ciascun file e' l'`Id` della copia (`CopieLibro.Id`) in formato
`Guid.ToString("N")`, esattamente come lo produce `MagickCoverImageService`
quando un utente carica davvero un'immagine. Le righe `PercorsoCopertina` /
`PercorsoCopertinaThumbnail` in `02-seed.sql` puntano gia' a questi nomi.

## Perche' non stanno in `wwwroot/covers`

`backend/APortataDiLibro.API/wwwroot/covers/` e' nel `.gitignore`: e' lo
storage per le copertine caricate a runtime dagli utenti, non va versionato.
Questi file restano quindi qui, in una cartella tracciata da git.

## Installazione

**Con `docker compose` non serve fare nulla:** all'avvio dell'API il
`docker-compose.yml` copia questi file nel volume delle copertine, saltando
quelli gia' presenti, quindi sono disponibili al primo avvio. La cartella non
e' montata direttamente perche' l'API in quel percorso deve poter scrivere le
copertine caricate dagli utenti.

I passaggi che seguono servono solo a chi avvia l'applicazione senza Docker,
e vanno eseguiti una volta sola dopo aver clonato il repository.

Con PowerShell, dalla root del repository:

```powershell
New-Item -ItemType Directory -Force backend/APortataDiLibro.API/wwwroot/covers
Copy-Item database/seed-covers/*.jpg backend/APortataDiLibro.API/wwwroot/covers/
```

Con bash:

```bash
mkdir -p backend/APortataDiLibro.API/wwwroot/covers
cp database/seed-covers/*.jpg backend/APortataDiLibro.API/wwwroot/covers/
```

Senza questo passaggio l'applicazione funziona comunque: le copie con
`PercorsoCopertina` valorizzato ma senza il file corrispondente cadono sul
segnaposto in UI, come qualunque copertina mancante.
