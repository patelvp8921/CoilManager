using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoilManager.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLaminationJobPlanningAndAllocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LaminationJobs",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LaminationJobNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DrawingNumber = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    DrawingRevision = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OemJobNumber = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    TransformerCompany = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Rating = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DesignType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StepLapOrientation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NumberOfSteps = table.Column<int>(type: "int", nullable: false),
                    GradeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Thickness = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CoreLossPerKg = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    WorkOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    WorkOrderNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PlannedDate = table.Column<DateOnly>(type: "date", nullable: false),
                    RequiredDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Machine = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Shift = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PlannerName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TotalPlannedPieces = table.Column<int>(type: "int", nullable: false),
                    TotalPlannedWeight = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    TotalAllocatedWeight = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DrawingAttachmentName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DrawingAttachmentPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReleasedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReleasedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    AllocatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AllocatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CancelledBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CancelledOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LaminationJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LaminationJobs_Grades_GradeId",
                        column: x => x.GradeId,
                        principalSchema: "app",
                        principalTable: "Grades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LaminationJobMaterialAllocations",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LaminationJobId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SlitCoilId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SlitCoilNumber = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    RequiredWidth = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    AllocatedWeight = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    IssuedWeight = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    ConsumedWeight = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    RemainingWeightAfterAllocation = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReservedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReservedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ReleasedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReleasedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LaminationJobMaterialAllocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LaminationJobMaterialAllocations_LaminationJobs_LaminationJobId",
                        column: x => x.LaminationJobId,
                        principalSchema: "app",
                        principalTable: "LaminationJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LaminationJobSteps",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LaminationJobId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StepNumber = table.Column<int>(type: "int", nullable: false),
                    StackQuantity = table.Column<int>(type: "int", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    Width = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    PlannedWeight = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LaminationJobSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LaminationJobSteps_LaminationJobs_LaminationJobId",
                        column: x => x.LaminationJobId,
                        principalSchema: "app",
                        principalTable: "LaminationJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LaminationJobPlates",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LaminationJobStepId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlateType = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Width = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    Length = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    PlannedWeight = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LaminationJobPlates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LaminationJobPlates_LaminationJobSteps_LaminationJobStepId",
                        column: x => x.LaminationJobStepId,
                        principalSchema: "app",
                        principalTable: "LaminationJobSteps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LaminationPlateDimensions",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LaminationJobPlateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DimensionCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DimensionValue = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "mm"),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LaminationPlateDimensions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LaminationPlateDimensions_LaminationJobPlates_LaminationJobPlateId",
                        column: x => x.LaminationJobPlateId,
                        principalSchema: "app",
                        principalTable: "LaminationJobPlates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LaminationJobMaterialAllocations_LaminationJobId",
                schema: "app",
                table: "LaminationJobMaterialAllocations",
                column: "LaminationJobId");

            migrationBuilder.CreateIndex(
                name: "IX_LaminationJobMaterialAllocations_SlitCoilId",
                schema: "app",
                table: "LaminationJobMaterialAllocations",
                column: "SlitCoilId");

            migrationBuilder.CreateIndex(
                name: "IX_LaminationJobPlates_LaminationJobStepId",
                schema: "app",
                table: "LaminationJobPlates",
                column: "LaminationJobStepId");

            migrationBuilder.CreateIndex(
                name: "IX_LaminationJobPlates_LaminationJobStepId_PlateType",
                schema: "app",
                table: "LaminationJobPlates",
                columns: new[] { "LaminationJobStepId", "PlateType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LaminationJobs_DrawingNumber",
                schema: "app",
                table: "LaminationJobs",
                column: "DrawingNumber");

            migrationBuilder.CreateIndex(
                name: "IX_LaminationJobs_GradeId",
                schema: "app",
                table: "LaminationJobs",
                column: "GradeId");

            migrationBuilder.CreateIndex(
                name: "IX_LaminationJobs_LaminationJobNumber",
                schema: "app",
                table: "LaminationJobs",
                column: "LaminationJobNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LaminationJobs_OemJobNumber",
                schema: "app",
                table: "LaminationJobs",
                column: "OemJobNumber");

            migrationBuilder.CreateIndex(
                name: "IX_LaminationJobs_PlannedDate",
                schema: "app",
                table: "LaminationJobs",
                column: "PlannedDate");

            migrationBuilder.CreateIndex(
                name: "IX_LaminationJobs_Status",
                schema: "app",
                table: "LaminationJobs",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_LaminationJobSteps_LaminationJobId",
                schema: "app",
                table: "LaminationJobSteps",
                column: "LaminationJobId");

            migrationBuilder.CreateIndex(
                name: "IX_LaminationJobSteps_LaminationJobId_StepNumber",
                schema: "app",
                table: "LaminationJobSteps",
                columns: new[] { "LaminationJobId", "StepNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LaminationPlateDimensions_LaminationJobPlateId_DimensionCode",
                schema: "app",
                table: "LaminationPlateDimensions",
                columns: new[] { "LaminationJobPlateId", "DimensionCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LaminationJobMaterialAllocations",
                schema: "app");

            migrationBuilder.DropTable(
                name: "LaminationPlateDimensions",
                schema: "app");

            migrationBuilder.DropTable(
                name: "LaminationJobPlates",
                schema: "app");

            migrationBuilder.DropTable(
                name: "LaminationJobSteps",
                schema: "app");

            migrationBuilder.DropTable(
                name: "LaminationJobs",
                schema: "app");
        }
    }
}
