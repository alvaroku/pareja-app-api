using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParejaAppAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class resource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UrlFoto",
                table: "Memorias");

            migrationBuilder.AddColumn<int>(
                name: "ResourceId",
                table: "Memorias",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Resources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Extension = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tamaño = table.Column<long>(type: "bigint", nullable: false),
                    UrlPublica = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Ubicacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UsuarioId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Resources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Resources_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Memorias_ResourceId",
                table: "Memorias",
                column: "ResourceId",
                unique: true,
                filter: "[ResourceId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_UsuarioId",
                table: "Resources",
                column: "UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Memorias_Resources_ResourceId",
                table: "Memorias",
                column: "ResourceId",
                principalTable: "Resources",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Memorias_Resources_ResourceId",
                table: "Memorias");

            migrationBuilder.DropTable(
                name: "Resources");

            migrationBuilder.DropIndex(
                name: "IX_Memorias_ResourceId",
                table: "Memorias");

            migrationBuilder.DropColumn(
                name: "ResourceId",
                table: "Memorias");

            migrationBuilder.AddColumn<string>(
                name: "UrlFoto",
                table: "Memorias",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
