using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebFormHTR.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBacksideTemplateReferenceToTemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Logsheets_Templates_BacksideTemplateId",
                table: "Logsheets");

            migrationBuilder.RenameColumn(
                name: "BacksideTemplateId",
                table: "Logsheets",
                newName: "TemplateId1");

            migrationBuilder.RenameIndex(
                name: "IX_Logsheets_BacksideTemplateId",
                table: "Logsheets",
                newName: "IX_Logsheets_TemplateId1");

            migrationBuilder.AddColumn<Guid>(
                name: "BacksideTemplateId",
                table: "Templates",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Templates_BacksideTemplateId",
                table: "Templates",
                column: "BacksideTemplateId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Logsheets_Templates_TemplateId1",
                table: "Logsheets",
                column: "TemplateId1",
                principalTable: "Templates",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Templates_Templates_BacksideTemplateId",
                table: "Templates",
                column: "BacksideTemplateId",
                principalTable: "Templates",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Logsheets_Templates_TemplateId1",
                table: "Logsheets");

            migrationBuilder.DropForeignKey(
                name: "FK_Templates_Templates_BacksideTemplateId",
                table: "Templates");

            migrationBuilder.DropIndex(
                name: "IX_Templates_BacksideTemplateId",
                table: "Templates");

            migrationBuilder.DropColumn(
                name: "BacksideTemplateId",
                table: "Templates");

            migrationBuilder.RenameColumn(
                name: "TemplateId1",
                table: "Logsheets",
                newName: "BacksideTemplateId");

            migrationBuilder.RenameIndex(
                name: "IX_Logsheets_TemplateId1",
                table: "Logsheets",
                newName: "IX_Logsheets_BacksideTemplateId");

            migrationBuilder.AddForeignKey(
                name: "FK_Logsheets_Templates_BacksideTemplateId",
                table: "Logsheets",
                column: "BacksideTemplateId",
                principalTable: "Templates",
                principalColumn: "Id");
        }
    }
}
