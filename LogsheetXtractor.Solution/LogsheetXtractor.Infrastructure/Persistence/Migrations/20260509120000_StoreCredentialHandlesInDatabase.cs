using System;
using LogsheetXtractor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogsheetXtractor.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20260509120000_StoreCredentialHandlesInDatabase")]
    public partial class StoreCredentialHandlesInDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserCredentialHandles",
                columns: table => new
                {
                    Handle = table.Column<string>(
                        type: "TEXT",
                        maxLength: 32,
                        nullable: false
                    ),
                    ProtectedPayload = table.Column<string>(type: "TEXT", nullable: false),
                    IssuedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCredentialHandles", x => x.Handle);
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_UserCredentialHandles_ExpiresAtUtc",
                table: "UserCredentialHandles",
                column: "ExpiresAtUtc"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "UserCredentialHandles");
        }
    }
}
