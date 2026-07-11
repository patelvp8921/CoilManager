using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoilManager.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSlittingWorkInProgressWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CancelledBy",
                schema: "app",
                table: "SlittingJobs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CancelledOn",
                schema: "app",
                table: "SlittingJobs",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompletedBy",
                schema: "app",
                table: "SlittingJobs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CompletedOn",
                schema: "app",
                table: "SlittingJobs",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReleasedBy",
                schema: "app",
                table: "SlittingJobs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ReleasedOn",
                schema: "app",
                table: "SlittingJobs",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StartedBy",
                schema: "app",
                table: "SlittingJobs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "StartedOn",
                schema: "app",
                table: "SlittingJobs",
                type: "datetimeoffset",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancelledBy",
                schema: "app",
                table: "SlittingJobs");

            migrationBuilder.DropColumn(
                name: "CancelledOn",
                schema: "app",
                table: "SlittingJobs");

            migrationBuilder.DropColumn(
                name: "CompletedBy",
                schema: "app",
                table: "SlittingJobs");

            migrationBuilder.DropColumn(
                name: "CompletedOn",
                schema: "app",
                table: "SlittingJobs");

            migrationBuilder.DropColumn(
                name: "ReleasedBy",
                schema: "app",
                table: "SlittingJobs");

            migrationBuilder.DropColumn(
                name: "ReleasedOn",
                schema: "app",
                table: "SlittingJobs");

            migrationBuilder.DropColumn(
                name: "StartedBy",
                schema: "app",
                table: "SlittingJobs");

            migrationBuilder.DropColumn(
                name: "StartedOn",
                schema: "app",
                table: "SlittingJobs");
        }
    }
}
