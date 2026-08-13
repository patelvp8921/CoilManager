using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoilManager.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkOrderFoundationAndFulfilmentPlanning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                schema: "app",
                table: "WorkOrders",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerCode",
                schema: "app",
                table: "WorkOrders",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                schema: "app",
                table: "WorkOrders",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerPONumber",
                schema: "app",
                table: "WorkOrders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "app",
                table: "WorkOrders",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "DispatchedQuantity",
                schema: "app",
                table: "WorkOrders",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "DrawingRevision",
                schema: "app",
                table: "WorkOrders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FulfilmentStrategy",
                schema: "app",
                table: "WorkOrders",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "GradeCode",
                schema: "app",
                table: "WorkOrders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Length",
                schema: "app",
                table: "WorkOrders",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OEMJobNumber",
                schema: "app",
                table: "WorkOrders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PlannedInventoryQuantity",
                schema: "app",
                table: "WorkOrders",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PlannedProductionQuantity",
                schema: "app",
                table: "WorkOrders",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateOnly>(
                name: "PlannedStartDate",
                schema: "app",
                table: "WorkOrders",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Planner",
                schema: "app",
                table: "WorkOrders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PlanningRequiredQuantity",
                schema: "app",
                table: "WorkOrders",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ProducedQuantity",
                schema: "app",
                table: "WorkOrders",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ProductionRoute",
                schema: "app",
                table: "WorkOrders",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "QuantityUnit",
                schema: "app",
                table: "WorkOrders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "ReadyQuantity",
                schema: "app",
                table: "WorkOrders",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ReservedInventoryQuantity",
                schema: "app",
                table: "WorkOrders",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "SalesOrderId",
                schema: "app",
                table: "WorkOrders",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SalesOrderLineId",
                schema: "app",
                table: "WorkOrders",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SalesOrderLineNumber",
                schema: "app",
                table: "WorkOrders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SalesOrderNumber",
                schema: "app",
                table: "WorkOrders",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceType",
                schema: "app",
                table: "WorkOrders",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TransformerRating",
                schema: "app",
                table: "WorkOrders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_CustomerId",
                schema: "app",
                table: "WorkOrders",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_FulfilmentStrategy",
                schema: "app",
                table: "WorkOrders",
                column: "FulfilmentStrategy");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_ProductionRoute",
                schema: "app",
                table: "WorkOrders",
                column: "ProductionRoute");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_ProductType",
                schema: "app",
                table: "WorkOrders",
                column: "ProductType");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_SalesOrderId",
                schema: "app",
                table: "WorkOrders",
                column: "SalesOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_SalesOrderLineId",
                schema: "app",
                table: "WorkOrders",
                column: "SalesOrderLineId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_SourceType",
                schema: "app",
                table: "WorkOrders",
                column: "SourceType");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrders_Customers_CustomerId",
                schema: "app",
                table: "WorkOrders",
                column: "CustomerId",
                principalSchema: "sales",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrders_SalesOrderLines_SalesOrderLineId",
                schema: "app",
                table: "WorkOrders",
                column: "SalesOrderLineId",
                principalSchema: "sales",
                principalTable: "SalesOrderLines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrders_SalesOrders_SalesOrderId",
                schema: "app",
                table: "WorkOrders",
                column: "SalesOrderId",
                principalSchema: "sales",
                principalTable: "SalesOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrders_Customers_CustomerId",
                schema: "app",
                table: "WorkOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrders_SalesOrderLines_SalesOrderLineId",
                schema: "app",
                table: "WorkOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrders_SalesOrders_SalesOrderId",
                schema: "app",
                table: "WorkOrders");

            migrationBuilder.DropIndex(
                name: "IX_WorkOrders_CustomerId",
                schema: "app",
                table: "WorkOrders");

            migrationBuilder.DropIndex(
                name: "IX_WorkOrders_FulfilmentStrategy",
                schema: "app",
                table: "WorkOrders");

            migrationBuilder.DropIndex(
                name: "IX_WorkOrders_ProductionRoute",
                schema: "app",
                table: "WorkOrders");

            migrationBuilder.DropIndex(
                name: "IX_WorkOrders_ProductType",
                schema: "app",
                table: "WorkOrders");

            migrationBuilder.DropIndex(
                name: "IX_WorkOrders_SalesOrderId",
                schema: "app",
                table: "WorkOrders");

            migrationBuilder.DropIndex(
                name: "IX_WorkOrders_SalesOrderLineId",
                schema: "app",
                table: "WorkOrders");

            migrationBuilder.DropIndex(
                name: "IX_WorkOrders_SourceType",
                schema: "app",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "CancellationReason",
                schema: "app",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "CustomerCode",
                schema: "app",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                schema: "app",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "CustomerPONumber",
                schema: "app",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "Description",
                schema: "app",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "DispatchedQuantity",
                schema: "app",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "DrawingRevision",
                schema: "app",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "FulfilmentStrategy",
                schema: "app",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "GradeCode",
                schema: "app",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "Length",
                schema: "app",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "OEMJobNumber",
                schema: "app",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "PlannedInventoryQuantity",
                schema: "app",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "PlannedProductionQuantity",
                schema: "app",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "PlannedStartDate",
                schema: "app",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "Planner",
                schema: "app",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "PlanningRequiredQuantity",
                schema: "app",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "ProducedQuantity",
                schema: "app",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "ProductionRoute",
                schema: "app",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "QuantityUnit",
                schema: "app",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "ReadyQuantity",
                schema: "app",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "ReservedInventoryQuantity",
                schema: "app",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "SalesOrderId",
                schema: "app",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "SalesOrderLineId",
                schema: "app",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "SalesOrderLineNumber",
                schema: "app",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "SalesOrderNumber",
                schema: "app",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "SourceType",
                schema: "app",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "TransformerRating",
                schema: "app",
                table: "WorkOrders");
        }
    }
}
