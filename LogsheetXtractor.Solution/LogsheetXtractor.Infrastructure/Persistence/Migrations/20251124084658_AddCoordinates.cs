using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogsheetXtractor.Infrastructure.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCoordinates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(name: "Y", table: "Rois", newName: "Coordinates_Y");

            migrationBuilder.RenameColumn(name: "X", table: "Rois", newName: "Coordinates_X");

            migrationBuilder.RenameColumn(
                name: "Width",
                table: "Rois",
                newName: "Coordinates_Width"
            );

            migrationBuilder.RenameColumn(
                name: "Height",
                table: "Rois",
                newName: "Coordinates_Height"
            );

            migrationBuilder.RenameColumn(name: "Y", table: "Residuals", newName: "Coordinates_Y");

            migrationBuilder.RenameColumn(name: "X", table: "Residuals", newName: "Coordinates_X");

            migrationBuilder.RenameColumn(
                name: "Width",
                table: "Residuals",
                newName: "Coordinates_Width"
            );

            migrationBuilder.RenameColumn(
                name: "Height",
                table: "Residuals",
                newName: "Coordinates_Height"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(name: "Coordinates_Y", table: "Rois", newName: "Y");

            migrationBuilder.RenameColumn(name: "Coordinates_X", table: "Rois", newName: "X");

            migrationBuilder.RenameColumn(
                name: "Coordinates_Width",
                table: "Rois",
                newName: "Width"
            );

            migrationBuilder.RenameColumn(
                name: "Coordinates_Height",
                table: "Rois",
                newName: "Height"
            );

            migrationBuilder.RenameColumn(name: "Coordinates_Y", table: "Residuals", newName: "Y");

            migrationBuilder.RenameColumn(name: "Coordinates_X", table: "Residuals", newName: "X");

            migrationBuilder.RenameColumn(
                name: "Coordinates_Width",
                table: "Residuals",
                newName: "Width"
            );

            migrationBuilder.RenameColumn(
                name: "Coordinates_Height",
                table: "Residuals",
                newName: "Height"
            );
        }
    }
}
