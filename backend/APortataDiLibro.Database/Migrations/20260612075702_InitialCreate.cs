using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APortataDiLibro.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Libri",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Titolo = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Autore = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Genere = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Isbn = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Libri", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Utenti",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Nickname = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Cognome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Ruolo = table.Column<int>(type: "integer", nullable: false),
                    DataRegistrazione = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Utenti", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CopieLibro",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LibroId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProprietarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    PercorsoCopertina = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: true),
                    PercorsoCopertinaThumbnail = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: true),
                    Stato = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CopieLibro", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CopieLibro_Libri_LibroId",
                        column: x => x.LibroId,
                        principalTable: "Libri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CopieLibro_Utenti_ProprietarioId",
                        column: x => x.ProprietarioId,
                        principalTable: "Utenti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Hub",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UtenteId = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Indirizzo = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Lat = table.Column<double>(type: "double precision", nullable: false),
                    Lng = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hub", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Hub_Utenti_UtenteId",
                        column: x => x.UtenteId,
                        principalTable: "Utenti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecensioniLibro",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LibroId = table.Column<Guid>(type: "uuid", nullable: false),
                    AutoreId = table.Column<Guid>(type: "uuid", nullable: false),
                    Voto = table.Column<int>(type: "integer", nullable: false),
                    Commento = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DataCreazione = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecensioniLibro", x => x.Id);
                    table.CheckConstraint("CK_RecensioniLibro_Voto", "\"Voto\" BETWEEN 1 AND 5");
                    table.ForeignKey(
                        name: "FK_RecensioniLibro_Libri_LibroId",
                        column: x => x.LibroId,
                        principalTable: "Libri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecensioniLibro_Utenti_AutoreId",
                        column: x => x.AutoreId,
                        principalTable: "Utenti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SottoscrizioniDisponibilita",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LibroId = table.Column<Guid>(type: "uuid", nullable: false),
                    UtenteId = table.Column<Guid>(type: "uuid", nullable: false),
                    DataRichiesta = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Notificata = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SottoscrizioniDisponibilita", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SottoscrizioniDisponibilita_Libri_LibroId",
                        column: x => x.LibroId,
                        principalTable: "Libri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SottoscrizioniDisponibilita_Utenti_UtenteId",
                        column: x => x.UtenteId,
                        principalTable: "Utenti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Prestiti",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CopiaLibroId = table.Column<Guid>(type: "uuid", nullable: false),
                    RichiedenteId = table.Column<Guid>(type: "uuid", nullable: false),
                    Stato = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DataRichiesta = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GiorniStimati = table.Column<int>(type: "integer", nullable: false),
                    DataRestituzioneEffettiva = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prestiti", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prestiti_CopieLibro_CopiaLibroId",
                        column: x => x.CopiaLibroId,
                        principalTable: "CopieLibro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Prestiti_Utenti_RichiedenteId",
                        column: x => x.RichiedenteId,
                        principalTable: "Utenti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EsclusioniHubLibro",
                columns: table => new
                {
                    CopiaLibroId = table.Column<Guid>(type: "uuid", nullable: false),
                    HubId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EsclusioniHubLibro", x => new { x.CopiaLibroId, x.HubId });
                    table.ForeignKey(
                        name: "FK_EsclusioniHubLibro_CopieLibro_CopiaLibroId",
                        column: x => x.CopiaLibroId,
                        principalTable: "CopieLibro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EsclusioniHubLibro_Hub_HubId",
                        column: x => x.HubId,
                        principalTable: "Hub",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MessaggiChat",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PrestitoId = table.Column<Guid>(type: "uuid", nullable: false),
                    MittenteId = table.Column<Guid>(type: "uuid", nullable: false),
                    Testo = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    DataInvio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessaggiChat", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MessaggiChat_Prestiti_PrestitoId",
                        column: x => x.PrestitoId,
                        principalTable: "Prestiti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MessaggiChat_Utenti_MittenteId",
                        column: x => x.MittenteId,
                        principalTable: "Utenti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RecensioniCopia",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PrestitoId = table.Column<Guid>(type: "uuid", nullable: false),
                    AutoreId = table.Column<Guid>(type: "uuid", nullable: false),
                    Voto = table.Column<int>(type: "integer", nullable: false),
                    Commento = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DataCreazione = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecensioniCopia", x => x.Id);
                    table.CheckConstraint("CK_RecensioniCopia_Voto", "\"Voto\" BETWEEN 1 AND 5");
                    table.ForeignKey(
                        name: "FK_RecensioniCopia_Prestiti_PrestitoId",
                        column: x => x.PrestitoId,
                        principalTable: "Prestiti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecensioniCopia_Utenti_AutoreId",
                        column: x => x.AutoreId,
                        principalTable: "Utenti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RecensioniUtente",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PrestitoId = table.Column<Guid>(type: "uuid", nullable: false),
                    AutoreId = table.Column<Guid>(type: "uuid", nullable: false),
                    UtenteRecensitoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Voto = table.Column<int>(type: "integer", nullable: false),
                    Commento = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DataCreazione = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecensioniUtente", x => x.Id);
                    table.CheckConstraint("CK_RecensioniUtente_Voto", "\"Voto\" BETWEEN 1 AND 5");
                    table.ForeignKey(
                        name: "FK_RecensioniUtente_Prestiti_PrestitoId",
                        column: x => x.PrestitoId,
                        principalTable: "Prestiti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecensioniUtente_Utenti_AutoreId",
                        column: x => x.AutoreId,
                        principalTable: "Utenti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecensioniUtente_Utenti_UtenteRecensitoId",
                        column: x => x.UtenteRecensitoId,
                        principalTable: "Utenti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Report",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PrestitoId = table.Column<Guid>(type: "uuid", nullable: false),
                    SegnalanteId = table.Column<Guid>(type: "uuid", nullable: false),
                    UtenteSegnalatoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Motivo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Stato = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DataCreazione = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Report", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Report_Prestiti_PrestitoId",
                        column: x => x.PrestitoId,
                        principalTable: "Prestiti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Report_Utenti_SegnalanteId",
                        column: x => x.SegnalanteId,
                        principalTable: "Utenti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Report_Utenti_UtenteSegnalatoId",
                        column: x => x.UtenteSegnalatoId,
                        principalTable: "Utenti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CopieLibro_LibroId",
                table: "CopieLibro",
                column: "LibroId");

            migrationBuilder.CreateIndex(
                name: "IX_CopieLibro_ProprietarioId",
                table: "CopieLibro",
                column: "ProprietarioId");

            migrationBuilder.CreateIndex(
                name: "IX_EsclusioniHubLibro_HubId",
                table: "EsclusioniHubLibro",
                column: "HubId");

            migrationBuilder.CreateIndex(
                name: "IX_Hub_Lat_Lng",
                table: "Hub",
                columns: new[] { "Lat", "Lng" });

            migrationBuilder.CreateIndex(
                name: "IX_Hub_UtenteId",
                table: "Hub",
                column: "UtenteId");

            migrationBuilder.CreateIndex(
                name: "IX_MessaggiChat_MittenteId",
                table: "MessaggiChat",
                column: "MittenteId");

            migrationBuilder.CreateIndex(
                name: "IX_MessaggiChat_PrestitoId_DataInvio",
                table: "MessaggiChat",
                columns: new[] { "PrestitoId", "DataInvio" });

            migrationBuilder.CreateIndex(
                name: "IX_Prestiti_CopiaLibroId",
                table: "Prestiti",
                column: "CopiaLibroId");

            migrationBuilder.CreateIndex(
                name: "IX_Prestiti_RichiedenteId",
                table: "Prestiti",
                column: "RichiedenteId");

            migrationBuilder.CreateIndex(
                name: "IX_RecensioniCopia_AutoreId",
                table: "RecensioniCopia",
                column: "AutoreId");

            migrationBuilder.CreateIndex(
                name: "IX_RecensioniCopia_PrestitoId_AutoreId",
                table: "RecensioniCopia",
                columns: new[] { "PrestitoId", "AutoreId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecensioniLibro_AutoreId",
                table: "RecensioniLibro",
                column: "AutoreId");

            migrationBuilder.CreateIndex(
                name: "IX_RecensioniLibro_LibroId",
                table: "RecensioniLibro",
                column: "LibroId");

            migrationBuilder.CreateIndex(
                name: "IX_RecensioniUtente_AutoreId",
                table: "RecensioniUtente",
                column: "AutoreId");

            migrationBuilder.CreateIndex(
                name: "IX_RecensioniUtente_PrestitoId_AutoreId",
                table: "RecensioniUtente",
                columns: new[] { "PrestitoId", "AutoreId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecensioniUtente_UtenteRecensitoId",
                table: "RecensioniUtente",
                column: "UtenteRecensitoId");

            migrationBuilder.CreateIndex(
                name: "IX_Report_PrestitoId",
                table: "Report",
                column: "PrestitoId");

            migrationBuilder.CreateIndex(
                name: "IX_Report_SegnalanteId",
                table: "Report",
                column: "SegnalanteId");

            migrationBuilder.CreateIndex(
                name: "IX_Report_UtenteSegnalatoId",
                table: "Report",
                column: "UtenteSegnalatoId");

            migrationBuilder.CreateIndex(
                name: "IX_SottoscrizioniDisponibilita_LibroId_UtenteId",
                table: "SottoscrizioniDisponibilita",
                columns: new[] { "LibroId", "UtenteId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SottoscrizioniDisponibilita_UtenteId",
                table: "SottoscrizioniDisponibilita",
                column: "UtenteId");

            migrationBuilder.CreateIndex(
                name: "IX_Utenti_Email",
                table: "Utenti",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Utenti_Username",
                table: "Utenti",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EsclusioniHubLibro");

            migrationBuilder.DropTable(
                name: "MessaggiChat");

            migrationBuilder.DropTable(
                name: "RecensioniCopia");

            migrationBuilder.DropTable(
                name: "RecensioniLibro");

            migrationBuilder.DropTable(
                name: "RecensioniUtente");

            migrationBuilder.DropTable(
                name: "Report");

            migrationBuilder.DropTable(
                name: "SottoscrizioniDisponibilita");

            migrationBuilder.DropTable(
                name: "Hub");

            migrationBuilder.DropTable(
                name: "Prestiti");

            migrationBuilder.DropTable(
                name: "CopieLibro");

            migrationBuilder.DropTable(
                name: "Libri");

            migrationBuilder.DropTable(
                name: "Utenti");
        }
    }
}
