using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Melodix.Data.Migrations
{
    /// <inheritdoc />
    public partial class v10 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UsuariosLikeAlbum_Albums_AlbumId",
                table: "UsuariosLikeAlbum");

            migrationBuilder.DropColumn(
                name: "RutaImagen",
                table: "Pistas");

            migrationBuilder.DropColumn(
                name: "RutaImagen",
                table: "ListasReproduccion");

            migrationBuilder.DropColumn(
                name: "RutaImagen",
                table: "Albums");

            migrationBuilder.AlterColumn<int>(
                name: "AlbumId",
                table: "UsuariosLikeAlbum",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_UsuariosLikeAlbum_Albums_AlbumId",
                table: "UsuariosLikeAlbum",
                column: "AlbumId",
                principalTable: "Albums",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UsuariosLikeAlbum_Albums_AlbumId",
                table: "UsuariosLikeAlbum");

            migrationBuilder.AlterColumn<int>(
                name: "AlbumId",
                table: "UsuariosLikeAlbum",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RutaImagen",
                table: "Pistas",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RutaImagen",
                table: "ListasReproduccion",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RutaImagen",
                table: "Albums",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UsuariosLikeAlbum_Albums_AlbumId",
                table: "UsuariosLikeAlbum",
                column: "AlbumId",
                principalTable: "Albums",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
