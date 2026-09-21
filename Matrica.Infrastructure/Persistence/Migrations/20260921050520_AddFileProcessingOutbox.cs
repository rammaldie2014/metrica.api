using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Metrica.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFileProcessingOutbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FileProcessingOutboxMessages",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileLoadId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileProcessingOutboxMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FileProcessingOutboxMessages_FileLoads_FileLoadId",
                        column: x => x.FileLoadId,
                        principalTable: "FileLoads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FileProcessingOutboxMessages_CreatedAt_Id",
                table: "FileProcessingOutboxMessages",
                columns: new[] { "CreatedAt", "Id" },
                filter: "[PublishedAt] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FileProcessingOutboxMessages_FileLoadId",
                table: "FileProcessingOutboxMessages",
                column: "FileLoadId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FileProcessingOutboxMessages");
        }
    }
}
