using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Metrica.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FileLoads",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    UserEmail = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    Period = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FinishedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileLoads", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FileLoadStatusHistories",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileLoadId = table.Column<long>(type: "bigint", nullable: false),
                    PreviousStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    NewStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileLoadStatusHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FileLoadStatusHistories_FileLoads_FileLoadId",
                        column: x => x.FileLoadId,
                        principalTable: "FileLoads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProcessedProducts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileLoadId = table.Column<long>(type: "bigint", nullable: false),
                    Period = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Stock = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessedProducts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcessedProducts_FileLoads_FileLoadId",
                        column: x => x.FileLoadId,
                        principalTable: "FileLoads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FileLoads_Period_Status",
                table: "FileLoads",
                columns: new[] { "Period", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_FileLoadStatusHistories_FileLoadId_CreatedAt",
                table: "FileLoadStatusHistories",
                columns: new[] { "FileLoadId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ProcessedProducts_FileLoadId",
                table: "ProcessedProducts",
                column: "FileLoadId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessedProducts_ProductCode",
                table: "ProcessedProducts",
                column: "ProductCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FileLoadStatusHistories");

            migrationBuilder.DropTable(
                name: "ProcessedProducts");

            migrationBuilder.DropTable(
                name: "FileLoads");
        }
    }
}
