using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoilManager.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkOrdersAndManualMaterialAllocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProductionType",
                schema: "app",
                table: "SlittingJobs",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "WorkOrderId",
                schema: "app",
                table: "SlittingJobs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkOrderNumber",
                schema: "app",
                table: "SlittingJobs",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "WorkOrderOperationId",
                schema: "app",
                table: "SlittingJobs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "WorkOrders",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkOrderNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    WorkOrderType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ProductType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SalesOrderReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    WorkOrderDate = table.Column<DateOnly>(type: "date", nullable: false),
                    RequiredDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    GradeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Thickness = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CoreLossPerKg = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    DrawingNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RequiredWidth = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    RequiredWeight = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    RequiredQuantity = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ReleasedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ReleasedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    StartedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    StartedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CompletedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CompletedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ClosedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ClosedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CancelledBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CancelledOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkOrders_Grades_GradeId",
                        column: x => x.GradeId,
                        principalSchema: "app",
                        principalTable: "Grades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkOrderMaterialAllocations",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CoilType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MotherCoilId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SlitCoilId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CoilNumber = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    AllocatedWeight = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    IssuedWeight = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    ConsumedWeight = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    RemainingWeightAfterAllocation = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ReservedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ReservedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ReleasedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ReleasedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrderMaterialAllocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkOrderMaterialAllocations_RawCoils_MotherCoilId",
                        column: x => x.MotherCoilId,
                        principalSchema: "app",
                        principalTable: "RawCoils",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkOrderMaterialAllocations_SlitCoils_SlitCoilId",
                        column: x => x.SlitCoilId,
                        principalSchema: "app",
                        principalTable: "SlitCoils",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkOrderMaterialAllocations_WorkOrders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalSchema: "app",
                        principalTable: "WorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkOrderOperations",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OperationType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    RelatedDocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RelatedDocumentNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    StartedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CompletedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrderOperations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkOrderOperations_WorkOrders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalSchema: "app",
                        principalTable: "WorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SlittingJobs_WorkOrderId",
                schema: "app",
                table: "SlittingJobs",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderMaterialAllocations_MotherCoilId",
                schema: "app",
                table: "WorkOrderMaterialAllocations",
                column: "MotherCoilId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderMaterialAllocations_SlitCoilId",
                schema: "app",
                table: "WorkOrderMaterialAllocations",
                column: "SlitCoilId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderMaterialAllocations_WorkOrderId",
                schema: "app",
                table: "WorkOrderMaterialAllocations",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderOperations_WorkOrderId_OperationType",
                schema: "app",
                table: "WorkOrderOperations",
                columns: new[] { "WorkOrderId", "OperationType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_GradeId",
                schema: "app",
                table: "WorkOrders",
                column: "GradeId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_RequiredDate",
                schema: "app",
                table: "WorkOrders",
                column: "RequiredDate");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_Status",
                schema: "app",
                table: "WorkOrders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_WorkOrderNumber",
                schema: "app",
                table: "WorkOrders",
                column: "WorkOrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_WorkOrderType_ProductType",
                schema: "app",
                table: "WorkOrders",
                columns: new[] { "WorkOrderType", "ProductType" });

            migrationBuilder.AddForeignKey(
                name: "FK_SlittingJobs_WorkOrders_WorkOrderId",
                schema: "app",
                table: "SlittingJobs",
                column: "WorkOrderId",
                principalSchema: "app",
                principalTable: "WorkOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SlittingJobs_WorkOrders_WorkOrderId",
                schema: "app",
                table: "SlittingJobs");

            migrationBuilder.DropTable(
                name: "WorkOrderMaterialAllocations",
                schema: "app");

            migrationBuilder.DropTable(
                name: "WorkOrderOperations",
                schema: "app");

            migrationBuilder.DropTable(
                name: "WorkOrders",
                schema: "app");

            migrationBuilder.DropIndex(
                name: "IX_SlittingJobs_WorkOrderId",
                schema: "app",
                table: "SlittingJobs");

            migrationBuilder.DropColumn(
                name: "ProductionType",
                schema: "app",
                table: "SlittingJobs");

            migrationBuilder.DropColumn(
                name: "WorkOrderId",
                schema: "app",
                table: "SlittingJobs");

            migrationBuilder.DropColumn(
                name: "WorkOrderNumber",
                schema: "app",
                table: "SlittingJobs");

            migrationBuilder.DropColumn(
                name: "WorkOrderOperationId",
                schema: "app",
                table: "SlittingJobs");
        }
    }
}
