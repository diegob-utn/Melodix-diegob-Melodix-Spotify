using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Melodix.Data.Migrations
{
    /// <inheritdoc />
    public partial class v2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UrlPortada",
                table: "Pistas",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "SpotifyPistaId",
                table: "Pistas",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<int>(
                name: "ContadorReproducciones",
                table: "Pistas",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "EsExplicita",
                table: "Pistas",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaSubida",
                table: "Pistas",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "GeneroId",
                table: "Pistas",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RutaArchivo",
                table: "Pistas",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RutaImagen",
                table: "Pistas",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UsuarioId",
                table: "Pistas",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "UrlPortada",
                table: "Albums",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Titulo",
                table: "Albums",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "SpotifyAlbumId",
                table: "Albums",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "Descripcion",
                table: "Albums",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RutaImagen",
                table: "Albums",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UsuarioId",
                table: "Albums",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Generos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Generos", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Pistas_GeneroId",
                table: "Pistas",
                column: "GeneroId");

            migrationBuilder.CreateIndex(
                name: "IX_Pistas_UsuarioId",
                table: "Pistas",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Albums_UsuarioId",
                table: "Albums",
                column: "UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Albums_AspNetUsers_UsuarioId",
                table: "Albums",
                column: "UsuarioId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Pistas_AspNetUsers_UsuarioId",
                table: "Pistas",
                column: "UsuarioId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Pistas_Generos_GeneroId",
                table: "Pistas",
                column: "GeneroId",
                principalTable: "Generos",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Albums_AspNetUsers_UsuarioId",
                table: "Albums");

            migrationBuilder.DropForeignKey(
                name: "FK_Pistas_AspNetUsers_UsuarioId",
                table: "Pistas");

            migrationBuilder.DropForeignKey(
                name: "FK_Pistas_Generos_GeneroId",
                table: "Pistas");

            migrationBuilder.DropTable(
                name: "Generos");

            migrationBuilder.DropIndex(
                name: "IX_Pistas_GeneroId",
                table: "Pistas");

            migrationBuilder.DropIndex(
                name: "IX_Pistas_UsuarioId",
                table: "Pistas");

            migrationBuilder.DropIndex(
                name: "IX_Albums_UsuarioId",
                table: "Albums");

            migrationBuilder.DropColumn(
                name: "ContadorReproducciones",
                table: "Pistas");

            migrationBuilder.DropColumn(
                name: "EsExplicita",
                table: "Pistas");

            migrationBuilder.DropColumn(
                name: "FechaSubida",
                table: "Pistas");

            migrationBuilder.DropColumn(
                name: "GeneroId",
                table: "Pistas");

            migrationBuilder.DropColumn(
                name: "RutaArchivo",
                table: "Pistas");

            migrationBuilder.DropColumn(
                name: "RutaImagen",
                table: "Pistas");

            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "Pistas");

            migrationBuilder.DropColumn(
                name: "Descripcion",
                table: "Albums");

            migrationBuilder.DropColumn(
                name: "RutaImagen",
                table: "Albums");

            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "Albums");

            migrationBuilder.AlterColumn<string>(
                name: "UrlPortada",
                table: "Pistas",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SpotifyPistaId",
                table: "Pistas",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UrlPortada",
                table: "Albums",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Titulo",
                table: "Albums",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "SpotifyAlbumId",
                table: "Albums",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);
        }
    }
}
