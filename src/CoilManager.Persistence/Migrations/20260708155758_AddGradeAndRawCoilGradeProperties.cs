using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoilManager.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddGradeAndRawCoilGradeProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                schema: "app",
                table: "RawCoils",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "CoreLossPerKg",
                schema: "app",
                table: "RawCoils",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ThicknessMm",
                schema: "app",
                table: "RawCoils",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Category",
                schema: "app",
                table: "Grades",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "CoreLossPerKg",
                schema: "app",
                table: "Grades",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ThicknessMm",
                schema: "app",
                table: "Grades",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.Sql(
                """
                UPDATE app.Grades
                SET
                    ThicknessMm = CASE
                        WHEN Code LIKE '23%' THEN 0.23
                        WHEN Code LIKE '27%' THEN 0.27
                        WHEN Code LIKE '30%' THEN 0.30
                        WHEN Code LIKE '35%' THEN 0.35
                        ELSE 0.23
                    END,
                    Category = CASE
                        WHEN Code LIKE '23%' THEN 'M3'
                        WHEN Code LIKE '27%' THEN 'M4'
                        WHEN Code LIKE '30%' THEN 'M5'
                        WHEN Code LIKE '35%' THEN 'M6'
                        ELSE 'M3'
                    END,
                    CoreLossPerKg = CASE
                        WHEN Code LIKE '%85%' THEN 0.85
                        WHEN Code LIKE '%90%' THEN 0.90
                        WHEN Code LIKE '%95%' THEN 0.95
                        ELSE 0.85
                    END;
                """);

            migrationBuilder.Sql(
                """
                UPDATE rawCoils
                SET
                    ThicknessMm = grades.ThicknessMm,
                    Category = grades.Category,
                    CoreLossPerKg = grades.CoreLossPerKg
                FROM app.RawCoils rawCoils
                INNER JOIN app.Grades grades ON grades.Id = rawCoils.GradeId;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                schema: "app",
                table: "RawCoils");

            migrationBuilder.DropColumn(
                name: "CoreLossPerKg",
                schema: "app",
                table: "RawCoils");

            migrationBuilder.DropColumn(
                name: "ThicknessMm",
                schema: "app",
                table: "RawCoils");

            migrationBuilder.DropColumn(
                name: "Category",
                schema: "app",
                table: "Grades");

            migrationBuilder.DropColumn(
                name: "CoreLossPerKg",
                schema: "app",
                table: "Grades");

            migrationBuilder.DropColumn(
                name: "ThicknessMm",
                schema: "app",
                table: "Grades");
        }
    }
}
