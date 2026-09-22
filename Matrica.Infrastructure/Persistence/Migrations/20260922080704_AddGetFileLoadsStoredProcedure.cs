using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Metrica.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddGetFileLoadsStoredProcedure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
        CREATE PROCEDURE dbo.sp_GetFileLoads
            @PageNumber INT,
            @PageSize INT
        AS
        BEGIN
            SET NOCOUNT ON;

            SELECT
                Id,
                FileName,
                UserEmail,
                FilePath,
                Period,
                Status,
                CreatedAt,
                FinishedAt
            FROM FileLoads
            ORDER BY CreatedAt DESC, Id DESC
            OFFSET (@PageNumber - 1) * @PageSize ROWS
            FETCH NEXT @PageSize ROWS ONLY;
        END;
        """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
        DROP PROCEDURE IF EXISTS dbo.sp_GetFileLoads;
        """);
        }
    }
}
