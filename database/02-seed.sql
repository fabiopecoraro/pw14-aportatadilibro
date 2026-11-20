-- =====================================================================
-- "A portata di libro" - dati di popolamento per ambiente di test
-- ---------------------------------------------------------------------
-- Prerequisito: 01-schema.sql gia' eseguito sullo stesso database.
--
-- Ambito scelto: citta' di Salerno, centro e lungomare. Gli Hub
-- hanno coordinate reali cosi' che la ricerca per bounding box e la
-- mappa Leaflet mostrino risultati plausibili senza doverli inventare
-- a mano durante una demo.
--
-- Lo script e' idempotente (ON CONFLICT DO NOTHING): puo' essere
-- rieseguito senza duplicare nulla. Gli identificativi sono fissi e
-- leggibili proprio per renderlo ripetibile e per poter referenziare
-- una riga specifica in fase di debug.
--
-- Le date sono relative a now(): il dataset resta "fresco" a ogni
-- esecuzione, cosi' le metriche della dashboard Admin (durata media dei
-- prestiti, prestiti attivi) restano significative nel tempo.
--
-- Credenziali degli utenti di prova:
--   admin         / Admin1234!   -> ruolo Admin, accede a /admin
--   giulia.rossi  / Demo1234!
--   marco.bianchi / Demo1234!
--   sara.conti    / Demo1234!
--   luca.ferrari  / Demo1234!
--   elena.greco   / Demo1234!
-- Gli hash sono PBKDF2-HMAC-SHA256, 600.000 iterazioni (fattore di lavoro
-- raccomandato da OWASP), nel formato
-- {iterazioni}.{salt}.{hash} prodotto da Pbkdf2PasswordHasher.
-- =====================================================================

BEGIN;

-- ---------------------------------------------------------------------
-- Utenti
-- "Ruolo" e' mappato su intero (0 = Utente, 1 = Admin), a differenza
-- degli altri enum del modello che EF converte in stringa.
-- ---------------------------------------------------------------------
-- "DataAccettazioneTermini" coincide con la registrazione: il consenso e' un
-- presupposto della creazione dell'account, non un passo successivo.
-- L'admin usa "ON CONFLICT DO NOTHING" senza colonna esplicita (anziche' solo
-- su "Id" come le altre tabelle) perche' su un database dove l'API e' gia'
-- stata avviata in Development con AdminBootstrap abilitato esiste gia' un
-- utente con questa Email ma un Id generato casualmente: un conflitto solo su
-- "Id" non lo intercetterebbe, l'INSERT fallirebbe sull'indice univoco
-- IX_Utenti_Email e manderebbe in abort l'intera transazione, con tutte le
-- istruzioni successive dello script che falliscono a cascata.
INSERT INTO "Utenti" ("Id", "Username", "Nickname", "Nome", "Cognome", "Email", "PasswordHash", "Ruolo", "DataRegistrazione", "DataAccettazioneTermini") VALUES
 ('a0000000-0000-4000-8000-000000000001', 'admin',         'Curatore',    'Anna',   'Moretti', 'admin@aportatadilibro.local', '600000.3Vhr1Qffi8Lrexi9Ja9kBQ==./VqUT6qpYajMbU+y/gvSGsa8VlNuZQfHpHq+itQPMuo=', 1, now() - interval '180 days', now() - interval '180 days'),
 ('a0000000-0000-4000-8000-000000000002', 'giulia.rossi',  'GiuliaLegge', 'Giulia', 'Rossi',   'giulia.rossi@example.com',   '600000.hYp8cExM1vUHB1dxPf5dRQ==.yA5cwN7Py0WswvfIh8miep1zcr+ApF650NsjhfIj0uY=', 0, now() - interval '150 days', now() - interval '150 days'),
 ('a0000000-0000-4000-8000-000000000003', 'marco.bianchi', 'MarcoB',      'Marco',  'Bianchi', 'marco.bianchi@example.com',  '600000.hYp8cExM1vUHB1dxPf5dRQ==.yA5cwN7Py0WswvfIh8miep1zcr+ApF650NsjhfIj0uY=', 0, now() - interval '140 days', now() - interval '140 days'),
 ('a0000000-0000-4000-8000-000000000004', 'sara.conti',    'SaraC',       'Sara',   'Conti',   'sara.conti@example.com',     '600000.hYp8cExM1vUHB1dxPf5dRQ==.yA5cwN7Py0WswvfIh8miep1zcr+ApF650NsjhfIj0uY=', 0, now() - interval '120 days', now() - interval '120 days'),
 ('a0000000-0000-4000-8000-000000000005', 'luca.ferrari',  'LucaF',       'Luca',   'Ferrari', 'luca.ferrari@example.com',   '600000.hYp8cExM1vUHB1dxPf5dRQ==.yA5cwN7Py0WswvfIh8miep1zcr+ApF650NsjhfIj0uY=', 0, now() - interval '90 days',  now() - interval '90 days'),
 ('a0000000-0000-4000-8000-000000000006', 'elena.greco',   'ElenaG',      'Elena',  'Greco',   'elena.greco@example.com',    '600000.hYp8cExM1vUHB1dxPf5dRQ==.yA5cwN7Py0WswvfIh8miep1zcr+ApF650NsjhfIj0uY=', 0, now() - interval '60 days',  now() - interval '60 days')
ON CONFLICT DO NOTHING;

-- ---------------------------------------------------------------------
-- Hub: punti di scambio pubblici registrati dagli utenti.
--
-- Coordinate WGS84 reali, ricavate da Nominatim (OpenStreetMap), la stessa
-- fonte cartografica usata da Leaflet nel frontend. Stanno tutte entro un
-- paio di chilometri l'una dall'altra: e' la distanza a piedi che rende
-- praticabile il modello degli Hub, e quindi la ragione per cui l'ambito
-- dichiarato e' una citta' e non una provincia.
--
-- L'indice composito (Lat, Lng) serve la ricerca per bounding box
-- implementata in RicercaService.
-- ---------------------------------------------------------------------
INSERT INTO "Hub" ("Id", "UtenteId", "Nome", "Indirizzo", "Lat", "Lng") VALUES
 ('d0000000-0000-4000-8000-000000000001', 'a0000000-0000-4000-8000-000000000002', 'Piazza della Liberta''',        'Piazza della Liberta'', Salerno',        40.67629, 14.75375),
 ('d0000000-0000-4000-8000-000000000002', 'a0000000-0000-4000-8000-000000000002', 'Villa Comunale',                'Villa Comunale, Salerno',                40.67856, 14.75353),
 ('d0000000-0000-4000-8000-000000000003', 'a0000000-0000-4000-8000-000000000003', 'Piazza Portanova',              'Piazza Sedile di Portanova, Salerno',    40.67794, 14.76239),
 ('d0000000-0000-4000-8000-000000000004', 'a0000000-0000-4000-8000-000000000004', 'Zerottonove',                   'Lungomare Trieste, Salerno',             40.67841, 14.75726),
 ('d0000000-0000-4000-8000-000000000005', 'a0000000-0000-4000-8000-000000000005', 'Stadio Donato Vestuti',         'Via Paolo Emilio Bilotti, Salerno',      40.68054, 14.77167),
 ('d0000000-0000-4000-8000-000000000006', 'a0000000-0000-4000-8000-000000000006', 'Piazza San Francesco',          'Piazza San Francesco d''Assisi, Salerno', 40.68060, 14.76751)
ON CONFLICT ("Id") DO NOTHING;

-- ---------------------------------------------------------------------
-- Vocabolario controllato delle categorie.
-- "Filosofia" resta volutamente senza opere collegate: serve a provare in
-- demo il percorso di eliminazione di una voce, che l'applicazione consente
-- solo quando nessuna opera e nessuna proposta la usano.
-- ---------------------------------------------------------------------
INSERT INTO "Categorie" ("Id", "Nome") VALUES
 ('70000000-0000-4000-8000-000000000001', 'Narrativa'),
 ('70000000-0000-4000-8000-000000000002', 'Classici'),
 ('70000000-0000-4000-8000-000000000003', 'Romanzo storico'),
 ('70000000-0000-4000-8000-000000000004', 'Giallo'),
 ('70000000-0000-4000-8000-000000000005', 'Distopia'),
 ('70000000-0000-4000-8000-000000000006', 'Memorialistica'),
 ('70000000-0000-4000-8000-000000000007', 'Diari'),
 ('70000000-0000-4000-8000-000000000008', 'Avventura'),
 ('70000000-0000-4000-8000-000000000009', 'Filosofia')
ON CONFLICT ("Id") DO NOTHING;

-- ---------------------------------------------------------------------
-- Catalogo condiviso. "Anno" e' facoltativo per modello: il libro 10 lo
-- lascia volutamente NULL per esercitare anche il caso "anno non noto".
-- ---------------------------------------------------------------------
INSERT INTO "Libri" ("Id", "Titolo", "Autore", "Anno", "Isbn") VALUES
 ('b0000000-0000-4000-8000-000000000001', 'Il nome della rosa',             'Umberto Eco',            1980, '9788845292613'),
 ('b0000000-0000-4000-8000-000000000002', 'Le citta'' invisibili',          'Italo Calvino',          1972, '9788804668237'),
 ('b0000000-0000-4000-8000-000000000003', 'La coscienza di Zeno',           'Italo Svevo',            1923, '9788807900174'),
 ('b0000000-0000-4000-8000-000000000004', 'Se questo e'' un uomo',          'Primo Levi',             1947, '9788806219352'),
 ('b0000000-0000-4000-8000-000000000005', 'Il giorno della civetta',        'Leonardo Sciascia',      1961, '9788845290091'),
 ('b0000000-0000-4000-8000-000000000006', 'L''amica geniale',               'Elena Ferrante',         2011, '9788866320328'),
 ('b0000000-0000-4000-8000-000000000007', 'Cent''anni di solitudine',       'Gabriel Garcia Marquez', 1967, '9788804707974'),
 ('b0000000-0000-4000-8000-000000000008', '1984',                           'George Orwell',          1949, '9788804668381'),
 ('b0000000-0000-4000-8000-000000000009', 'Il barone rampante',             'Italo Calvino',          1957, '9788804682479'),
 ('b0000000-0000-4000-8000-000000000010', 'La solitudine dei numeri primi', 'Paolo Giordano',         NULL, '9788831700320'),
 ('b0000000-0000-4000-8000-000000000011', 'Il deserto dei Tartari',         'Dino Buzzati',           1940, '9788804683049')
ON CONFLICT ("Id") DO NOTHING;

-- Seconda ondata di opere: allarga il catalogo oltre l'essenziale necessario a
-- esercitare gli stati applicativi, cosi' ricerca e filtri per categoria hanno
-- abbastanza materiale da restituire risultati non banali anche in demo.
INSERT INTO "Libri" ("Id", "Titolo", "Autore", "Anno", "Isbn") VALUES
 ('b0000000-0000-4000-8000-000000000012', 'Se una notte d''inverno un viaggiatore', 'Italo Calvino',              1979, '9788804668213'),
 ('b0000000-0000-4000-8000-000000000013', 'Il visconte dimezzato',                 'Italo Calvino',              1952, '9788804682257'),
 ('b0000000-0000-4000-8000-000000000014', 'La luna e i falo''',                    'Cesare Pavese',              1950, '9788806219970'),
 ('b0000000-0000-4000-8000-000000000015', 'Uno, nessuno e centomila',              'Luigi Pirandello',           1926, '9788817001100'),
 ('b0000000-0000-4000-8000-000000000016', 'L''isola di Arturo',                    'Elsa Morante',               1957, '9788806167479'),
 ('b0000000-0000-4000-8000-000000000017', 'Sostiene Pereira',                      'Antonio Tabucchi',           1994, '9788845247637'),
 ('b0000000-0000-4000-8000-000000000018', 'La tregua',                             'Primo Levi',                 1963, '9788806204600'),
 ('b0000000-0000-4000-8000-000000000019', 'Diario',                                'Anne Frank',                 1947, '9788804667629'),
 ('b0000000-0000-4000-8000-000000000020', 'Gomorra',                               'Roberto Saviano',            2006, '9788804566878'),
 ('b0000000-0000-4000-8000-000000000021', 'Novecento',                             'Alessandro Baricco',         1994, '9788807815393'),
 ('b0000000-0000-4000-8000-000000000022', 'Il piccolo principe',                   'Antoine de Saint-Exupery',   1943, '9788845929762'),
 ('b0000000-0000-4000-8000-000000000023', 'Fahrenheit 451',                        'Ray Bradbury',               1953, '9788804668398'),
 ('b0000000-0000-4000-8000-000000000024', 'Il mondo nuovo',                        'Aldous Huxley',              1932, '9788845929151'),
 ('b0000000-0000-4000-8000-000000000025', 'Delitto e castigo',                     'Fedor Dostoevskij',          1866, '9788817001322'),
 ('b0000000-0000-4000-8000-000000000026', 'Orgoglio e pregiudizio',                'Jane Austen',                1813, '9788807900297')
ON CONFLICT ("Id") DO NOTHING;

-- Classificazione: la maggior parte delle opere ha piu' di una categoria, che
-- e' il motivo per cui il campo testuale singolo e' stato sostituito.
INSERT INTO "LibriCategorie" ("LibroId", "CategoriaId") VALUES
 ('b0000000-0000-4000-8000-000000000001', '70000000-0000-4000-8000-000000000003'),
 ('b0000000-0000-4000-8000-000000000001', '70000000-0000-4000-8000-000000000004'),
 ('b0000000-0000-4000-8000-000000000002', '70000000-0000-4000-8000-000000000001'),
 ('b0000000-0000-4000-8000-000000000002', '70000000-0000-4000-8000-000000000002'),
 ('b0000000-0000-4000-8000-000000000003', '70000000-0000-4000-8000-000000000002'),
 ('b0000000-0000-4000-8000-000000000003', '70000000-0000-4000-8000-000000000001'),
 ('b0000000-0000-4000-8000-000000000004', '70000000-0000-4000-8000-000000000006'),
 ('b0000000-0000-4000-8000-000000000004', '70000000-0000-4000-8000-000000000002'),
 ('b0000000-0000-4000-8000-000000000005', '70000000-0000-4000-8000-000000000004'),
 ('b0000000-0000-4000-8000-000000000005', '70000000-0000-4000-8000-000000000001'),
 ('b0000000-0000-4000-8000-000000000006', '70000000-0000-4000-8000-000000000001'),
 ('b0000000-0000-4000-8000-000000000007', '70000000-0000-4000-8000-000000000001'),
 ('b0000000-0000-4000-8000-000000000007', '70000000-0000-4000-8000-000000000002'),
 ('b0000000-0000-4000-8000-000000000008', '70000000-0000-4000-8000-000000000005'),
 ('b0000000-0000-4000-8000-000000000008', '70000000-0000-4000-8000-000000000002'),
 ('b0000000-0000-4000-8000-000000000009', '70000000-0000-4000-8000-000000000001'),
 ('b0000000-0000-4000-8000-000000000009', '70000000-0000-4000-8000-000000000008'),
 ('b0000000-0000-4000-8000-000000000010', '70000000-0000-4000-8000-000000000001'),
 ('b0000000-0000-4000-8000-000000000011', '70000000-0000-4000-8000-000000000001'),
 ('b0000000-0000-4000-8000-000000000011', '70000000-0000-4000-8000-000000000002'),
 ('b0000000-0000-4000-8000-000000000012', '70000000-0000-4000-8000-000000000001'),
 ('b0000000-0000-4000-8000-000000000012', '70000000-0000-4000-8000-000000000002'),
 ('b0000000-0000-4000-8000-000000000013', '70000000-0000-4000-8000-000000000001'),
 ('b0000000-0000-4000-8000-000000000013', '70000000-0000-4000-8000-000000000008'),
 ('b0000000-0000-4000-8000-000000000014', '70000000-0000-4000-8000-000000000001'),
 ('b0000000-0000-4000-8000-000000000014', '70000000-0000-4000-8000-000000000002'),
 ('b0000000-0000-4000-8000-000000000015', '70000000-0000-4000-8000-000000000001'),
 ('b0000000-0000-4000-8000-000000000015', '70000000-0000-4000-8000-000000000002'),
 ('b0000000-0000-4000-8000-000000000016', '70000000-0000-4000-8000-000000000001'),
 ('b0000000-0000-4000-8000-000000000016', '70000000-0000-4000-8000-000000000002'),
 ('b0000000-0000-4000-8000-000000000017', '70000000-0000-4000-8000-000000000003'),
 ('b0000000-0000-4000-8000-000000000017', '70000000-0000-4000-8000-000000000001'),
 ('b0000000-0000-4000-8000-000000000018', '70000000-0000-4000-8000-000000000006'),
 ('b0000000-0000-4000-8000-000000000019', '70000000-0000-4000-8000-000000000007'),
 ('b0000000-0000-4000-8000-000000000019', '70000000-0000-4000-8000-000000000006'),
 ('b0000000-0000-4000-8000-000000000020', '70000000-0000-4000-8000-000000000006'),
 ('b0000000-0000-4000-8000-000000000021', '70000000-0000-4000-8000-000000000001'),
 ('b0000000-0000-4000-8000-000000000022', '70000000-0000-4000-8000-000000000001'),
 ('b0000000-0000-4000-8000-000000000022', '70000000-0000-4000-8000-000000000008'),
 ('b0000000-0000-4000-8000-000000000023', '70000000-0000-4000-8000-000000000005'),
 ('b0000000-0000-4000-8000-000000000024', '70000000-0000-4000-8000-000000000005'),
 ('b0000000-0000-4000-8000-000000000025', '70000000-0000-4000-8000-000000000002'),
 ('b0000000-0000-4000-8000-000000000025', '70000000-0000-4000-8000-000000000004'),
 ('b0000000-0000-4000-8000-000000000026', '70000000-0000-4000-8000-000000000002'),
 ('b0000000-0000-4000-8000-000000000026', '70000000-0000-4000-8000-000000000001')
ON CONFLICT ("LibroId", "CategoriaId") DO NOTHING;

-- ---------------------------------------------------------------------
-- Copie fisiche. Lo stato deve restare coerente con i prestiti inseriti
-- piu' sotto: 'InPrestito' solo per le copie con un prestito Accettato.
-- In questo primo blocco le copertine restano NULL: nessun file le
-- accompagna, e la UI cade sul segnaposto invece che su un 404. Le copie
-- con copertina sono nel blocco successivo.
-- ---------------------------------------------------------------------
INSERT INTO "CopieLibro" ("Id", "LibroId", "ProprietarioId", "PercorsoCopertina", "PercorsoCopertinaThumbnail", "Stato") VALUES
 ('c0000000-0000-4000-8000-000000000001', 'b0000000-0000-4000-8000-000000000001', 'a0000000-0000-4000-8000-000000000002', NULL, NULL, 'Disponibile'),
 ('c0000000-0000-4000-8000-000000000002', 'b0000000-0000-4000-8000-000000000002', 'a0000000-0000-4000-8000-000000000002', NULL, NULL, 'Disponibile'),
 ('c0000000-0000-4000-8000-000000000003', 'b0000000-0000-4000-8000-000000000003', 'a0000000-0000-4000-8000-000000000003', NULL, NULL, 'Disponibile'),
 ('c0000000-0000-4000-8000-000000000004', 'b0000000-0000-4000-8000-000000000004', 'a0000000-0000-4000-8000-000000000003', NULL, NULL, 'InPrestito'),
 ('c0000000-0000-4000-8000-000000000005', 'b0000000-0000-4000-8000-000000000005', 'a0000000-0000-4000-8000-000000000004', NULL, NULL, 'Disponibile'),
 ('c0000000-0000-4000-8000-000000000006', 'b0000000-0000-4000-8000-000000000006', 'a0000000-0000-4000-8000-000000000004', NULL, NULL, 'InPrestito'),
 ('c0000000-0000-4000-8000-000000000007', 'b0000000-0000-4000-8000-000000000007', 'a0000000-0000-4000-8000-000000000005', NULL, NULL, 'Disponibile'),
 ('c0000000-0000-4000-8000-000000000008', 'b0000000-0000-4000-8000-000000000001', 'a0000000-0000-4000-8000-000000000005', NULL, NULL, 'Disponibile'),
 ('c0000000-0000-4000-8000-000000000009', 'b0000000-0000-4000-8000-000000000008', 'a0000000-0000-4000-8000-000000000006', NULL, NULL, 'Disponibile'),
 ('c0000000-0000-4000-8000-000000000010', 'b0000000-0000-4000-8000-000000000009', 'a0000000-0000-4000-8000-000000000006', NULL, NULL, 'Disponibile'),
 ('c0000000-0000-4000-8000-000000000011', 'b0000000-0000-4000-8000-000000000010', 'a0000000-0000-4000-8000-000000000002', NULL, NULL, 'InPrestito'),
 ('c0000000-0000-4000-8000-000000000012', 'b0000000-0000-4000-8000-000000000011', 'a0000000-0000-4000-8000-000000000003', NULL, NULL, 'Disponibile')
ON CONFLICT ("Id") DO NOTHING;

-- Copie della seconda ondata di opere, una per libro, ruotate sui 5 utenti
-- non-Admin. Circa meta' ha una copertina reale sotto wwwroot/covers (vedi
-- database/seed-covers/README.md per come portarcele); le altre restano NULL
-- di proposito, per continuare a esercitare anche qui il segnaposto in UI.
INSERT INTO "CopieLibro" ("Id", "LibroId", "ProprietarioId", "PercorsoCopertina", "PercorsoCopertinaThumbnail", "Stato") VALUES
 ('c0000000-0000-4000-8000-000000000013', 'b0000000-0000-4000-8000-000000000012', 'a0000000-0000-4000-8000-000000000002', '/covers/c0000000000040008000000000000013.jpg', '/covers/c0000000000040008000000000000013_thumb.jpg', 'Disponibile'),
 ('c0000000-0000-4000-8000-000000000014', 'b0000000-0000-4000-8000-000000000013', 'a0000000-0000-4000-8000-000000000003', '/covers/c0000000000040008000000000000014.jpg', '/covers/c0000000000040008000000000000014_thumb.jpg', 'Disponibile'),
 ('c0000000-0000-4000-8000-000000000015', 'b0000000-0000-4000-8000-000000000014', 'a0000000-0000-4000-8000-000000000004', NULL, NULL, 'Disponibile'),
 ('c0000000-0000-4000-8000-000000000016', 'b0000000-0000-4000-8000-000000000015', 'a0000000-0000-4000-8000-000000000005', '/covers/c0000000000040008000000000000016.jpg', '/covers/c0000000000040008000000000000016_thumb.jpg', 'Disponibile'),
 ('c0000000-0000-4000-8000-000000000017', 'b0000000-0000-4000-8000-000000000016', 'a0000000-0000-4000-8000-000000000006', NULL, NULL, 'Disponibile'),
 ('c0000000-0000-4000-8000-000000000018', 'b0000000-0000-4000-8000-000000000017', 'a0000000-0000-4000-8000-000000000002', '/covers/c0000000000040008000000000000018.jpg', '/covers/c0000000000040008000000000000018_thumb.jpg', 'Disponibile'),
 ('c0000000-0000-4000-8000-000000000019', 'b0000000-0000-4000-8000-000000000018', 'a0000000-0000-4000-8000-000000000003', NULL, NULL, 'Disponibile'),
 ('c0000000-0000-4000-8000-000000000020', 'b0000000-0000-4000-8000-000000000019', 'a0000000-0000-4000-8000-000000000004', '/covers/c0000000000040008000000000000020.jpg', '/covers/c0000000000040008000000000000020_thumb.jpg', 'Disponibile'),
 ('c0000000-0000-4000-8000-000000000021', 'b0000000-0000-4000-8000-000000000020', 'a0000000-0000-4000-8000-000000000005', NULL, NULL, 'Disponibile'),
 ('c0000000-0000-4000-8000-000000000022', 'b0000000-0000-4000-8000-000000000021', 'a0000000-0000-4000-8000-000000000006', '/covers/c0000000000040008000000000000022.jpg', '/covers/c0000000000040008000000000000022_thumb.jpg', 'Disponibile'),
 ('c0000000-0000-4000-8000-000000000023', 'b0000000-0000-4000-8000-000000000022', 'a0000000-0000-4000-8000-000000000002', NULL, NULL, 'Disponibile'),
 ('c0000000-0000-4000-8000-000000000024', 'b0000000-0000-4000-8000-000000000023', 'a0000000-0000-4000-8000-000000000003', '/covers/c0000000000040008000000000000024.jpg', '/covers/c0000000000040008000000000000024_thumb.jpg', 'Disponibile'),
 ('c0000000-0000-4000-8000-000000000025', 'b0000000-0000-4000-8000-000000000024', 'a0000000-0000-4000-8000-000000000004', NULL, NULL, 'Disponibile'),
 ('c0000000-0000-4000-8000-000000000026', 'b0000000-0000-4000-8000-000000000025', 'a0000000-0000-4000-8000-000000000005', '/covers/c0000000000040008000000000000026.jpg', '/covers/c0000000000040008000000000000026_thumb.jpg', 'Disponibile'),
 ('c0000000-0000-4000-8000-000000000027', 'b0000000-0000-4000-8000-000000000026', 'a0000000-0000-4000-8000-000000000006', NULL, NULL, 'Disponibile')
ON CONFLICT ("Id") DO NOTHING;

-- ---------------------------------------------------------------------
-- Esclusione Copia/Hub: "Il nome della rosa" di Giulia si ritira solo in
-- Piazza della Liberta', non alla Villa Comunale. Serve a verificare che la
-- ricerca per area escluda davvero la coppia Hub/copia.
-- ---------------------------------------------------------------------
INSERT INTO "EsclusioniHubLibro" ("CopiaLibroId", "HubId") VALUES
 ('c0000000-0000-4000-8000-000000000001', 'd0000000-0000-4000-8000-000000000002')
ON CONFLICT ("CopiaLibroId", "HubId") DO NOTHING;

-- ---------------------------------------------------------------------
-- Prestiti: coprono quattro dei cinque stati del workflow, cosi' che le
-- schermate "Richieste inviate/ricevute" e le metriche Admin abbiano un
-- caso per ramo. Manca Annullato, che si ottiene dall'interfaccia ritirando
-- la richiesta in attesa. I due prestiti Completato danno una durata
-- media di 18 giorni nella dashboard.
-- ---------------------------------------------------------------------
INSERT INTO "Prestiti" ("Id", "CopiaLibroId", "RichiedenteId", "Stato", "DataRichiesta", "GiorniStimati", "DataRestituzioneEffettiva") VALUES
 ('e0000000-0000-4000-8000-000000000001', 'c0000000-0000-4000-8000-000000000001', 'a0000000-0000-4000-8000-000000000003', 'Completato', now() - interval '40 days', 14, now() - interval '26 days'),
 ('e0000000-0000-4000-8000-000000000002', 'c0000000-0000-4000-8000-000000000004', 'a0000000-0000-4000-8000-000000000002', 'Accettato',  now() - interval '10 days', 21, NULL),
 ('e0000000-0000-4000-8000-000000000003', 'c0000000-0000-4000-8000-000000000006', 'a0000000-0000-4000-8000-000000000005', 'Accettato',  now() - interval '5 days',  30, NULL),
 ('e0000000-0000-4000-8000-000000000004', 'c0000000-0000-4000-8000-000000000011', 'a0000000-0000-4000-8000-000000000006', 'Accettato',  now() - interval '3 days',  15, NULL),
 ('e0000000-0000-4000-8000-000000000005', 'c0000000-0000-4000-8000-000000000005', 'a0000000-0000-4000-8000-000000000003', 'Richiesto',  now() - interval '1 days',  10, NULL),
 ('e0000000-0000-4000-8000-000000000006', 'c0000000-0000-4000-8000-000000000007', 'a0000000-0000-4000-8000-000000000004', 'Completato', now() - interval '70 days', 20, now() - interval '48 days'),
 ('e0000000-0000-4000-8000-000000000007', 'c0000000-0000-4000-8000-000000000009', 'a0000000-0000-4000-8000-000000000002', 'Rifiutato',  now() - interval '15 days', 7,  NULL)
ON CONFLICT ("Id") DO NOTHING;

-- ---------------------------------------------------------------------
-- Cronologia chat. Persistita anche oltre SignalR perche' l'Admin deve
-- poter ispezionare le conversazioni oggetto di segnalazione.
-- ---------------------------------------------------------------------
INSERT INTO "MessaggiChat" ("Id", "PrestitoId", "MittenteId", "Testo", "DataInvio") VALUES
 ('f0000000-0000-4000-8000-000000000001', 'e0000000-0000-4000-8000-000000000001', 'a0000000-0000-4000-8000-000000000003', 'Ciao Giulia! Passo a Piazza della Liberta'' domani verso le 18, va bene?', now() - interval '39 days'),
 ('f0000000-0000-4000-8000-000000000002', 'e0000000-0000-4000-8000-000000000001', 'a0000000-0000-4000-8000-000000000002', 'Perfetto, ci sono. Il libro e'' in una busta gialla.',            now() - interval '39 days'),
 ('f0000000-0000-4000-8000-000000000003', 'e0000000-0000-4000-8000-000000000001', 'a0000000-0000-4000-8000-000000000003', 'Restituito, grazie mille! Letto tutto d''un fiato.',              now() - interval '26 days'),
 ('f0000000-0000-4000-8000-000000000004', 'e0000000-0000-4000-8000-000000000003', 'a0000000-0000-4000-8000-000000000005', 'Buongiorno Sara, quando ci vediamo per il ritiro?',               now() - interval '5 days'),
 ('f0000000-0000-4000-8000-000000000005', 'e0000000-0000-4000-8000-000000000003', 'a0000000-0000-4000-8000-000000000004', 'Vieni quando vuoi, ma sbrigati che non ho tempo da perdere.',     now() - interval '4 days'),
 ('f0000000-0000-4000-8000-000000000006', 'e0000000-0000-4000-8000-000000000003', 'a0000000-0000-4000-8000-000000000005', 'Va bene, ci sono giovedi'' alle 17.',                            now() - interval '4 days'),
 ('f0000000-0000-4000-8000-000000000007', 'e0000000-0000-4000-8000-000000000003', 'a0000000-0000-4000-8000-000000000004', 'Se non arrivi puntuale lascio perdere, chiaro?',                  now() - interval '3 days'),
 ('f0000000-0000-4000-8000-000000000008', 'e0000000-0000-4000-8000-000000000002', 'a0000000-0000-4000-8000-000000000002', 'Ciao Marco, grazie! Dove preferisci per il ritiro?',              now() - interval '10 days'),
 ('f0000000-0000-4000-8000-000000000009', 'e0000000-0000-4000-8000-000000000002', 'a0000000-0000-4000-8000-000000000003', 'A Piazza Portanova, e'' quello che mi viene piu'' comodo.',        now() - interval '10 days'),
 ('f0000000-0000-4000-8000-00000000000a', 'e0000000-0000-4000-8000-000000000002', 'a0000000-0000-4000-8000-000000000002', 'Perfetto, passo sabato mattina verso le 10.',                     now() - interval '9 days'),
 ('f0000000-0000-4000-8000-00000000000b', 'e0000000-0000-4000-8000-000000000002', 'a0000000-0000-4000-8000-000000000003', 'Ottimo, a sabato!',                                              now() - interval '9 days')
ON CONFLICT ("Id") DO NOTHING;

-- ---------------------------------------------------------------------
-- Sistema di recensioni a 3 livelli.
-- Livello 2: l'opera. Alimenta la colonna MediaVotiRecensioni del CSV Admin.
-- ---------------------------------------------------------------------
INSERT INTO "RecensioniLibro" ("Id", "LibroId", "AutoreId", "Voto", "Commento", "DataCreazione") VALUES
 ('11111111-0000-4000-8000-000000000001', 'b0000000-0000-4000-8000-000000000001', 'a0000000-0000-4000-8000-000000000003', 5, 'Un labirinto perfetto, il finale vale tutte le pagine.', now() - interval '25 days'),
 ('11111111-0000-4000-8000-000000000002', 'b0000000-0000-4000-8000-000000000001', 'a0000000-0000-4000-8000-000000000004', 4, 'Denso ma bellissimo.',                                  now() - interval '20 days'),
 ('11111111-0000-4000-8000-000000000003', 'b0000000-0000-4000-8000-000000000001', 'a0000000-0000-4000-8000-000000000006', 5, NULL,                                                   now() - interval '12 days'),
 ('11111111-0000-4000-8000-000000000004', 'b0000000-0000-4000-8000-000000000002', 'a0000000-0000-4000-8000-000000000002', 5, 'Da rileggere una citta'' alla volta.',                  now() - interval '35 days'),
 ('11111111-0000-4000-8000-000000000005', 'b0000000-0000-4000-8000-000000000002', 'a0000000-0000-4000-8000-000000000005', 4, NULL,                                                   now() - interval '18 days'),
 ('11111111-0000-4000-8000-000000000006', 'b0000000-0000-4000-8000-000000000004', 'a0000000-0000-4000-8000-000000000002', 5, 'Necessario.',                                           now() - interval '8 days'),
 ('11111111-0000-4000-8000-000000000007', 'b0000000-0000-4000-8000-000000000006', 'a0000000-0000-4000-8000-000000000005', 3, 'Parte lentamente, poi ingrana.',                        now() - interval '2 days'),
 ('11111111-0000-4000-8000-000000000008', 'b0000000-0000-4000-8000-000000000007', 'a0000000-0000-4000-8000-000000000004', 4, NULL,                                                   now() - interval '45 days'),
 ('11111111-0000-4000-8000-000000000009', 'b0000000-0000-4000-8000-000000000008', 'a0000000-0000-4000-8000-000000000002', 4, 'Invecchiato benissimo, purtroppo.',                     now() - interval '14 days')
ON CONFLICT ("Id") DO NOTHING;

-- Livello 1: affidabilita' della controparte. Vincolo unico (Prestito, Autore).
INSERT INTO "RecensioniUtente" ("Id", "PrestitoId", "AutoreId", "UtenteRecensitoId", "Voto", "Commento", "DataCreazione") VALUES
 ('22222222-0000-4000-8000-000000000001', 'e0000000-0000-4000-8000-000000000001', 'a0000000-0000-4000-8000-000000000003', 'a0000000-0000-4000-8000-000000000002', 5, 'Puntualissima e gentile.',         now() - interval '25 days'),
 ('22222222-0000-4000-8000-000000000002', 'e0000000-0000-4000-8000-000000000001', 'a0000000-0000-4000-8000-000000000002', 'a0000000-0000-4000-8000-000000000003', 5, 'Ha restituito prima del previsto.', now() - interval '25 days'),
 ('22222222-0000-4000-8000-000000000003', 'e0000000-0000-4000-8000-000000000006', 'a0000000-0000-4000-8000-000000000004', 'a0000000-0000-4000-8000-000000000005', 4, 'Tutto liscio.',                    now() - interval '47 days'),
 ('22222222-0000-4000-8000-000000000004', 'e0000000-0000-4000-8000-000000000006', 'a0000000-0000-4000-8000-000000000005', 'a0000000-0000-4000-8000-000000000004', 5, NULL,                               now() - interval '47 days')
ON CONFLICT ("Id") DO NOTHING;

-- Livello 3: esperienza di scambio sulla singola copia.
INSERT INTO "RecensioniCopia" ("Id", "PrestitoId", "AutoreId", "Voto", "Commento", "DataCreazione") VALUES
 ('33333333-0000-4000-8000-000000000001', 'e0000000-0000-4000-8000-000000000001', 'a0000000-0000-4000-8000-000000000003', 5, 'Copia in ottimo stato, scambio rapido.',  now() - interval '25 days'),
 ('33333333-0000-4000-8000-000000000002', 'e0000000-0000-4000-8000-000000000006', 'a0000000-0000-4000-8000-000000000004', 4, 'Qualche pagina segnata, nulla di grave.', now() - interval '47 days')
ON CONFLICT ("Id") DO NOTHING;

-- ---------------------------------------------------------------------
-- Segnalazioni: una aperta (la chat del prestito e...03, quella scortese)
-- e una gia' chiusa, cosi' entrambi i filtri della dashboard hanno righe.
-- ---------------------------------------------------------------------
INSERT INTO "Report" ("Id", "PrestitoId", "SegnalanteId", "UtenteSegnalatoId", "Motivo", "Stato", "DataCreazione") VALUES
 ('44444444-0000-4000-8000-000000000001', 'e0000000-0000-4000-8000-000000000003', 'a0000000-0000-4000-8000-000000000005', 'a0000000-0000-4000-8000-000000000004', 'Toni aggressivi e pressioni sull''orario di consegna.', 'Aperto', now() - interval '3 days'),
 ('44444444-0000-4000-8000-000000000002', 'e0000000-0000-4000-8000-000000000001', 'a0000000-0000-4000-8000-000000000002', 'a0000000-0000-4000-8000-000000000003', 'Segnalazione rientrata dopo chiarimento fra le parti.', 'Chiuso', now() - interval '30 days')
ON CONFLICT ("Id") DO NOTHING;

-- ---------------------------------------------------------------------
-- Sottoscrizioni di disponibilita'. La lista dei desideri ha due sezioni,
-- "disponibili ora" e "in attesa", e lo stato non e' nella sottoscrizione ma
-- nella disponibilita' corrente del libro: servono quindi sia sottoscrizioni
-- su libri con tutte le copie in prestito sia su libri gia' disponibili,
-- altrimenti una delle due sezioni resterebbe sempre vuota.
-- ---------------------------------------------------------------------
INSERT INTO "SottoscrizioniDisponibilita" ("Id", "LibroId", "UtenteId", "DataRichiesta", "Notificata") VALUES
 ('55555555-0000-4000-8000-000000000001', 'b0000000-0000-4000-8000-000000000010', 'a0000000-0000-4000-8000-000000000004', now() - interval '2 days', false),
 ('55555555-0000-4000-8000-000000000002', 'b0000000-0000-4000-8000-000000000006', 'a0000000-0000-4000-8000-000000000003', now() - interval '4 days', false),
 ('55555555-0000-4000-8000-000000000003', 'b0000000-0000-4000-8000-000000000006', 'a0000000-0000-4000-8000-000000000002', now() - interval '6 days', false),
 ('55555555-0000-4000-8000-000000000004', 'b0000000-0000-4000-8000-000000000004', 'a0000000-0000-4000-8000-000000000002', now() - interval '3 days', false),
 -- Libro gia' tornato disponibile: popola la sezione "disponibili ora".
 -- Notificata resta false perche' a true la sottoscrizione risulta consumata
 -- e il servizio non la elenca piu'.
 ('55555555-0000-4000-8000-000000000005', 'b0000000-0000-4000-8000-000000000009', 'a0000000-0000-4000-8000-000000000002', now() - interval '8 days', false)
ON CONFLICT ("Id") DO NOTHING;

-- ---------------------------------------------------------------------
-- Proposte di nuovi libri: una per ciascuno stato del workflow di
-- approvazione. Quella approvata e' collegata al libro 11 e alla copia
-- c...12, esattamente come farebbe ApprovaPropostaAsync.
-- ---------------------------------------------------------------------
INSERT INTO "ProposteLibro" ("Id", "Titolo", "Autore", "Anno", "Isbn", "ProponenteId", "Stato", "DataProposta", "DataGestione", "LibroCreatoId") VALUES
 ('66666666-0000-4000-8000-000000000001', 'Il mestiere di vivere',      'Cesare Pavese', 1952, '9788806219994', 'a0000000-0000-4000-8000-000000000006', 'InAttesa',  now() - interval '2 days',  NULL,                       NULL),
 ('66666666-0000-4000-8000-000000000002', 'Il deserto dei Tartari',     'Dino Buzzati',  1940, '9788804683049', 'a0000000-0000-4000-8000-000000000003', 'Approvata', now() - interval '30 days', now() - interval '29 days', 'b0000000-0000-4000-8000-000000000011'),
 ('66666666-0000-4000-8000-000000000003', 'Appunti di viaggio (bozza)', 'Marco Bianchi', NULL, NULL,            'a0000000-0000-4000-8000-000000000003', 'Rifiutata', now() - interval '25 days', now() - interval '24 days', NULL)
ON CONFLICT ("Id") DO NOTHING;

-- Anche le proposte scelgono dal vocabolario: all'approvazione le
-- associazioni vengono ricopiate sull'opera creata senza interpretare nulla.
INSERT INTO "ProposteLibroCategorie" ("PropostaLibroId", "CategoriaId") VALUES
 ('66666666-0000-4000-8000-000000000001', '70000000-0000-4000-8000-000000000007'),
 ('66666666-0000-4000-8000-000000000001', '70000000-0000-4000-8000-000000000006'),
 ('66666666-0000-4000-8000-000000000002', '70000000-0000-4000-8000-000000000001'),
 ('66666666-0000-4000-8000-000000000002', '70000000-0000-4000-8000-000000000002'),
 ('66666666-0000-4000-8000-000000000003', '70000000-0000-4000-8000-000000000001')
ON CONFLICT ("PropostaLibroId", "CategoriaId") DO NOTHING;

-- ---------------------------------------------------------------------
-- Visualizzazioni delle schede libro: la metrica d'uso della dashboard.
-- Generate al volo su 45 giorni invece che elencate riga per riga (sarebbero
-- oltre trecento INSERT): il peso per libro determina la popolarita' relativa
-- e i due termini modulo danno una variabilita' giornaliera plausibile,
-- restando deterministici — lo stesso script produce sempre gli stessi
-- numeri. I giorni a zero non vengono inseriti: la serie li ricostruisce
-- comunque lato applicativo.
-- ---------------------------------------------------------------------
INSERT INTO "VisualizzazioniLibro" ("LibroId", "Giorno", "Conteggio")
SELECT "LibroId", "Giorno", "Conteggio"
FROM (
    SELECT
        libro.id AS "LibroId",
        (CURRENT_DATE - g.offset_giorni)::date AS "Giorno",
        GREATEST(0, libro.peso - (g.offset_giorni % 5) + ((g.offset_giorni * 3 + libro.peso) % 4)) AS "Conteggio"
    FROM (VALUES
        ('b0000000-0000-4000-8000-000000000001'::uuid, 9),
        ('b0000000-0000-4000-8000-000000000006'::uuid, 7),
        ('b0000000-0000-4000-8000-000000000002'::uuid, 6),
        ('b0000000-0000-4000-8000-000000000004'::uuid, 5),
        ('b0000000-0000-4000-8000-000000000008'::uuid, 4),
        ('b0000000-0000-4000-8000-000000000010'::uuid, 3),
        ('b0000000-0000-4000-8000-000000000003'::uuid, 2),
        ('b0000000-0000-4000-8000-000000000005'::uuid, 2)
    ) AS libro(id, peso)
    CROSS JOIN generate_series(0, 44) AS g(offset_giorni)
) AS generati
WHERE "Conteggio" > 0
ON CONFLICT ("LibroId", "Giorno") DO NOTHING;

COMMIT;
