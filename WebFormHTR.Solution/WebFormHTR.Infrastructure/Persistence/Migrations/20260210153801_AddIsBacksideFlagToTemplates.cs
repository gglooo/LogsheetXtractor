using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebFormHTR.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddIsBacksideFlagToTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsBackside",
                table: "Templates",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsBackside",
                table: "Templates");
        }
    }
}
