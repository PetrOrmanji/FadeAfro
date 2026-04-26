using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FadeAfro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class IndexToMasterScheduleDayAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MasterSchedules_MasterProfileId",
                table: "MasterSchedules");

            migrationBuilder.CreateIndex(
                name: "IX_MasterSchedules_MasterProfileId_DayOfWeek",
                table: "MasterSchedules",
                columns: new[] { "MasterProfileId", "DayOfWeek" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MasterSchedules_MasterProfileId_DayOfWeek",
                table: "MasterSchedules");

            migrationBuilder.CreateIndex(
                name: "IX_MasterSchedules_MasterProfileId",
                table: "MasterSchedules",
                column: "MasterProfileId");
        }
    }
}
