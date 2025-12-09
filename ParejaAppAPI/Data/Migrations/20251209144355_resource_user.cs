using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParejaAppAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class resource_user : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Resources_Usuarios_UsuarioId",
                table: "Resources");

            migrationBuilder.DropIndex(
                name: "IX_Resources_UsuarioId",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "FotoPerfil",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "Resources");

            migrationBuilder.AddColumn<int>(
                name: "ResourceId",
                table: "Usuarios",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_ResourceId",
                table: "Usuarios",
                column: "ResourceId",
                unique: true,
                filter: "[ResourceId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Resources_ResourceId",
                table: "Usuarios",
                column: "ResourceId",
                principalTable: "Resources",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Resources_ResourceId",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_ResourceId",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "ResourceId",
                table: "Usuarios");

            migrationBuilder.AddColumn<string>(
                name: "FotoPerfil",
                table: "Usuarios",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UsuarioId",
                table: "Resources",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Resources_UsuarioId",
                table: "Resources",
                column: "UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Resources_Usuarios_UsuarioId",
                table: "Resources",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id");
        }
    }
}
