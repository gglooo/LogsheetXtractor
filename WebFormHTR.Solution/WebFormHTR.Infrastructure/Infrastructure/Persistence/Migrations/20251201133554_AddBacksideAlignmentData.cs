using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebFormHTR.Infrastructure.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBacksideAlignmentData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AlignmentData",
                table: "Logsheets",
                newName: "FrontAlignmentData");

            migrationBuilder.AddColumn<string>(
                name: "BackAlignmentData",
                table: "Logsheets",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BackAlignmentData",
                table: "Logsheets");

            migrationBuilder.RenameColumn(
                name: "FrontAlignmentData",
                table: "Logsheets",
                newName: "AlignmentData");
        }
    }
}
