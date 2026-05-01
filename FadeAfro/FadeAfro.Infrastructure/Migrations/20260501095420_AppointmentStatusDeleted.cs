using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FadeAfro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AppointmentStatusDeleted : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Appointments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Appointments",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
