using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoilManager.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDispatchAndPackingSlipWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Dispatches",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DispatchNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PackingSlipNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    WorkOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkOrderNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SalesOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SalesOrderNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SalesOrderLineId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CustomerCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustomerPONumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProductType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GradeCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Thickness = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    Width = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    DrawingNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DrawingRevision = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OEMJobNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TransformerRating = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CoreLossPerKg = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    DispatchQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    QuantityUnit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NetWeight = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    GrossWeight = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    NumberOfPackages = table.Column<int>(type: "int", nullable: true),
                    DispatchDate = table.Column<DateOnly>(type: "date", nullable: true),
                    TransporterName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VehicleNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LRGRNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LRGRDate = table.Column<DateOnly>(type: "date", nullable: true),
                    EWayBillNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EWayBillDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ShippingAddress = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ContactPerson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContactPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PackingRemarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DispatchRemarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DispatchedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DispatchedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CancelledBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CancelledOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dispatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dispatches_WorkOrders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalSchema: "app",
                        principalTable: "WorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DispatchInventorySources",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DispatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InventoryType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    InventoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InventoryNumber = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Width = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DispatchInventorySources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DispatchInventorySources_Dispatches_DispatchId",
                        column: x => x.DispatchId,
                        principalSchema: "app",
                        principalTable: "Dispatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DispatchPackages",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DispatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PackageNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    QuantityUnit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    NetWeight = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    GrossWeight = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    Length = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    Width = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    Height = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Sequence = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DispatchPackages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DispatchPackages_Dispatches_DispatchId",
                        column: x => x.DispatchId,
                        principalSchema: "app",
                        principalTable: "Dispatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Dispatches_DispatchNumber",
                schema: "app",
                table: "Dispatches",
                column: "DispatchNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Dispatches_PackingSlipNumber",
                schema: "app",
                table: "Dispatches",
                column: "PackingSlipNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Dispatches_Status",
                schema: "app",
                table: "Dispatches",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Dispatches_WorkOrderId",
                schema: "app",
                table: "Dispatches",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_DispatchInventorySources_DispatchId",
                schema: "app",
                table: "DispatchInventorySources",
                column: "DispatchId");

            migrationBuilder.CreateIndex(
                name: "IX_DispatchPackages_DispatchId_PackageNumber",
                schema: "app",
                table: "DispatchPackages",
                columns: new[] { "DispatchId", "PackageNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DispatchInventorySources",
                schema: "app");

            migrationBuilder.DropTable(
                name: "DispatchPackages",
                schema: "app");

            migrationBuilder.DropTable(
                name: "Dispatches",
                schema: "app");
        }
    }
}
