using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoilManager.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRawCoilConcurrencyAndIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WidthMm",
                schema: "app",
                table: "RawCoils",
                newName: "Width");

            migrationBuilder.RenameColumn(
                name: "WeightMt",
                schema: "app",
                table: "RawCoils",
                newName: "Weight");

            migrationBuilder.RenameColumn(
                name: "Warehouse",
                schema: "app",
                table: "RawCoils",
                newName: "WarehouseLocation");

            migrationBuilder.RenameColumn(
                name: "ThicknessMm",
                schema: "app",
                table: "RawCoils",
                newName: "Thickness");

            migrationBuilder.RenameColumn(
                name: "Location",
                schema: "app",
                table: "RawCoils",
                newName: "MillTCNo");

            migrationBuilder.AddColumn<string>(
                name: "BISLicNumber",
                schema: "app",
                table: "RawCoils",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CoilID",
                schema: "app",
                table: "RawCoils",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Length",
                schema: "app",
                table: "RawCoils",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "MillName",
                schema: "app",
                table: "RawCoils",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                schema: "app",
                table: "RawCoils",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<decimal>(
                name: "WattLossPerKg",
                schema: "app",
                table: "RawCoils",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_RawCoils_CoilID",
                schema: "app",
                table: "RawCoils",
                column: "CoilID",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RawCoils_CoilID",
                schema: "app",
                table: "RawCoils");

            migrationBuilder.DropColumn(
                name: "BISLicNumber",
                schema: "app",
                table: "RawCoils");

            migrationBuilder.DropColumn(
                name: "CoilID",
                schema: "app",
                table: "RawCoils");

            migrationBuilder.DropColumn(
                name: "Length",
                schema: "app",
                table: "RawCoils");

            migrationBuilder.DropColumn(
                name: "MillName",
                schema: "app",
                table: "RawCoils");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                schema: "app",
                table: "RawCoils");

            migrationBuilder.DropColumn(
                name: "WattLossPerKg",
                schema: "app",
                table: "RawCoils");

            migrationBuilder.RenameColumn(
                name: "Width",
                schema: "app",
                table: "RawCoils",
                newName: "WidthMm");

            migrationBuilder.RenameColumn(
                name: "Weight",
                schema: "app",
                table: "RawCoils",
                newName: "WeightMt");

            migrationBuilder.RenameColumn(
                name: "WarehouseLocation",
                schema: "app",
                table: "RawCoils",
                newName: "Warehouse");

            migrationBuilder.RenameColumn(
                name: "Thickness",
                schema: "app",
                table: "RawCoils",
                newName: "ThicknessMm");

            migrationBuilder.RenameColumn(
                name: "MillTCNo",
                schema: "app",
                table: "RawCoils",
                newName: "Location");
        }
    }
}
