using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoilManager.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminMasterDataManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "app",
                table: "Suppliers",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                schema: "app",
                table: "Suppliers",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "app",
                table: "Manufacturers",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                schema: "app",
                table: "Manufacturers",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                schema: "app",
                table: "Grades",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                schema: "app",
                table: "Grades",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("UPDATE app.Grades SET Name = Code WHERE Name = '' OR Name IS NULL;");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                schema: "app",
                table: "Grades",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_Name",
                schema: "app",
                table: "Suppliers",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Manufacturers_Name",
                schema: "app",
                table: "Manufacturers",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Grades_Name",
                schema: "app",
                table: "Grades",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Suppliers_Name",
                schema: "app",
                table: "Suppliers");

            migrationBuilder.DropIndex(
                name: "IX_Manufacturers_Name",
                schema: "app",
                table: "Manufacturers");

            migrationBuilder.DropIndex(
                name: "IX_Grades_Name",
                schema: "app",
                table: "Grades");

            migrationBuilder.DropColumn(
                name: "Description",
                schema: "app",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                schema: "app",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "Description",
                schema: "app",
                table: "Manufacturers");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                schema: "app",
                table: "Manufacturers");

            migrationBuilder.DropColumn(
                name: "Name",
                schema: "app",
                table: "Grades");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                schema: "app",
                table: "Grades");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                schema: "app",
                table: "Grades",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250,
                oldNullable: true);
        }
    }
}
