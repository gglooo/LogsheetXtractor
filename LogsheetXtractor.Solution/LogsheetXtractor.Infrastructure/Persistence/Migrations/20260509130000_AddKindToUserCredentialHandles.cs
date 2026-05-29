using LogsheetXtractor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogsheetXtractor.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20260509130000_AddKindToUserCredentialHandles")]
    public partial class AddKindToUserCredentialHandles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Kind",
                table: "UserCredentialHandles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1
            );

            migrationBuilder.CreateIndex(
                name: "IX_UserCredentialHandles_Kind_ExpiresAtUtc",
                table: "UserCredentialHandles",
                columns: new[] { "Kind", "ExpiresAtUtc" }
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                CREATE TABLE "UserCredentialHandles_Old" (
                    "Handle" TEXT NOT NULL CONSTRAINT "PK_UserCredentialHandles" PRIMARY KEY,
                    "ProtectedPayload" TEXT NOT NULL,
                    "IssuedAtUtc" TEXT NOT NULL,
                    "ExpiresAtUtc" TEXT NOT NULL
                );
                """
            );

            migrationBuilder.Sql(
                """
                INSERT INTO "UserCredentialHandles_Old" (
                    "Handle",
                    "ProtectedPayload",
                    "IssuedAtUtc",
                    "ExpiresAtUtc"
                )
                SELECT
                    "Handle",
                    "ProtectedPayload",
                    "IssuedAtUtc",
                    "ExpiresAtUtc"
                FROM "UserCredentialHandles";
                """
            );

            migrationBuilder.DropTable(name: "UserCredentialHandles");

            migrationBuilder.RenameTable(
                name: "UserCredentialHandles_Old",
                newName: "UserCredentialHandles"
            );

            migrationBuilder.CreateIndex(
                name: "IX_UserCredentialHandles_ExpiresAtUtc",
                table: "UserCredentialHandles",
                column: "ExpiresAtUtc"
            );
        }
    }
}
