using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogsheetXtractor.Infrastructure.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MakeCoordinatesFloat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<float>(
                name: "Coordinates_Y",
                table: "Rois",
                type: "REAL",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER"
            );

            migrationBuilder.AlterColumn<float>(
                name: "Coordinates_X",
                table: "Rois",
                type: "REAL",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER"
            );

            migrationBuilder.AlterColumn<float>(
                name: "Coordinates_Width",
                table: "Rois",
                type: "REAL",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER"
            );

            migrationBuilder.AlterColumn<float>(
                name: "Coordinates_Height",
                table: "Rois",
                type: "REAL",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER"
            );

            migrationBuilder.AlterColumn<float>(
                name: "Coordinates_Y",
                table: "Residuals",
                type: "REAL",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER"
            );

            migrationBuilder.AlterColumn<float>(
                name: "Coordinates_X",
                table: "Residuals",
                type: "REAL",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER"
            );

            migrationBuilder.AlterColumn<float>(
                name: "Coordinates_Width",
                table: "Residuals",
                type: "REAL",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER"
            );

            migrationBuilder.AlterColumn<float>(
                name: "Coordinates_Height",
                table: "Residuals",
                type: "REAL",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Coordinates_Y",
                table: "Rois",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "REAL"
            );

            migrationBuilder.AlterColumn<int>(
                name: "Coordinates_X",
                table: "Rois",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "REAL"
            );

            migrationBuilder.AlterColumn<int>(
                name: "Coordinates_Width",
                table: "Rois",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "REAL"
            );

            migrationBuilder.AlterColumn<int>(
                name: "Coordinates_Height",
                table: "Rois",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "REAL"
            );

            migrationBuilder.AlterColumn<int>(
                name: "Coordinates_Y",
                table: "Residuals",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "REAL"
            );

            migrationBuilder.AlterColumn<int>(
                name: "Coordinates_X",
                table: "Residuals",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "REAL"
            );

            migrationBuilder.AlterColumn<int>(
                name: "Coordinates_Width",
                table: "Residuals",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "REAL"
            );

            migrationBuilder.AlterColumn<int>(
                name: "Coordinates_Height",
                table: "Residuals",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "REAL"
            );
        }
    }
}
