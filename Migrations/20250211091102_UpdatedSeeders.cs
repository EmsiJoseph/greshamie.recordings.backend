using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedSeeders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "903281ca-54d2-4d9f-8e9e-ef529d0afd4c",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 2, 11, 9, 11, 1, 476, DateTimeKind.Utc).AddTicks(9608), new DateTime(2025, 2, 11, 9, 11, 1, 476, DateTimeKind.Utc).AddTicks(9611) });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b7ee85d4-608f-4671-9a00-71cb05ffc2d4",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 2, 11, 9, 11, 1, 477, DateTimeKind.Utc).AddTicks(613), new DateTime(2025, 2, 11, 9, 11, 1, 477, DateTimeKind.Utc).AddTicks(614) });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "ccb2c35c-99a6-4d9c-8ee0-d0aec4a9a48a",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "751144e6-bc50-4236-a557-06c41c7f4549", "AQAAAAIAAYagAAAAEPwdSJoVi8MJplNdg42z0QByHny5vrJRIx9hzX8zEyS+dUafkJTPSYRXROnaA8wpkQ==", "c2d5f04e-5dc1-4886-a25a-4fede600a77c" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
    }
}
