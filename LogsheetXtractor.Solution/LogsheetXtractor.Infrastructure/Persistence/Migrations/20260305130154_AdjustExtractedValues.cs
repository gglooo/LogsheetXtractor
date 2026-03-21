using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogsheetXtractor.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AdjustExtractedValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExtractedValues_Rois_RoiId",
                table: "ExtractedValues"
            );

            migrationBuilder.AlterColumn<string>(
                name: "AlignmentData",
                table: "Logsheets",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true
            );

            migrationBuilder.AddForeignKey(
                name: "FK_ExtractedValues_Rois_RoiId",
                table: "ExtractedValues",
                column: "RoiId",
                principalTable: "Rois",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExtractedValues_Rois_RoiId",
                table: "ExtractedValues"
            );

            migrationBuilder.AlterColumn<string>(
                name: "AlignmentData",
                table: "Logsheets",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_ExtractedValues_Rois_RoiId",
                table: "ExtractedValues",
                column: "RoiId",
                principalTable: "Rois",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );
        }
    }
}
