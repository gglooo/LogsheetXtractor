using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogsheetXtractor.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveKindFromUserCredentialHandles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserCredentialHandles_Kind_ExpiresAtUtc",
                table: "UserCredentialHandles");

            migrationBuilder.DropColumn(
                name: "Kind",
                table: "UserCredentialHandles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Kind",
                table: "UserCredentialHandles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_UserCredentialHandles_Kind_ExpiresAtUtc",
                table: "UserCredentialHandles",
                columns: new[] { "Kind", "ExpiresAtUtc" });
        }
    }
}
