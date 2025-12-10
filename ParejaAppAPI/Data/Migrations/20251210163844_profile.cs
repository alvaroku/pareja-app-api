using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParejaAppAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class profile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Resources_ResourceId",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_ResourceId",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "FechaAniversario",
                table: "Usuarios");

            migrationBuilder.RenameColumn(
                name: "ResourceId",
                table: "Usuarios",
                newName: "ProfilePhotoId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_ProfilePhotoId",
                table: "Usuarios",
                column: "ProfilePhotoId",
                unique: true,
                filter: "[ProfilePhotoId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Resources_ProfilePhotoId",
                table: "Usuarios",
                column: "ProfilePhotoId",
                principalTable: "Resources",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Resources_ProfilePhotoId",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_ProfilePhotoId",
                table: "Usuarios");

            migrationBuilder.RenameColumn(
                name: "ProfilePhotoId",
                table: "Usuarios",
                newName: "ResourceId");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaAniversario",
                table: "Usuarios",
                type: "datetime2",
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
    }
}
