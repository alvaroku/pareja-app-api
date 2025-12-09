using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParejaAppAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class pareja : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Parejas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioEnviaId = table.Column<int>(type: "int", nullable: false),
                    UsuarioRecibeId = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parejas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Parejas_Usuarios_UsuarioEnviaId",
                        column: x => x.UsuarioEnviaId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Parejas_Usuarios_UsuarioRecibeId",
                        column: x => x.UsuarioRecibeId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Parejas_UsuarioEnviaId",
                table: "Parejas",
                column: "UsuarioEnviaId");

            migrationBuilder.CreateIndex(
                name: "IX_Parejas_UsuarioRecibeId",
                table: "Parejas",
                column: "UsuarioRecibeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Parejas");
        }
    }
}
