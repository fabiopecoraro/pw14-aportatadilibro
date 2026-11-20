using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APortataDiLibro.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddAnnoLibro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Anno",
                table: "ProposteLibro",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Anno",
                table: "Libri",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Anno",
                table: "ProposteLibro");

            migrationBuilder.DropColumn(
                name: "Anno",
                table: "Libri");
        }
    }
}
