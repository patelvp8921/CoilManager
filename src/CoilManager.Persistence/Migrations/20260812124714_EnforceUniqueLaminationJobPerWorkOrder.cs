using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoilManager.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EnforceUniqueLaminationJobPerWorkOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_LaminationJobs_WorkOrderId",
                schema: "app",
                table: "LaminationJobs",
                column: "WorkOrderId",
                unique: true,
                filter: "[WorkOrderId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LaminationJobs_WorkOrderId",
                schema: "app",
                table: "LaminationJobs");
        }
    }
}
