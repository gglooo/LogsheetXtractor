using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogsheetXtractor.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixRoiExtractedValueRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rois_ExtractedValues_ExtractedValueId",
                table: "Rois"
            );

            migrationBuilder.DropIndex(name: "IX_Rois_ExtractedValueId", table: "Rois");

            migrationBuilder.DropColumn(name: "ExtractedValueId", table: "Rois");

            migrationBuilder.CreateIndex(
                name: "IX_ExtractedValues_RoiId",
                table: "ExtractedValues",
                column: "RoiId"
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExtractedValues_Rois_RoiId",
                table: "ExtractedValues"
            );

            migrationBuilder.DropIndex(name: "IX_ExtractedValues_RoiId", table: "ExtractedValues");

            migrationBuilder.AddColumn<Guid>(
                name: "ExtractedValueId",
                table: "Rois",
                type: "TEXT",
                nullable: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_Rois_ExtractedValueId",
                table: "Rois",
                column: "ExtractedValueId",
                unique: true
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Rois_ExtractedValues_ExtractedValueId",
                table: "Rois",
                column: "ExtractedValueId",
                principalTable: "ExtractedValues",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );
        }
    }
}
