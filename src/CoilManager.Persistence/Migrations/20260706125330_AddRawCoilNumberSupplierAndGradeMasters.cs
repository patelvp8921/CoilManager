using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoilManager.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRawCoilNumberSupplierAndGradeMasters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CoilID",
                schema: "app",
                table: "RawCoils",
                newName: "RawCoilNumber");

            migrationBuilder.RenameIndex(
                name: "IX_RawCoils_CoilID",
                schema: "app",
                table: "RawCoils",
                newName: "IX_RawCoils_RawCoilNumber");

            migrationBuilder.CreateTable(
                name: "Grades",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Grades", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Suppliers",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suppliers", x => x.Id);
                });

            migrationBuilder.InsertData(
                schema: "app",
                table: "Grades",
                columns: ["Id", "Code", "Description", "IsActive", "CreatedAtUtc"],
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "23HP85", "23HP85", true, new DateTimeOffset(new DateTime(2026, 7, 6, 0, 0, 0, DateTimeKind.Utc)) },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "23HP90", "23HP90", true, new DateTimeOffset(new DateTime(2026, 7, 6, 0, 0, 0, DateTimeKind.Utc)) },
                    { new Guid("33333333-3333-3333-3333-333333333333"), "23HP95", "23HP95", true, new DateTimeOffset(new DateTime(2026, 7, 6, 0, 0, 0, DateTimeKind.Utc)) }
                });

            migrationBuilder.InsertData(
                schema: "app",
                table: "Suppliers",
                columns: ["Id", "Name", "Code", "IsActive", "CreatedAtUtc"],
                values: new object[,]
                {
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1"), "Prime Steel Suppliers", "PSS", true, new DateTimeOffset(new DateTime(2026, 7, 6, 0, 0, 0, DateTimeKind.Utc)) },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2"), "National Coil Traders", "NCT", true, new DateTimeOffset(new DateTime(2026, 7, 6, 0, 0, 0, DateTimeKind.Utc)) },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3"), "Apex Metals", "APX", true, new DateTimeOffset(new DateTime(2026, 7, 6, 0, 0, 0, DateTimeKind.Utc)) }
                });

            migrationBuilder.Sql("""
                INSERT INTO app.Suppliers (Id, Name, Code, IsActive, CreatedAtUtc)
                SELECT NEWID(), SupplierName, CONCAT('SUP', FORMAT(ROW_NUMBER() OVER (ORDER BY SupplierName), '0000')), 1, SYSDATETIMEOFFSET()
                FROM (
                    SELECT DISTINCT SupplierName
                    FROM app.RawCoils rawCoil
                    WHERE SupplierName IS NOT NULL
                      AND LTRIM(RTRIM(SupplierName)) <> ''
                      AND NOT EXISTS (
                          SELECT 1
                          FROM app.Suppliers
                          WHERE Name = rawCoil.SupplierName
                      )
                ) suppliers;
                """);

            migrationBuilder.Sql("""
                INSERT INTO app.Grades (Id, Code, Description, IsActive, CreatedAtUtc)
                SELECT NEWID(), Grade, Grade, 1, SYSDATETIMEOFFSET()
                FROM (
                    SELECT DISTINCT Grade
                    FROM app.RawCoils rawCoil
                    WHERE Grade IS NOT NULL
                      AND LTRIM(RTRIM(Grade)) <> ''
                      AND NOT EXISTS (
                          SELECT 1
                          FROM app.Grades
                          WHERE Code = rawCoil.Grade
                      )
                ) grades;
                """);

            migrationBuilder.AddColumn<Guid>(
                name: "GradeId",
                schema: "app",
                table: "RawCoils",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SupplierId",
                schema: "app",
                table: "RawCoils",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE rawCoil
                SET SupplierId = supplier.Id
                FROM app.RawCoils rawCoil
                INNER JOIN app.Suppliers supplier ON supplier.Name = rawCoil.SupplierName;

                UPDATE rawCoil
                SET GradeId = grade.Id
                FROM app.RawCoils rawCoil
                INNER JOIN app.Grades grade ON grade.Code = rawCoil.Grade;

                UPDATE app.RawCoils
                SET SupplierId = 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1'
                WHERE SupplierId IS NULL;

                UPDATE app.RawCoils
                SET GradeId = '11111111-1111-1111-1111-111111111111'
                WHERE GradeId IS NULL;

                WITH Numbered AS (
                    SELECT Id,
                           CONCAT(
                               'MC-',
                               YEAR(ReceivedDate),
                               '-',
                               RIGHT(CONCAT('0000000', ROW_NUMBER() OVER (PARTITION BY YEAR(ReceivedDate) ORDER BY CreatedAtUtc, Id)), 7)
                           ) AS NextRawCoilNumber
                    FROM app.RawCoils
                )
                UPDATE rawCoil
                SET RawCoilNumber = numbered.NextRawCoilNumber
                FROM app.RawCoils rawCoil
                INNER JOIN Numbered numbered ON numbered.Id = rawCoil.Id;
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "GradeId",
                schema: "app",
                table: "RawCoils",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "SupplierId",
                schema: "app",
                table: "RawCoils",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RawCoils_GradeId",
                schema: "app",
                table: "RawCoils",
                column: "GradeId");

            migrationBuilder.CreateIndex(
                name: "IX_RawCoils_SupplierId",
                schema: "app",
                table: "RawCoils",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_Grades_Code",
                schema: "app",
                table: "Grades",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_Code",
                schema: "app",
                table: "Suppliers",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RawCoils_Grades_GradeId",
                schema: "app",
                table: "RawCoils",
                column: "GradeId",
                principalSchema: "app",
                principalTable: "Grades",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RawCoils_Suppliers_SupplierId",
                schema: "app",
                table: "RawCoils",
                column: "SupplierId",
                principalSchema: "app",
                principalTable: "Suppliers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.DropColumn(
                name: "Grade",
                schema: "app",
                table: "RawCoils");

            migrationBuilder.DropColumn(
                name: "SupplierName",
                schema: "app",
                table: "RawCoils");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RawCoils_Grades_GradeId",
                schema: "app",
                table: "RawCoils");

            migrationBuilder.DropForeignKey(
                name: "FK_RawCoils_Suppliers_SupplierId",
                schema: "app",
                table: "RawCoils");

            migrationBuilder.DropTable(
                name: "Grades",
                schema: "app");

            migrationBuilder.DropTable(
                name: "Suppliers",
                schema: "app");

            migrationBuilder.DropIndex(
                name: "IX_RawCoils_GradeId",
                schema: "app",
                table: "RawCoils");

            migrationBuilder.DropIndex(
                name: "IX_RawCoils_SupplierId",
                schema: "app",
                table: "RawCoils");

            migrationBuilder.DropColumn(
                name: "GradeId",
                schema: "app",
                table: "RawCoils");

            migrationBuilder.DropColumn(
                name: "SupplierId",
                schema: "app",
                table: "RawCoils");

            migrationBuilder.RenameColumn(
                name: "RawCoilNumber",
                schema: "app",
                table: "RawCoils",
                newName: "CoilID");

            migrationBuilder.RenameIndex(
                name: "IX_RawCoils_RawCoilNumber",
                schema: "app",
                table: "RawCoils",
                newName: "IX_RawCoils_CoilID");

            migrationBuilder.AddColumn<string>(
                name: "Grade",
                schema: "app",
                table: "RawCoils",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SupplierName",
                schema: "app",
                table: "RawCoils",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");
        }
    }
}
