using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoilManager.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddManufacturerMasterToRawCoils : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Manufacturers",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Manufacturers", x => x.Id);
                });

            migrationBuilder.InsertData(
                schema: "app",
                table: "Manufacturers",
                columns: ["Id", "Name", "Code", "IsActive", "CreatedAtUtc"],
                values: new object[,]
                {
                    { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb1"), "Tata Steel", "TATA", true, new DateTimeOffset(new DateTime(2026, 7, 6, 0, 0, 0, DateTimeKind.Utc)) },
                    { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb2"), "JSW Steel", "JSW", true, new DateTimeOffset(new DateTime(2026, 7, 6, 0, 0, 0, DateTimeKind.Utc)) },
                    { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb3"), "SAIL", "SAIL", true, new DateTimeOffset(new DateTime(2026, 7, 6, 0, 0, 0, DateTimeKind.Utc)) }
                });

            migrationBuilder.Sql("""
                INSERT INTO app.Manufacturers (Id, Name, Code, IsActive, CreatedAtUtc)
                SELECT NEWID(), MillName, CONCAT('MFG', FORMAT(ROW_NUMBER() OVER (ORDER BY MillName), '0000')), 1, SYSDATETIMEOFFSET()
                FROM (
                    SELECT DISTINCT MillName
                    FROM app.RawCoils rawCoil
                    WHERE MillName IS NOT NULL
                      AND LTRIM(RTRIM(MillName)) <> ''
                      AND NOT EXISTS (
                          SELECT 1
                          FROM app.Manufacturers
                          WHERE Name = rawCoil.MillName
                      )
                ) manufacturers;
                """);

            migrationBuilder.AddColumn<Guid>(
                name: "ManufacturerId",
                schema: "app",
                table: "RawCoils",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE rawCoil
                SET ManufacturerId = manufacturer.Id
                FROM app.RawCoils rawCoil
                INNER JOIN app.Manufacturers manufacturer ON manufacturer.Name = rawCoil.MillName;

                UPDATE app.RawCoils
                SET ManufacturerId = 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb1'
                WHERE ManufacturerId IS NULL;
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "ManufacturerId",
                schema: "app",
                table: "RawCoils",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RawCoils_ManufacturerId",
                schema: "app",
                table: "RawCoils",
                column: "ManufacturerId");

            migrationBuilder.CreateIndex(
                name: "IX_Manufacturers_Code",
                schema: "app",
                table: "Manufacturers",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RawCoils_Manufacturers_ManufacturerId",
                schema: "app",
                table: "RawCoils",
                column: "ManufacturerId",
                principalSchema: "app",
                principalTable: "Manufacturers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.DropColumn(
                name: "MillName",
                schema: "app",
                table: "RawCoils");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RawCoils_Manufacturers_ManufacturerId",
                schema: "app",
                table: "RawCoils");

            migrationBuilder.DropTable(
                name: "Manufacturers",
                schema: "app");

            migrationBuilder.DropIndex(
                name: "IX_RawCoils_ManufacturerId",
                schema: "app",
                table: "RawCoils");

            migrationBuilder.DropColumn(
                name: "ManufacturerId",
                schema: "app",
                table: "RawCoils");

            migrationBuilder.AddColumn<string>(
                name: "MillName",
                schema: "app",
                table: "RawCoils",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
