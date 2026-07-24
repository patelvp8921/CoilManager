using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoilManager.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLaminationPlateTypeWeights : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "BottomPlateWeight",
                schema: "app",
                table: "LaminationJobs",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CenterPlateWeight",
                schema: "app",
                table: "LaminationJobs",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "LeftSidePlateWeight",
                schema: "app",
                table: "LaminationJobs",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "RightSidePlateWeight",
                schema: "app",
                table: "LaminationJobs",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TopPlateWeight",
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
                name: "BottomPlateWeight",
                schema: "app",
                table: "LaminationJobs");

            migrationBuilder.DropColumn(
                name: "CenterPlateWeight",
                schema: "app",
                table: "LaminationJobs");

            migrationBuilder.DropColumn(
                name: "LeftSidePlateWeight",
                schema: "app",
                table: "LaminationJobs");

            migrationBuilder.DropColumn(
                name: "RightSidePlateWeight",
                schema: "app",
                table: "LaminationJobs");

            migrationBuilder.DropColumn(
                name: "TopPlateWeight",
                schema: "app",
                table: "LaminationJobs");
        }
    }
}
