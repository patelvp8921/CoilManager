using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoilManager.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class BackfillReleasedSlittingJobsReservedState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE sj
                SET
                    ReleasedOn = COALESCE(sj.ReleasedOn, sj.CreatedAtUtc),
                    ReleasedBy = COALESCE(sj.ReleasedBy, sj.CreatedBy, N'System')
                FROM [app].[SlittingJobs] sj
                WHERE sj.[Status] = N'Released';
                """);

            migrationBuilder.Sql("""
                UPDATE r
                SET [Status] = N'Reserved'
                FROM [app].[RawCoils] r
                WHERE r.[Status] = N'Available'
                  AND EXISTS (
                      SELECT 1
                      FROM [app].[SlittingJobs] sj
                      WHERE sj.MotherCoilId = r.Id
                        AND sj.[Status] = N'Released'
                  );
                """);

            migrationBuilder.Sql("""
                UPDATE item
                SET [Status] = N'Released'
                FROM [app].[SlittingJobItems] item
                INNER JOIN [app].[SlittingJobs] sj ON sj.Id = item.SlittingJobId
                WHERE sj.[Status] = N'Released'
                  AND item.[Status] = N'Draft';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Data backfill is intentionally not reversed.
        }
    }
}
