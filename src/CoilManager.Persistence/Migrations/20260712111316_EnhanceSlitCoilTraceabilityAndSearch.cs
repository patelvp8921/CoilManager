using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoilManager.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EnhanceSlitCoilTraceabilityAndSearch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_SlitCoils_BarcodeValue",
                schema: "app",
                table: "SlitCoils",
                column: "BarcodeValue");

            migrationBuilder.CreateIndex(
                name: "IX_SlitCoils_CreatedAtUtc",
                schema: "app",
                table: "SlitCoils",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_SlitCoils_QrCodeValue",
                schema: "app",
                table: "SlitCoils",
                column: "QrCodeValue");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SlitCoils_BarcodeValue",
                schema: "app",
                table: "SlitCoils");

            migrationBuilder.DropIndex(
                name: "IX_SlitCoils_CreatedAtUtc",
                schema: "app",
                table: "SlitCoils");

            migrationBuilder.DropIndex(
                name: "IX_SlitCoils_QrCodeValue",
                schema: "app",
                table: "SlitCoils");
        }
    }
}
