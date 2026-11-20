using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APortataDiLibro.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddPropostaLibro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProposteLibro",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Titolo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Autore = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Genere = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Isbn = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ProponenteId = table.Column<Guid>(type: "uuid", nullable: false),
                    Stato = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DataProposta = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataGestione = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LibroCreatoId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProposteLibro", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProposteLibro_Libri_LibroCreatoId",
                        column: x => x.LibroCreatoId,
                        principalTable: "Libri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ProposteLibro_Utenti_ProponenteId",
                        column: x => x.ProponenteId,
                        principalTable: "Utenti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProposteLibro_LibroCreatoId",
                table: "ProposteLibro",
                column: "LibroCreatoId");

            migrationBuilder.CreateIndex(
                name: "IX_ProposteLibro_ProponenteId",
                table: "ProposteLibro",
                column: "ProponenteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProposteLibro");
        }
    }
}
