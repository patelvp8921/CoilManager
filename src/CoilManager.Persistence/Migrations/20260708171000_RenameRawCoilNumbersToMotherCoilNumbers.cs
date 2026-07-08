using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace CoilManager.Persistence.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260708171000_RenameRawCoilNumbersToMotherCoilNumbers")]
    public partial class RenameRawCoilNumbersToMotherCoilNumbers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE app.RawCoils
                SET RawCoilNumber = CONCAT('MC-', SUBSTRING(RawCoilNumber, 4, LEN(RawCoilNumber)))
                WHERE RawCoilNumber LIKE 'RC-%';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE app.RawCoils
                SET RawCoilNumber = CONCAT('RC-', SUBSTRING(RawCoilNumber, 4, LEN(RawCoilNumber)))
                WHERE RawCoilNumber LIKE 'MC-%';
                """);
        }
    }
}
