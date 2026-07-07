using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoilManager.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSupplierDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                schema: "app",
                table: "Suppliers",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactNo",
                schema: "app",
                table: "Suppliers",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                schema: "app",
                table: "Suppliers",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GST",
                schema: "app",
                table: "Suppliers",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                schema: "app",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "ContactNo",
                schema: "app",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "Email",
                schema: "app",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "GST",
                schema: "app",
                table: "Suppliers");
        }
    }
}
