using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoilManager.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSlitCoilsAndInventoryTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ActualWeight",
                schema: "app",
                table: "SlittingJobItems",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ActualWidth",
                schema: "app",
                table: "SlittingJobItems",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "InventoryTransactions",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransactionType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CoilType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CoilId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CoilNumber = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    RelatedDocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RelatedDocumentNumber = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    FromStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ToStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    QuantityWeight = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    TransactionDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryTransactions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SlitCoils",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CoilNumber = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    ParentCoilId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RootMotherCoilId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MotherCoilId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SlittingJobId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SlitSequence = table.Column<int>(type: "int", nullable: false),
                    GenerationLevel = table.Column<int>(type: "int", nullable: false),
                    GradeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ManufacturerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HeatNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Thickness = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CoreLossPerKg = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Width = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    WarehouseLocation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    BarcodeValue = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    QrCodeValue = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    LabelVersion = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SlitCoils", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SlitCoils_Grades_GradeId",
                        column: x => x.GradeId,
                        principalSchema: "app",
                        principalTable: "Grades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SlitCoils_Manufacturers_ManufacturerId",
                        column: x => x.ManufacturerId,
                        principalSchema: "app",
                        principalTable: "Manufacturers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SlitCoils_RawCoils_MotherCoilId",
                        column: x => x.MotherCoilId,
                        principalSchema: "app",
                        principalTable: "RawCoils",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SlitCoils_SlittingJobs_SlittingJobId",
                        column: x => x.SlittingJobId,
                        principalSchema: "app",
                        principalTable: "SlittingJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SlitCoils_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalSchema: "app",
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_CoilId",
                schema: "app",
                table: "InventoryTransactions",
                column: "CoilId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_CoilNumber",
                schema: "app",
                table: "InventoryTransactions",
                column: "CoilNumber");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_RelatedDocumentId",
                schema: "app",
                table: "InventoryTransactions",
                column: "RelatedDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_TransactionDate",
                schema: "app",
                table: "InventoryTransactions",
                column: "TransactionDate");

            migrationBuilder.CreateIndex(
                name: "IX_SlitCoils_CoilNumber",
                schema: "app",
                table: "SlitCoils",
                column: "CoilNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SlitCoils_GradeId",
                schema: "app",
                table: "SlitCoils",
                column: "GradeId");

            migrationBuilder.CreateIndex(
                name: "IX_SlitCoils_ManufacturerId",
                schema: "app",
                table: "SlitCoils",
                column: "ManufacturerId");

            migrationBuilder.CreateIndex(
                name: "IX_SlitCoils_MotherCoilId",
                schema: "app",
                table: "SlitCoils",
                column: "MotherCoilId");

            migrationBuilder.CreateIndex(
                name: "IX_SlitCoils_ParentCoilId",
                schema: "app",
                table: "SlitCoils",
                column: "ParentCoilId");

            migrationBuilder.CreateIndex(
                name: "IX_SlitCoils_RootMotherCoilId",
                schema: "app",
                table: "SlitCoils",
                column: "RootMotherCoilId");

            migrationBuilder.CreateIndex(
                name: "IX_SlitCoils_SlittingJobId",
                schema: "app",
                table: "SlitCoils",
                column: "SlittingJobId");

            migrationBuilder.CreateIndex(
                name: "IX_SlitCoils_Status",
                schema: "app",
                table: "SlitCoils",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SlitCoils_SupplierId",
                schema: "app",
                table: "SlitCoils",
                column: "SupplierId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventoryTransactions",
                schema: "app");

            migrationBuilder.DropTable(
                name: "SlitCoils",
                schema: "app");

            migrationBuilder.DropColumn(
                name: "ActualWeight",
                schema: "app",
                table: "SlittingJobItems");

            migrationBuilder.DropColumn(
                name: "ActualWidth",
                schema: "app",
                table: "SlittingJobItems");
        }
    }
}
