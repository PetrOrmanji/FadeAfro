using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FadeAfro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UnavailabilitiesOnlyDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "MasterUnavailabilities");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "MasterUnavailabilities");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeOnly>(
                name: "EndTime",
                table: "MasterUnavailabilities",
                type: "time without time zone",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "StartTime",
                table: "MasterUnavailabilities",
                type: "time without time zone",
                nullable: true);
        }
    }
}
