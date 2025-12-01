using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebFormHTR.Infrastructure.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBacksideTemplateToLogsheet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BacksideTemplateId",
                table: "Logsheets",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Logsheets_BacksideTemplateId",
                table: "Logsheets",
                column: "BacksideTemplateId");

            migrationBuilder.AddForeignKey(
                name: "FK_Logsheets_Templates_BacksideTemplateId",
                table: "Logsheets",
                column: "BacksideTemplateId",
                principalTable: "Templates",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Logsheets_Templates_BacksideTemplateId",
                table: "Logsheets");

            migrationBuilder.DropIndex(
                name: "IX_Logsheets_BacksideTemplateId",
                table: "Logsheets");

            migrationBuilder.DropColumn(
                name: "BacksideTemplateId",
                table: "Logsheets");
        }
    }
}
