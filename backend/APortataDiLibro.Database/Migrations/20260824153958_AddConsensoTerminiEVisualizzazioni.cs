using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APortataDiLibro.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddConsensoTerminiEVisualizzazioni : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DataAccettazioneTermini",
                table: "Utenti",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "VisualizzazioniLibro",
                columns: table => new
                {
                    LibroId = table.Column<Guid>(type: "uuid", nullable: false),
                    Giorno = table.Column<DateOnly>(type: "date", nullable: false),
                    Conteggio = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisualizzazioniLibro", x => new { x.LibroId, x.Giorno });
                    table.ForeignKey(
                        name: "FK_VisualizzazioniLibro_Libri_LibroId",
                        column: x => x.LibroId,
                        principalTable: "Libri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VisualizzazioniLibro_Giorno",
                table: "VisualizzazioniLibro",
                column: "Giorno");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VisualizzazioniLibro");

            migrationBuilder.DropColumn(
                name: "DataAccettazioneTermini",
                table: "Utenti");
        }
    }
}
