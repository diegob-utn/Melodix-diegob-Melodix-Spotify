using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Melodix.Data.Migrations
{
    /// <inheritdoc />
    public partial class v9 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Duracion",
                table: "Pistas",
                newName: "Genero");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Genero",
                table: "Pistas",
                newName: "Duracion");
        }
    }
}
