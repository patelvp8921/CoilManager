using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoilManager.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSlitCoilLabelPrinting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LabelLastPrintedBy",
                schema: "app",
                table: "SlitCoils",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LabelLastPrintedOn",
                schema: "app",
                table: "SlitCoils",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LabelPrintCount",
                schema: "app",
                table: "SlitCoils",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "LabelPrinted",
                schema: "app",
                table: "SlitCoils",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "SlitCoilLabelPrintHistories",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SlitCoilId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CoilNumber = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    LabelVersion = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PrintedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PrintedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Copies = table.Column<int>(type: "int", nullable: false),
                    PrinterName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    PrintType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SlitCoilLabelPrintHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SlitCoilLabelPrintHistories_SlitCoils_SlitCoilId",
                        column: x => x.SlitCoilId,
                        principalSchema: "app",
                        principalTable: "SlitCoils",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SlitCoilLabelPrintHistories_CoilNumber",
                schema: "app",
                table: "SlitCoilLabelPrintHistories",
                column: "CoilNumber");

            migrationBuilder.CreateIndex(
                name: "IX_SlitCoilLabelPrintHistories_PrintedOn",
                schema: "app",
                table: "SlitCoilLabelPrintHistories",
                column: "PrintedOn");

            migrationBuilder.CreateIndex(
                name: "IX_SlitCoilLabelPrintHistories_SlitCoilId",
                schema: "app",
                table: "SlitCoilLabelPrintHistories",
                column: "SlitCoilId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SlitCoilLabelPrintHistories",
                schema: "app");

            migrationBuilder.DropColumn(
                name: "LabelLastPrintedBy",
                schema: "app",
                table: "SlitCoils");

            migrationBuilder.DropColumn(
                name: "LabelLastPrintedOn",
                schema: "app",
                table: "SlitCoils");

            migrationBuilder.DropColumn(
                name: "LabelPrintCount",
                schema: "app",
                table: "SlitCoils");

            migrationBuilder.DropColumn(
                name: "LabelPrinted",
                schema: "app",
                table: "SlitCoils");
        }
    }
}
