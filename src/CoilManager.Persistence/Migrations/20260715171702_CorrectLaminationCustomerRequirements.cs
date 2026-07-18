using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoilManager.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CorrectLaminationCustomerRequirements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LaminationJobs_OemJobNumber",
                schema: "app",
                table: "LaminationJobs");

            migrationBuilder.RenameColumn(
                name: "DrawingNumber",
                schema: "app",
                table: "LaminationJobs",
                newName: "JobOrDrawingNumber");

            migrationBuilder.RenameColumn(
                name: "TransformerCompany",
                schema: "app",
                table: "LaminationJobs",
                newName: "Customer");

            migrationBuilder.RenameIndex(
                name: "IX_LaminationJobs_DrawingNumber",
                schema: "app",
                table: "LaminationJobs",
                newName: "IX_LaminationJobs_JobOrDrawingNumber");

            migrationBuilder.DropColumn(
                name: "DrawingRevision",
                schema: "app",
                table: "LaminationJobs");

            migrationBuilder.DropColumn(
                name: "Machine",
                schema: "app",
                table: "LaminationJobs");

            migrationBuilder.Sql("UPDATE [app].[LaminationJobs] SET [JobOrDrawingNumber] = LEFT(COALESCE(NULLIF([JobOrDrawingNumber], ''), [OemJobNumber]), 100)");

            migrationBuilder.AlterColumn<string>(
                name: "JobOrDrawingNumber",
                schema: "app",
                table: "LaminationJobs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Customer",
                schema: "app",
                table: "LaminationJobs",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CustomerCoreLossPerKg",
                schema: "app",
                table: "LaminationJobs",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "NoLoadLossWatts",
                schema: "app",
                table: "LaminationJobs",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalWeight",
                schema: "app",
                table: "LaminationJobs",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.Sql("UPDATE [app].[LaminationJobs] SET [CustomerCoreLossPerKg] = [CoreLossPerKg], [TotalWeight] = CASE WHEN [TotalPlannedWeight] > 0 THEN [TotalPlannedWeight] ELSE 1 END, [NoLoadLossWatts] = ROUND((CASE WHEN [TotalPlannedWeight] > 0 THEN [TotalPlannedWeight] ELSE 1 END) * [CoreLossPerKg] * 1.15, 2)");

            migrationBuilder.DropColumn(name: "OemJobNumber", schema: "app", table: "LaminationJobs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerCoreLossPerKg",
                schema: "app",
                table: "LaminationJobs");

            migrationBuilder.DropColumn(
                name: "NoLoadLossWatts",
                schema: "app",
                table: "LaminationJobs");

            migrationBuilder.DropColumn(
                name: "TotalWeight",
                schema: "app",
                table: "LaminationJobs");

            migrationBuilder.AlterColumn<string>(
                name: "JobOrDrawingNumber",
                schema: "app",
                table: "LaminationJobs",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Customer",
                schema: "app",
                table: "LaminationJobs",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DrawingRevision",
                schema: "app",
                table: "LaminationJobs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Machine",
                schema: "app",
                table: "LaminationJobs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OemJobNumber",
                schema: "app",
                table: "LaminationJobs",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.RenameIndex(
                name: "IX_LaminationJobs_JobOrDrawingNumber",
                schema: "app",
                table: "LaminationJobs",
                newName: "IX_LaminationJobs_DrawingNumber");

            migrationBuilder.RenameColumn(name: "JobOrDrawingNumber", schema: "app", table: "LaminationJobs", newName: "DrawingNumber");
            migrationBuilder.RenameColumn(name: "Customer", schema: "app", table: "LaminationJobs", newName: "TransformerCompany");

            migrationBuilder.CreateIndex(
                name: "IX_LaminationJobs_OemJobNumber",
                schema: "app",
                table: "LaminationJobs",
                column: "OemJobNumber");
        }
    }
}
