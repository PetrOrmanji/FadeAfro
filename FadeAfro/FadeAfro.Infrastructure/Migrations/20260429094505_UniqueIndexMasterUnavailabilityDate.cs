using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FadeAfro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UniqueIndexMasterUnavailabilityDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MasterUnavailabilities_MasterProfileId",
                table: "MasterUnavailabilities");

            migrationBuilder.CreateIndex(
                name: "IX_MasterUnavailabilities_MasterProfileId_Date",
                table: "MasterUnavailabilities",
                columns: new[] { "MasterProfileId", "Date" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MasterUnavailabilities_MasterProfileId_Date",
                table: "MasterUnavailabilities");

            migrationBuilder.CreateIndex(
                name: "IX_MasterUnavailabilities_MasterProfileId",
                table: "MasterUnavailabilities",
                column: "MasterProfileId");
        }
    }
}
