using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoilManager.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ReviseLaminationJobWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CompletedBy",
                schema: "app",
                table: "LaminationJobs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CompletedOn",
                schema: "app",
                table: "LaminationJobs",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompletionRemarks",
                schema: "app",
                table: "LaminationJobs",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalConsumedWeight",
                schema: "app",
                table: "LaminationJobs",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "TotalGoodPieces",
                schema: "app",
                table: "LaminationJobs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalRejectedPieces",
                schema: "app",
                table: "LaminationJobs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalScrapWeight",
                schema: "app",
                table: "LaminationJobs",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletedBy",
                schema: "app",
                table: "LaminationJobs");

            migrationBuilder.DropColumn(
                name: "CompletedOn",
                schema: "app",
                table: "LaminationJobs");

            migrationBuilder.DropColumn(
                name: "CompletionRemarks",
                schema: "app",
                table: "LaminationJobs");

            migrationBuilder.DropColumn(
                name: "TotalConsumedWeight",
                schema: "app",
                table: "LaminationJobs");

            migrationBuilder.DropColumn(
                name: "TotalGoodPieces",
                schema: "app",
                table: "LaminationJobs");

            migrationBuilder.DropColumn(
                name: "TotalRejectedPieces",
                schema: "app",
                table: "LaminationJobs");

            migrationBuilder.DropColumn(
                name: "TotalScrapWeight",
                schema: "app",
                table: "LaminationJobs");
        }
    }
}
