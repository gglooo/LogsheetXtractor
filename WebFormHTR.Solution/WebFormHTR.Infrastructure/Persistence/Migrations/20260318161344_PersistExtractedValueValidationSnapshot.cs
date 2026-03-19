using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebFormHTR.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PersistExtractedValueValidationSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ValidationRulesVersion",
                table: "ExtractedValues",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ValidationWarnings",
                table: "ExtractedValues",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ValidationRulesVersion",
                table: "ExtractedValues");

            migrationBuilder.DropColumn(
                name: "ValidationWarnings",
                table: "ExtractedValues");
        }
    }
}
