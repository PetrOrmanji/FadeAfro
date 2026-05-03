using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FadeAfro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DescriptionRemovedFromMasterProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "MasterProfiles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "MasterProfiles",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);
        }
    }
}
