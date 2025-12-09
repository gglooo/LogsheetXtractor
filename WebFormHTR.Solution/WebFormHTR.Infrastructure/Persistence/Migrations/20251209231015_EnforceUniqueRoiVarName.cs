using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebFormHTR.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EnforceUniqueRoiVarName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Rois_TemplateId",
                table: "Rois");

            migrationBuilder.AlterColumn<float>(
                name: "Width",
                table: "Templates",
                type: "REAL",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "REAL");

            migrationBuilder.AlterColumn<float>(
                name: "Height",
                table: "Templates",
                type: "REAL",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "REAL");

            migrationBuilder.CreateIndex(
                name: "IX_Rois_TemplateId_VariableName",
                table: "Rois",
                columns: new[] { "TemplateId", "VariableName" },
                unique: true,
                filter: "[DeletedAt] IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Rois_TemplateId_VariableName",
                table: "Rois");

            migrationBuilder.AlterColumn<float>(
                name: "Width",
                table: "Templates",
                type: "REAL",
                nullable: false,
                defaultValue: 0f,
                oldClrType: typeof(float),
                oldType: "REAL",
                oldNullable: true);

            migrationBuilder.AlterColumn<float>(
                name: "Height",
                table: "Templates",
                type: "REAL",
                nullable: false,
                defaultValue: 0f,
                oldClrType: typeof(float),
                oldType: "REAL",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rois_TemplateId",
                table: "Rois",
                column: "TemplateId");
        }
    }
}
