using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoilManager.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class OptimizeRawCoilListIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_RawCoils_CreatedAtUtc",
                schema: "app",
                table: "RawCoils",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_RawCoils_ReceivedDate",
                schema: "app",
                table: "RawCoils",
                column: "ReceivedDate");

            migrationBuilder.CreateIndex(
                name: "IX_RawCoils_Status",
                schema: "app",
                table: "RawCoils",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RawCoils_CreatedAtUtc",
                schema: "app",
                table: "RawCoils");

            migrationBuilder.DropIndex(
                name: "IX_RawCoils_ReceivedDate",
                schema: "app",
                table: "RawCoils");

            migrationBuilder.DropIndex(
                name: "IX_RawCoils_Status",
                schema: "app",
                table: "RawCoils");
        }
    }
}
