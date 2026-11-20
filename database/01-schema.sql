CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260612075702_InitialCreate') THEN
    CREATE TABLE "Libri" (
        "Id" uuid NOT NULL,
        "Titolo" character varying(250) NOT NULL,
        "Autore" character varying(150) NOT NULL,
        "Genere" character varying(50) NOT NULL,
        "Isbn" character varying(13),
        CONSTRAINT "PK_Libri" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260612075702_InitialCreate') THEN
    CREATE TABLE "Utenti" (
        "Id" uuid NOT NULL,
        "Username" character varying(50) NOT NULL,
        "Nickname" character varying(50) NOT NULL,
        "Nome" character varying(100) NOT NULL,
        "Cognome" character varying(100) NOT NULL,
        "Email" character varying(256) NOT NULL,
        "PasswordHash" text NOT NULL,
        "Ruolo" integer NOT NULL,
        "DataRegistrazione" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_Utenti" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260612075702_InitialCreate') THEN
    CREATE TABLE "CopieLibro" (
        "Id" uuid NOT NULL,
        "LibroId" uuid NOT NULL,
        "ProprietarioId" uuid NOT NULL,
        "PercorsoCopertina" character varying(260),
        "PercorsoCopertinaThumbnail" character varying(260),
        "Stato" character varying(20) NOT NULL,
        CONSTRAINT "PK_CopieLibro" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_CopieLibro_Libri_LibroId" FOREIGN KEY ("LibroId") REFERENCES "Libri" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_CopieLibro_Utenti_ProprietarioId" FOREIGN KEY ("ProprietarioId") REFERENCES "Utenti" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260612075702_InitialCreate') THEN
    CREATE TABLE "Hub" (
        "Id" uuid NOT NULL,
        "UtenteId" uuid NOT NULL,
        "Nome" character varying(100) NOT NULL,
        "Indirizzo" character varying(250) NOT NULL,
        "Lat" double precision NOT NULL,
        "Lng" double precision NOT NULL,
        CONSTRAINT "PK_Hub" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Hub_Utenti_UtenteId" FOREIGN KEY ("UtenteId") REFERENCES "Utenti" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260612075702_InitialCreate') THEN
    CREATE TABLE "RecensioniLibro" (
        "Id" uuid NOT NULL,
        "LibroId" uuid NOT NULL,
        "AutoreId" uuid NOT NULL,
        "Voto" integer NOT NULL,
        "Commento" character varying(1000),
        "DataCreazione" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_RecensioniLibro" PRIMARY KEY ("Id"),
        CONSTRAINT "CK_RecensioniLibro_Voto" CHECK ("Voto" BETWEEN 1 AND 5),
        CONSTRAINT "FK_RecensioniLibro_Libri_LibroId" FOREIGN KEY ("LibroId") REFERENCES "Libri" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_RecensioniLibro_Utenti_AutoreId" FOREIGN KEY ("AutoreId") REFERENCES "Utenti" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260612075702_InitialCreate') THEN
    CREATE TABLE "SottoscrizioniDisponibilita" (
        "Id" uuid NOT NULL,
        "LibroId" uuid NOT NULL,
        "UtenteId" uuid NOT NULL,
        "DataRichiesta" timestamp with time zone NOT NULL,
        "Notificata" boolean NOT NULL,
        CONSTRAINT "PK_SottoscrizioniDisponibilita" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_SottoscrizioniDisponibilita_Libri_LibroId" FOREIGN KEY ("LibroId") REFERENCES "Libri" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_SottoscrizioniDisponibilita_Utenti_UtenteId" FOREIGN KEY ("UtenteId") REFERENCES "Utenti" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260612075702_InitialCreate') THEN
    CREATE TABLE "Prestiti" (
        "Id" uuid NOT NULL,
        "CopiaLibroId" uuid NOT NULL,
        "RichiedenteId" uuid NOT NULL,
        "Stato" character varying(20) NOT NULL,
        "DataRichiesta" timestamp with time zone NOT NULL,
        "GiorniStimati" integer NOT NULL,
        "DataRestituzioneEffettiva" timestamp with time zone,
        CONSTRAINT "PK_Prestiti" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Prestiti_CopieLibro_CopiaLibroId" FOREIGN KEY ("CopiaLibroId") REFERENCES "CopieLibro" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_Prestiti_Utenti_RichiedenteId" FOREIGN KEY ("RichiedenteId") REFERENCES "Utenti" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260612075702_InitialCreate') THEN
    CREATE TABLE "EsclusioniHubLibro" (
        "CopiaLibroId" uuid NOT NULL,
        "HubId" uuid NOT NULL,
        CONSTRAINT "PK_EsclusioniHubLibro" PRIMARY KEY ("CopiaLibroId", "HubId"),
        CONSTRAINT "FK_EsclusioniHubLibro_CopieLibro_CopiaLibroId" FOREIGN KEY ("CopiaLibroId") REFERENCES "CopieLibro" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_EsclusioniHubLibro_Hub_HubId" FOREIGN KEY ("HubId") REFERENCES "Hub" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260612075702_InitialCreate') THEN
    CREATE TABLE "MessaggiChat" (
        "Id" uuid NOT NULL,
        "PrestitoId" uuid NOT NULL,
        "MittenteId" uuid NOT NULL,
        "Testo" character varying(2000) NOT NULL,
        "DataInvio" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_MessaggiChat" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_MessaggiChat_Prestiti_PrestitoId" FOREIGN KEY ("PrestitoId") REFERENCES "Prestiti" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_MessaggiChat_Utenti_MittenteId" FOREIGN KEY ("MittenteId") REFERENCES "Utenti" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260612075702_InitialCreate') THEN
    CREATE TABLE "RecensioniCopia" (
        "Id" uuid NOT NULL,
        "PrestitoId" uuid NOT NULL,
        "AutoreId" uuid NOT NULL,
        "Voto" integer NOT NULL,
        "Commento" character varying(1000),
        "DataCreazione" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_RecensioniCopia" PRIMARY KEY ("Id"),
        CONSTRAINT "CK_RecensioniCopia_Voto" CHECK ("Voto" BETWEEN 1 AND 5),
        CONSTRAINT "FK_RecensioniCopia_Prestiti_PrestitoId" FOREIGN KEY ("PrestitoId") REFERENCES "Prestiti" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_RecensioniCopia_Utenti_AutoreId" FOREIGN KEY ("AutoreId") REFERENCES "Utenti" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260612075702_InitialCreate') THEN
    CREATE TABLE "RecensioniUtente" (
        "Id" uuid NOT NULL,
        "PrestitoId" uuid NOT NULL,
        "AutoreId" uuid NOT NULL,
        "UtenteRecensitoId" uuid NOT NULL,
        "Voto" integer NOT NULL,
        "Commento" character varying(1000),
        "DataCreazione" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_RecensioniUtente" PRIMARY KEY ("Id"),
        CONSTRAINT "CK_RecensioniUtente_Voto" CHECK ("Voto" BETWEEN 1 AND 5),
        CONSTRAINT "FK_RecensioniUtente_Prestiti_PrestitoId" FOREIGN KEY ("PrestitoId") REFERENCES "Prestiti" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_RecensioniUtente_Utenti_AutoreId" FOREIGN KEY ("AutoreId") REFERENCES "Utenti" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_RecensioniUtente_Utenti_UtenteRecensitoId" FOREIGN KEY ("UtenteRecensitoId") REFERENCES "Utenti" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260612075702_InitialCreate') THEN
    CREATE TABLE "Report" (
        "Id" uuid NOT NULL,
        "PrestitoId" uuid NOT NULL,
        "SegnalanteId" uuid NOT NULL,
        "UtenteSegnalatoId" uuid NOT NULL,
        "Motivo" character varying(500) NOT NULL,
        "Stato" character varying(20) NOT NULL,
        "DataCreazione" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_Report" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Report_Prestiti_PrestitoId" FOREIGN KEY ("PrestitoId") REFERENCES "Prestiti" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_Report_Utenti_SegnalanteId" FOREIGN KEY ("SegnalanteId") REFERENCES "Utenti" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_Report_Utenti_UtenteSegnalatoId" FOREIGN KEY ("UtenteSegnalatoId") REFERENCES "Utenti" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260612075702_InitialCreate') THEN
    CREATE INDEX "IX_CopieLibro_LibroId" ON "CopieLibro" ("LibroId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260612075702_InitialCreate') THEN
    CREATE INDEX "IX_CopieLibro_ProprietarioId" ON "CopieLibro" ("ProprietarioId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260612075702_InitialCreate') THEN
    CREATE INDEX "IX_EsclusioniHubLibro_HubId" ON "EsclusioniHubLibro" ("HubId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260612075702_InitialCreate') THEN
    CREATE INDEX "IX_Hub_Lat_Lng" ON "Hub" ("Lat", "Lng");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260612075702_InitialCreate') THEN
    CREATE INDEX "IX_Hub_UtenteId" ON "Hub" ("UtenteId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260612075702_InitialCreate') THEN
    CREATE INDEX "IX_MessaggiChat_MittenteId" ON "MessaggiChat" ("MittenteId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260612075702_InitialCreate') THEN
    CREATE INDEX "IX_MessaggiChat_PrestitoId_DataInvio" ON "MessaggiChat" ("PrestitoId", "DataInvio");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260612075702_InitialCreate') THEN
    CREATE INDEX "IX_Prestiti_CopiaLibroId" ON "Prestiti" ("CopiaLibroId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260612075702_InitialCreate') THEN
    CREATE INDEX "IX_Prestiti_RichiedenteId" ON "Prestiti" ("RichiedenteId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260612075702_InitialCreate') THEN
    CREATE INDEX "IX_RecensioniCopia_AutoreId" ON "RecensioniCopia" ("AutoreId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260612075702_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_RecensioniCopia_PrestitoId_AutoreId" ON "RecensioniCopia" ("PrestitoId", "AutoreId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260612075702_InitialCreate') THEN
    CREATE INDEX "IX_RecensioniLibro_AutoreId" ON "RecensioniLibro" ("AutoreId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260612075702_InitialCreate') THEN
    CREATE INDEX "IX_RecensioniLibro_LibroId" ON "RecensioniLibro" ("LibroId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260612075702_InitialCreate') THEN
    CREATE INDEX "IX_RecensioniUtente_AutoreId" ON "RecensioniUtente" ("AutoreId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260612075702_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_RecensioniUtente_PrestitoId_AutoreId" ON "RecensioniUtente" ("PrestitoId", "AutoreId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260612075702_InitialCreate') THEN
    CREATE INDEX "IX_RecensioniUtente_UtenteRecensitoId" ON "RecensioniUtente" ("UtenteRecensitoId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260612075702_InitialCreate') THEN
    CREATE INDEX "IX_Report_PrestitoId" ON "Report" ("PrestitoId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260612075702_InitialCreate') THEN
    CREATE INDEX "IX_Report_SegnalanteId" ON "Report" ("SegnalanteId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260612075702_InitialCreate') THEN
    CREATE INDEX "IX_Report_UtenteSegnalatoId" ON "Report" ("UtenteSegnalatoId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260612075702_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_SottoscrizioniDisponibilita_LibroId_UtenteId" ON "SottoscrizioniDisponibilita" ("LibroId", "UtenteId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260612075702_InitialCreate') THEN
    CREATE INDEX "IX_SottoscrizioniDisponibilita_UtenteId" ON "SottoscrizioniDisponibilita" ("UtenteId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260612075702_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_Utenti_Email" ON "Utenti" ("Email");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260612075702_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_Utenti_Username" ON "Utenti" ("Username");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260612075702_InitialCreate') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260612075702_InitialCreate', '10.0.9');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260612152325_AddPropostaLibro') THEN
    CREATE TABLE "ProposteLibro" (
        "Id" uuid NOT NULL,
        "Titolo" character varying(200) NOT NULL,
        "Autore" character varying(200) NOT NULL,
        "Genere" character varying(100) NOT NULL,
        "Isbn" character varying(20),
        "ProponenteId" uuid NOT NULL,
        "Stato" character varying(20) NOT NULL,
        "DataProposta" timestamp with time zone NOT NULL,
        "DataGestione" timestamp with time zone,
        "LibroCreatoId" uuid,
        CONSTRAINT "PK_ProposteLibro" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ProposteLibro_Libri_LibroCreatoId" FOREIGN KEY ("LibroCreatoId") REFERENCES "Libri" ("Id") ON DELETE SET NULL,
        CONSTRAINT "FK_ProposteLibro_Utenti_ProponenteId" FOREIGN KEY ("ProponenteId") REFERENCES "Utenti" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260612152325_AddPropostaLibro') THEN
    CREATE INDEX "IX_ProposteLibro_LibroCreatoId" ON "ProposteLibro" ("LibroCreatoId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260612152325_AddPropostaLibro') THEN
    CREATE INDEX "IX_ProposteLibro_ProponenteId" ON "ProposteLibro" ("ProponenteId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260612152325_AddPropostaLibro') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260612152325_AddPropostaLibro', '10.0.9');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260824122737_AddAnnoLibro') THEN
    ALTER TABLE "ProposteLibro" ADD "Anno" integer;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260824122737_AddAnnoLibro') THEN
    ALTER TABLE "Libri" ADD "Anno" integer;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260824122737_AddAnnoLibro') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260824122737_AddAnnoLibro', '10.0.9');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260824153958_AddConsensoTerminiEVisualizzazioni') THEN
    ALTER TABLE "Utenti" ADD "DataAccettazioneTermini" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260824153958_AddConsensoTerminiEVisualizzazioni') THEN
    CREATE TABLE "VisualizzazioniLibro" (
        "LibroId" uuid NOT NULL,
        "Giorno" date NOT NULL,
        "Conteggio" integer NOT NULL,
        CONSTRAINT "PK_VisualizzazioniLibro" PRIMARY KEY ("LibroId", "Giorno"),
        CONSTRAINT "FK_VisualizzazioniLibro_Libri_LibroId" FOREIGN KEY ("LibroId") REFERENCES "Libri" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260824153958_AddConsensoTerminiEVisualizzazioni') THEN
    CREATE INDEX "IX_VisualizzazioniLibro_Giorno" ON "VisualizzazioniLibro" ("Giorno");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260824153958_AddConsensoTerminiEVisualizzazioni') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260824153958_AddConsensoTerminiEVisualizzazioni', '10.0.9');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260824165622_AddVocabolarioCategorie') THEN
    CREATE TABLE "Categorie" (
        "Id" uuid NOT NULL,
        "Nome" character varying(50) NOT NULL,
        CONSTRAINT "PK_Categorie" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260824165622_AddVocabolarioCategorie') THEN
    CREATE TABLE "LibriCategorie" (
        "LibroId" uuid NOT NULL,
        "CategoriaId" uuid NOT NULL,
        CONSTRAINT "PK_LibriCategorie" PRIMARY KEY ("LibroId", "CategoriaId"),
        CONSTRAINT "FK_LibriCategorie_Categorie_CategoriaId" FOREIGN KEY ("CategoriaId") REFERENCES "Categorie" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_LibriCategorie_Libri_LibroId" FOREIGN KEY ("LibroId") REFERENCES "Libri" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260824165622_AddVocabolarioCategorie') THEN
    CREATE TABLE "ProposteLibroCategorie" (
        "PropostaLibroId" uuid NOT NULL,
        "CategoriaId" uuid NOT NULL,
        CONSTRAINT "PK_ProposteLibroCategorie" PRIMARY KEY ("PropostaLibroId", "CategoriaId"),
        CONSTRAINT "FK_ProposteLibroCategorie_Categorie_CategoriaId" FOREIGN KEY ("CategoriaId") REFERENCES "Categorie" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_ProposteLibroCategorie_ProposteLibro_PropostaLibroId" FOREIGN KEY ("PropostaLibroId") REFERENCES "ProposteLibro" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260824165622_AddVocabolarioCategorie') THEN
    CREATE UNIQUE INDEX "IX_Categorie_Nome" ON "Categorie" ("Nome");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260824165622_AddVocabolarioCategorie') THEN
    CREATE UNIQUE INDEX "IX_Categorie_Nome_Lower" ON "Categorie" (lower("Nome"));
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260824165622_AddVocabolarioCategorie') THEN
    CREATE INDEX "IX_LibriCategorie_CategoriaId" ON "LibriCategorie" ("CategoriaId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260824165622_AddVocabolarioCategorie') THEN
    CREATE INDEX "IX_ProposteLibroCategorie_CategoriaId" ON "ProposteLibroCategorie" ("CategoriaId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260824165622_AddVocabolarioCategorie') THEN
    INSERT INTO "Categorie" ("Id", "Nome")
    SELECT gen_random_uuid(), upper(left(nome, 1)) || lower(substring(nome from 2))
    FROM (
        SELECT min(left(btrim("Genere"), 50)) AS nome
        FROM (
            SELECT "Genere" FROM "Libri"
            UNION ALL
            SELECT "Genere" FROM "ProposteLibro"
        ) AS generi
        WHERE btrim("Genere") <> ''
        GROUP BY lower(left(btrim("Genere"), 50))
    ) AS distinti;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260824165622_AddVocabolarioCategorie') THEN
    INSERT INTO "LibriCategorie" ("LibroId", "CategoriaId")
    SELECT l."Id", c."Id"
    FROM "Libri" l
    JOIN "Categorie" c ON lower(c."Nome") = lower(left(btrim(l."Genere"), 50))
    WHERE btrim(l."Genere") <> ''
    ON CONFLICT DO NOTHING;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260824165622_AddVocabolarioCategorie') THEN
    INSERT INTO "ProposteLibroCategorie" ("PropostaLibroId", "CategoriaId")
    SELECT p."Id", c."Id"
    FROM "ProposteLibro" p
    JOIN "Categorie" c ON lower(c."Nome") = lower(left(btrim(p."Genere"), 50))
    WHERE btrim(p."Genere") <> ''
    ON CONFLICT DO NOTHING;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260824165622_AddVocabolarioCategorie') THEN
    ALTER TABLE "ProposteLibro" DROP COLUMN "Genere";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260824165622_AddVocabolarioCategorie') THEN
    ALTER TABLE "Libri" DROP COLUMN "Genere";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260824165622_AddVocabolarioCategorie') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260824165622_AddVocabolarioCategorie', '10.0.9');
    END IF;
END $EF$;
COMMIT;

