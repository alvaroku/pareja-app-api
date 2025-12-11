using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParejaAppAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class notification_MinutosAntesNotificar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MinutosAntesNotificar",
                table: "Citas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "NotificacionEnviada",
                table: "Citas",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MinutosAntesNotificar",
                table: "Citas");

            migrationBuilder.DropColumn(
                name: "NotificacionEnviada",
                table: "Citas");
        }
    }
}
