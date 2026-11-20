using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APortataDiLibro.Database.Migrations
{
    /// <summary>
    /// Sostituisce il campo testuale <c>Genere</c> con un vocabolario
    /// controllato di categorie in relazione molti-a-molti con opere e
    /// proposte.
    /// <para>
    /// L'ordine delle operazioni non e' quello generato da EF, che metteva le
    /// <c>DropColumn</c> in testa: le colonne vanno lette per popolare il
    /// vocabolario, quindi si eliminano soltanto dopo aver travasato i dati.
    /// Invertire i due passaggi avrebbe cancellato la classificazione di ogni
    /// libro gia' in catalogo.
    /// </para>
    /// </summary>
    public partial class AddVocabolarioCategorie : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // --- 1. Nuove tabelle -------------------------------------------

            migrationBuilder.CreateTable(
                name: "Categorie",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorie", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LibriCategorie",
                columns: table => new
                {
                    LibroId = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoriaId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LibriCategorie", x => new { x.LibroId, x.CategoriaId });
                    table.ForeignKey(
                        name: "FK_LibriCategorie_Categorie_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorie",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LibriCategorie_Libri_LibroId",
                        column: x => x.LibroId,
                        principalTable: "Libri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProposteLibroCategorie",
                columns: table => new
                {
                    PropostaLibroId = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoriaId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProposteLibroCategorie", x => new { x.PropostaLibroId, x.CategoriaId });
                    table.ForeignKey(
                        name: "FK_ProposteLibroCategorie_Categorie_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorie",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProposteLibroCategorie_ProposteLibro_PropostaLibroId",
                        column: x => x.PropostaLibroId,
                        principalTable: "ProposteLibro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categorie_Nome",
                table: "Categorie",
                column: "Nome",
                unique: true);

            // L'unicita' che conta e' quella case-insensitive: senza, "Giallo" e
            // "giallo" resterebbero due voci distinte, cioe' il problema che il
            // vocabolario controllato esiste per risolvere. Non e' esprimibile
            // con la Fluent API, quindi si crea qui come indice su espressione.
            migrationBuilder.Sql(
                """
                CREATE UNIQUE INDEX "IX_Categorie_Nome_Lower" ON "Categorie" (lower("Nome"));
                """);

            migrationBuilder.CreateIndex(
                name: "IX_LibriCategorie_CategoriaId",
                table: "LibriCategorie",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_ProposteLibroCategorie_CategoriaId",
                table: "ProposteLibroCategorie",
                column: "CategoriaId");

            // --- 2. Travaso dei dati esistenti ------------------------------

            // Il vocabolario iniziale sono i generi gia' presenti, presi una
            // volta sola a parita' di maiuscole/minuscole.
            //
            // La grafia viene normalizzata a "iniziale maiuscola, resto
            // minuscolo": fra "GIALLO", "giallo" e "Giallo" nessuna scelta e'
            // piu' corretta delle altre, e prendere la prima in ordine
            // alfabetico produrrebbe un vocabolario dall'aspetto casuale.
            // Il resto in minuscolo e' la forma giusta in italiano per i
            // titoli composti ("Romanzo storico", non "Romanzo Storico").
            // Casi come "Storia USA" perdono le maiuscole interne: l'Admin
            // puo' correggerli con una rinomina, che ora si propaga da sola a
            // tutte le opere collegate.
            migrationBuilder.Sql(
                """
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
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO "LibriCategorie" ("LibroId", "CategoriaId")
                SELECT l."Id", c."Id"
                FROM "Libri" l
                JOIN "Categorie" c ON lower(c."Nome") = lower(left(btrim(l."Genere"), 50))
                WHERE btrim(l."Genere") <> ''
                ON CONFLICT DO NOTHING;
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO "ProposteLibroCategorie" ("PropostaLibroId", "CategoriaId")
                SELECT p."Id", c."Id"
                FROM "ProposteLibro" p
                JOIN "Categorie" c ON lower(c."Nome") = lower(left(btrim(p."Genere"), 50))
                WHERE btrim(p."Genere") <> ''
                ON CONFLICT DO NOTHING;
                """);

            // --- 3. Rimozione delle colonne ora ridondanti ------------------

            migrationBuilder.DropColumn(
                name: "Genere",
                table: "ProposteLibro");

            migrationBuilder.DropColumn(
                name: "Genere",
                table: "Libri");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Genere",
                table: "ProposteLibro",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Genere",
                table: "Libri",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            // Il ritorno indietro e' per forza lossy: da piu' categorie per
            // opera si torna a un solo campo di testo. Si conserva la prima in
            // ordine alfabetico, che e' anche quella mostrata per prima
            // nell'interfaccia.
            migrationBuilder.Sql(
                """
                UPDATE "Libri" l
                SET "Genere" = COALESCE((
                    SELECT c."Nome"
                    FROM "LibriCategorie" lc
                    JOIN "Categorie" c ON c."Id" = lc."CategoriaId"
                    WHERE lc."LibroId" = l."Id"
                    ORDER BY c."Nome"
                    LIMIT 1
                ), '');
                """);

            migrationBuilder.Sql(
                """
                UPDATE "ProposteLibro" p
                SET "Genere" = COALESCE((
                    SELECT c."Nome"
                    FROM "ProposteLibroCategorie" pc
                    JOIN "Categorie" c ON c."Id" = pc."CategoriaId"
                    WHERE pc."PropostaLibroId" = p."Id"
                    ORDER BY c."Nome"
                    LIMIT 1
                ), '');
                """);

            migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_Categorie_Nome_Lower";""");

            migrationBuilder.DropTable(name: "LibriCategorie");
            migrationBuilder.DropTable(name: "ProposteLibroCategorie");
            migrationBuilder.DropTable(name: "Categorie");
        }
    }
}
