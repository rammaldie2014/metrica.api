using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Metrica.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFileLoadErrors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FileLoadErrors",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileLoadId = table.Column<long>(type: "bigint", nullable: false),
                    RowNumber = table.Column<int>(type: "int", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileLoadErrors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FileLoadErrors_FileLoads_FileLoadId",
                        column: x => x.FileLoadId,
                        principalTable: "FileLoads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FileLoadErrors_FileLoadId_CreatedAt",
                table: "FileLoadErrors",
                columns: new[] { "FileLoadId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FileLoadErrors");
        }
    }
}
