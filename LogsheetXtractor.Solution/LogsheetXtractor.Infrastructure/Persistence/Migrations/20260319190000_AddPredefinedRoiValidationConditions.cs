using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogsheetXtractor.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPredefinedRoiValidationConditions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PredefinedRoiValidationConditions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Label = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    RoiType = table.Column<int>(type: "INTEGER", nullable: false),
                    Condition = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PredefinedRoiValidationConditions", x => x.Id);
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_PredefinedRoiValidationConditions_Code_RoiType",
                table: "PredefinedRoiValidationConditions",
                columns: new[] { "Code", "RoiType" },
                unique: true
            );

            var createdAt = new DateTime(2026, 3, 19, 0, 0, 0, DateTimeKind.Utc);

            migrationBuilder.InsertData(
                table: "PredefinedRoiValidationConditions",
                columns: new[]
                {
                    "Id",
                    "Code",
                    "Label",
                    "RoiType",
                    "Condition",
                    "CreatedAt",
                    "UpdatedAt",
                    "DeletedAt",
                },
                values: new object[,]
                {
                    {
                        new Guid("2f9057d8-6fcf-4f3e-9b94-f422f05414b4"),
                        "year",
                        "Year",
                        1,
                        "{\"type\":\"group\",\"operator\":\"AND\",\"children\":[{\"type\":\"rule\",\"ruleType\":\"common.requiredNonEmpty\",\"params\":{}},{\"type\":\"rule\",\"ruleType\":\"number.integerOnly\",\"params\":{}},{\"type\":\"rule\",\"ruleType\":\"number.range\",\"params\":{\"min\":1900,\"max\":2100,\"inclusiveMin\":true,\"inclusiveMax\":true}}]}",
                        createdAt,
                        null,
                        null,
                    },
                    {
                        new Guid("7bb61889-b393-442c-b796-c5ef93dc5cd6"),
                        "month",
                        "Month",
                        1,
                        "{\"type\":\"group\",\"operator\":\"AND\",\"children\":[{\"type\":\"rule\",\"ruleType\":\"common.requiredNonEmpty\",\"params\":{}},{\"type\":\"rule\",\"ruleType\":\"number.integerOnly\",\"params\":{}},{\"type\":\"rule\",\"ruleType\":\"number.range\",\"params\":{\"min\":1,\"max\":12,\"inclusiveMin\":true,\"inclusiveMax\":true}}]}",
                        createdAt,
                        null,
                        null,
                    },
                    {
                        new Guid("f3f4f94d-5449-45e0-98d1-368a5f27680a"),
                        "day",
                        "Day",
                        1,
                        "{\"type\":\"group\",\"operator\":\"AND\",\"children\":[{\"type\":\"rule\",\"ruleType\":\"common.requiredNonEmpty\",\"params\":{}},{\"type\":\"rule\",\"ruleType\":\"number.integerOnly\",\"params\":{}},{\"type\":\"rule\",\"ruleType\":\"number.range\",\"params\":{\"min\":1,\"max\":31,\"inclusiveMin\":true,\"inclusiveMax\":true}}]}",
                        createdAt,
                        null,
                        null,
                    },
                    {
                        new Guid("af95f89f-2fd3-42a8-befe-0f7935555815"),
                        "latitudeHemisphere",
                        "Latitude hemisphere (N/S)",
                        0,
                        "{\"type\":\"group\",\"operator\":\"AND\",\"children\":[{\"type\":\"rule\",\"ruleType\":\"common.requiredNonEmpty\",\"params\":{}},{\"type\":\"rule\",\"ruleType\":\"text.regex\",\"params\":{\"pattern\":\"^[NnSs]$\"}}]}",
                        createdAt,
                        null,
                        null,
                    },
                    {
                        new Guid("2c7f95de-81c3-4d5f-9e2f-7d25f0f489f4"),
                        "latitudeDegrees",
                        "Latitude degrees (DD)",
                        1,
                        "{\"type\":\"group\",\"operator\":\"AND\",\"children\":[{\"type\":\"rule\",\"ruleType\":\"common.requiredNonEmpty\",\"params\":{}},{\"type\":\"rule\",\"ruleType\":\"number.integerOnly\",\"params\":{}},{\"type\":\"rule\",\"ruleType\":\"number.range\",\"params\":{\"min\":0,\"max\":90,\"inclusiveMin\":true,\"inclusiveMax\":true}}]}",
                        createdAt,
                        null,
                        null,
                    },
                    {
                        new Guid("2af0d8cd-22eb-41de-a2c9-97677d08ce75"),
                        "latitudeMinutes",
                        "Latitude minutes (MM.MMM)",
                        1,
                        "{\"type\":\"group\",\"operator\":\"AND\",\"children\":[{\"type\":\"rule\",\"ruleType\":\"common.requiredNonEmpty\",\"params\":{}},{\"type\":\"rule\",\"ruleType\":\"number.range\",\"params\":{\"min\":0,\"max\":59.999,\"inclusiveMin\":true,\"inclusiveMax\":true}},{\"type\":\"rule\",\"ruleType\":\"number.decimalScaleMax\",\"params\":{\"max\":3}}]}",
                        createdAt,
                        null,
                        null,
                    },
                    {
                        new Guid("f8f48c5b-dd37-4e9a-a0c1-617424f357e7"),
                        "hours",
                        "Hours",
                        1,
                        "{\"type\":\"group\",\"operator\":\"AND\",\"children\":[{\"type\":\"rule\",\"ruleType\":\"common.requiredNonEmpty\",\"params\":{}},{\"type\":\"rule\",\"ruleType\":\"number.integerOnly\",\"params\":{}},{\"type\":\"rule\",\"ruleType\":\"number.range\",\"params\":{\"min\":0,\"max\":23,\"inclusiveMin\":true,\"inclusiveMax\":true}}]}",
                        createdAt,
                        null,
                        null,
                    },
                    {
                        new Guid("ef2e4194-38ab-4fff-9cc6-d9a17e6ca6c4"),
                        "minutes",
                        "Minutes",
                        1,
                        "{\"type\":\"group\",\"operator\":\"AND\",\"children\":[{\"type\":\"rule\",\"ruleType\":\"common.requiredNonEmpty\",\"params\":{}},{\"type\":\"rule\",\"ruleType\":\"number.integerOnly\",\"params\":{}},{\"type\":\"rule\",\"ruleType\":\"number.range\",\"params\":{\"min\":0,\"max\":59,\"inclusiveMin\":true,\"inclusiveMax\":true}}]}",
                        createdAt,
                        null,
                        null,
                    },
                    {
                        new Guid("6ad27c31-cbee-46b2-9c63-3dfa52ccce77"),
                        "longitudeHemisphere",
                        "Longitude hemisphere (E/W)",
                        0,
                        "{\"type\":\"group\",\"operator\":\"AND\",\"children\":[{\"type\":\"rule\",\"ruleType\":\"common.requiredNonEmpty\",\"params\":{}},{\"type\":\"rule\",\"ruleType\":\"text.regex\",\"params\":{\"pattern\":\"^[EeWw]$\"}}]}",
                        createdAt,
                        null,
                        null,
                    },
                    {
                        new Guid("5b6a5d6f-8f2e-44ce-8fb4-cb2d0a6f0fc1"),
                        "longitudeDegrees",
                        "Longitude degrees (DDD)",
                        1,
                        "{\"type\":\"group\",\"operator\":\"AND\",\"children\":[{\"type\":\"rule\",\"ruleType\":\"common.requiredNonEmpty\",\"params\":{}},{\"type\":\"rule\",\"ruleType\":\"number.integerOnly\",\"params\":{}},{\"type\":\"rule\",\"ruleType\":\"number.range\",\"params\":{\"min\":0,\"max\":180,\"inclusiveMin\":true,\"inclusiveMax\":true}}]}",
                        createdAt,
                        null,
                        null,
                    },
                    {
                        new Guid("655f0cc2-4bc9-49a4-ad4a-fad8028bb0a0"),
                        "longitudeMinutes",
                        "Longitude minutes (MM.MMM)",
                        1,
                        "{\"type\":\"group\",\"operator\":\"AND\",\"children\":[{\"type\":\"rule\",\"ruleType\":\"common.requiredNonEmpty\",\"params\":{}},{\"type\":\"rule\",\"ruleType\":\"number.range\",\"params\":{\"min\":0,\"max\":59.999,\"inclusiveMin\":true,\"inclusiveMax\":true}},{\"type\":\"rule\",\"ruleType\":\"number.decimalScaleMax\",\"params\":{\"max\":3}}]}",
                        createdAt,
                        null,
                        null,
                    },
                    {
                        new Guid("d9d86f38-c898-4757-9087-b9f28f9e5159"),
                        "waterTemperatureC",
                        "Water temperature C",
                        1,
                        "{\"type\":\"group\",\"operator\":\"AND\",\"children\":[{\"type\":\"rule\",\"ruleType\":\"common.requiredNonEmpty\",\"params\":{}},{\"type\":\"rule\",\"ruleType\":\"number.range\",\"params\":{\"min\":-2,\"max\":40,\"inclusiveMin\":true,\"inclusiveMax\":true}},{\"type\":\"rule\",\"ruleType\":\"number.decimalScaleMax\",\"params\":{\"max\":2}}]}",
                        createdAt,
                        null,
                        null,
                    },
                    {
                        new Guid("8f1f5de8-c57a-483c-b67e-84a1f1f98022"),
                        "ph",
                        "pH",
                        1,
                        "{\"type\":\"group\",\"operator\":\"AND\",\"children\":[{\"type\":\"rule\",\"ruleType\":\"common.requiredNonEmpty\",\"params\":{}},{\"type\":\"rule\",\"ruleType\":\"number.range\",\"params\":{\"min\":6.5,\"max\":9.5,\"inclusiveMin\":true,\"inclusiveMax\":true}},{\"type\":\"rule\",\"ruleType\":\"number.decimalScaleMax\",\"params\":{\"max\":2}}]}",
                        createdAt,
                        null,
                        null,
                    },
                    {
                        new Guid("6c02c4c8-b8e1-46e1-88ef-0087a2352eca"),
                        "dissolvedOxygenMgL",
                        "Dissolved oxygen mg/L",
                        1,
                        "{\"type\":\"group\",\"operator\":\"AND\",\"children\":[{\"type\":\"rule\",\"ruleType\":\"common.requiredNonEmpty\",\"params\":{}},{\"type\":\"rule\",\"ruleType\":\"number.range\",\"params\":{\"min\":0,\"max\":20,\"inclusiveMin\":true,\"inclusiveMax\":true}},{\"type\":\"rule\",\"ruleType\":\"number.decimalScaleMax\",\"params\":{\"max\":2}}]}",
                        createdAt,
                        null,
                        null,
                    },
                    {
                        new Guid("0e30d20b-c756-4896-b584-32d81bc64428"),
                        "windSpeedMs",
                        "Wind speed m/s",
                        1,
                        "{\"type\":\"group\",\"operator\":\"AND\",\"children\":[{\"type\":\"rule\",\"ruleType\":\"common.requiredNonEmpty\",\"params\":{}},{\"type\":\"rule\",\"ruleType\":\"number.range\",\"params\":{\"min\":0,\"max\":80,\"inclusiveMin\":true,\"inclusiveMax\":true}},{\"type\":\"rule\",\"ruleType\":\"number.decimalScaleMax\",\"params\":{\"max\":2}}]}",
                        createdAt,
                        null,
                        null,
                    },
                    {
                        new Guid("b3406ec9-1eb0-43b2-a838-e99f025c9f35"),
                        "windDirectionDeg",
                        "Wind direction deg",
                        1,
                        "{\"type\":\"group\",\"operator\":\"AND\",\"children\":[{\"type\":\"rule\",\"ruleType\":\"common.requiredNonEmpty\",\"params\":{}},{\"type\":\"rule\",\"ruleType\":\"number.integerOnly\",\"params\":{}},{\"type\":\"rule\",\"ruleType\":\"number.range\",\"params\":{\"min\":0,\"max\":360,\"inclusiveMin\":true,\"inclusiveMax\":true}}]}",
                        createdAt,
                        null,
                        null,
                    },
                }
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "PredefinedRoiValidationConditions");
        }
    }
}
