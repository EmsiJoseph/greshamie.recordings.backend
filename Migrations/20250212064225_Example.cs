using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class Example : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "UpdatedAt" },
                values: new object[] { "f5dd7f67-3d93-4f3a-a911-1adb40bbd3df", new DateTime(2025, 2, 12, 6, 42, 24, 739, DateTimeKind.Utc).AddTicks(4088), new DateTime(2025, 2, 12, 6, 42, 24, 739, DateTimeKind.Utc).AddTicks(4090) });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2",
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "UpdatedAt" },
                values: new object[] { "37c77ed3-57e0-4067-9fdf-ab9e995a08c5", new DateTime(2025, 2, 12, 6, 42, 24, 746, DateTimeKind.Utc).AddTicks(5639), new DateTime(2025, 2, 12, 6, 42, 24, 746, DateTimeKind.Utc).AddTicks(5644) });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "Email", "NormalizedEmail", "PasswordHash", "SecurityStamp" },
                values: new object[] { "2c966cbc-1905-424a-90be-99269da8c211", null, null, "AQAAAAIAAYagAAAAEBhkT0sHWFMsOkqlevgIwi9a99eXPRDePmw0+lLpkNY3UvCOxQTSq2gkp9bIRdHNXg==", "2cee6bc6-4268-4d37-b67b-be550ba1a755" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "UpdatedAt" },
                values: new object[] { "4e1cbdf6-4549-4b69-a445-1380b7c43817", new DateTime(2025, 2, 12, 6, 41, 41, 560, DateTimeKind.Utc).AddTicks(1556), new DateTime(2025, 2, 12, 6, 41, 41, 560, DateTimeKind.Utc).AddTicks(1559) });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2",
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "UpdatedAt" },
                values: new object[] { "c3e9813d-9c0d-42a5-b0d8-dc72819fb0ee", new DateTime(2025, 2, 12, 6, 41, 41, 567, DateTimeKind.Utc).AddTicks(6262), new DateTime(2025, 2, 12, 6, 41, 41, 567, DateTimeKind.Utc).AddTicks(6265) });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "Email", "NormalizedEmail", "PasswordHash", "SecurityStamp" },
                values: new object[] { "8f9f0bdd-ca7c-4396-97ec-580d229e2614", "GHIE-API@example.com", "GHIE-API@EXAMPLE.COM", "AQAAAAIAAYagAAAAECo3LyRlhrImfGjxj9RWY2BwAUnnk3hNyTbPhdsODP3NtTebDNDlvOmm/xUnWBJgIw==", "602e9e39-1b9e-47c9-9a54-7909ffe461b4" });
        }
    }
}
