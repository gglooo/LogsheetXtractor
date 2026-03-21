using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogsheetXtractor.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBacksideLogsheets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Logsheets_Templates_TemplateId1",
                table: "Logsheets"
            );

            migrationBuilder.DropIndex(name: "IX_Logsheets_TemplateId1", table: "Logsheets");

            migrationBuilder.DropColumn(name: "TemplateId1", table: "Logsheets");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TemplateId1",
                table: "Logsheets",
                type: "TEXT",
                nullable: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_Logsheets_TemplateId1",
                table: "Logsheets",
                column: "TemplateId1"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Logsheets_Templates_TemplateId1",
                table: "Logsheets",
                column: "TemplateId1",
                principalTable: "Templates",
                principalColumn: "Id"
            );
        }
    }
}
