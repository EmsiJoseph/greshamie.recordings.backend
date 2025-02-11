using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class Seeders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "903281ca-54d2-4d9f-8e9e-ef529d0afd4c",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 2, 11, 9, 5, 43, 857, DateTimeKind.Utc).AddTicks(7948), new DateTime(2025, 2, 11, 9, 5, 43, 857, DateTimeKind.Utc).AddTicks(7951) });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b7ee85d4-608f-4671-9a00-71cb05ffc2d4",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 2, 11, 9, 5, 43, 857, DateTimeKind.Utc).AddTicks(8938), new DateTime(2025, 2, 11, 9, 5, 43, 857, DateTimeKind.Utc).AddTicks(8938) });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "ccb2c35c-99a6-4d9c-8ee0-d0aec4a9a48a",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "343ef2d0-716b-403e-acc4-69c76927ddd7", "AQAAAAIAAYagAAAAENb718KHpOMnwT4gtH+5LkmlBbx3abSo+7zYIJxu+cw2Xg8f9URa3JkDkSY4lIK22w==", "d3caf6a3-1379-4e8d-b6a6-601b7c1c76bc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "903281ca-54d2-4d9f-8e9e-ef529d0afd4c",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 2, 11, 8, 28, 29, 311, DateTimeKind.Utc).AddTicks(1253), new DateTime(2025, 2, 11, 8, 28, 29, 311, DateTimeKind.Utc).AddTicks(1258) });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b7ee85d4-608f-4671-9a00-71cb05ffc2d4",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 2, 11, 8, 28, 29, 311, DateTimeKind.Utc).AddTicks(2289), new DateTime(2025, 2, 11, 8, 28, 29, 311, DateTimeKind.Utc).AddTicks(2290) });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "ccb2c35c-99a6-4d9c-8ee0-d0aec4a9a48a",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "22334a72-f8fc-4e68-9b0d-d0041d2f7d76", "AQAAAAIAAYagAAAAEGSuMmqtFEvIsSFmB8+Z1VdC3o7JSDW+xaKm07gHx/xONVNAJ87bpdut7/8PD5+sWQ==", "0356dc1e-cf91-41cb-be09-23269ece7cb0" });
        }
    }
}
