using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoilManager.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSlittingJobPlanning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SlittingJobs",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SlittingJobNo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PlanningDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PlannerId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MotherCoilId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MachineId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Shift = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    KnifeThickness = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    LeftEdgeTrim = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    RightEdgeTrim = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SlittingJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SlittingJobs_RawCoils_MotherCoilId",
                        column: x => x.MotherCoilId,
                        principalSchema: "app",
                        principalTable: "RawCoils",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SlittingJobItems",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SlittingJobId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SequenceNo = table.Column<int>(type: "int", nullable: false),
                    SlitCoilId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Width = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    EstimatedWeight = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SlittingJobItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SlittingJobItems_SlittingJobs_SlittingJobId",
                        column: x => x.SlittingJobId,
                        principalSchema: "app",
                        principalTable: "SlittingJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SlittingJobItems_SlitCoilId",
                schema: "app",
                table: "SlittingJobItems",
                column: "SlitCoilId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SlittingJobItems_SlittingJobId_SequenceNo",
                schema: "app",
                table: "SlittingJobItems",
                columns: new[] { "SlittingJobId", "SequenceNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SlittingJobs_MotherCoilId",
                schema: "app",
                table: "SlittingJobs",
                column: "MotherCoilId");

            migrationBuilder.CreateIndex(
                name: "IX_SlittingJobs_PlanningDate",
                schema: "app",
                table: "SlittingJobs",
                column: "PlanningDate");

            migrationBuilder.CreateIndex(
                name: "IX_SlittingJobs_SlittingJobNo",
                schema: "app",
                table: "SlittingJobs",
                column: "SlittingJobNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SlittingJobs_Status",
                schema: "app",
                table: "SlittingJobs",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SlittingJobItems",
                schema: "app");

            migrationBuilder.DropTable(
                name: "SlittingJobs",
                schema: "app");
        }
    }
}
