using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogsheetXtractor.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveIsBackside : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "IsBackside", table: "Templates");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsBackside",
                table: "Templates",
                type: "INTEGER",
                nullable: false,
                defaultValue: false
            );
        }
    }
}
