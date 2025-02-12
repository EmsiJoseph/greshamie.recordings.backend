using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class Test : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "903281ca-54d2-4d9f-8e9e-ef529d0afd4c",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 2, 11, 9, 12, 35, 510, DateTimeKind.Utc).AddTicks(7983), new DateTime(2025, 2, 11, 9, 12, 35, 510, DateTimeKind.Utc).AddTicks(7987) });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b7ee85d4-608f-4671-9a00-71cb05ffc2d4",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 2, 11, 9, 12, 35, 510, DateTimeKind.Utc).AddTicks(9057), new DateTime(2025, 2, 11, 9, 12, 35, 510, DateTimeKind.Utc).AddTicks(9058) });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "ccb2c35c-99a6-4d9c-8ee0-d0aec4a9a48a",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "d90e7d83-e0d4-457b-a3f9-4f4d9bab85f9", "AQAAAAIAAYagAAAAENPSYeCXhaBFGkl53pOHkC2YXuN3n4b/g/EOgfqwEKcSyLAqyYP+5BcxDJ+lvJs/tw==", "3a6078a7-0d12-4dd2-87fa-df7764fcdaee" });

            migrationBuilder.UpdateData(
                table: "AuditEvents",
                keyColumn: "Id",
                keyValue: 6,
                column: "Description",
                value: "A token was refreshed. The old token is now invalid.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.UpdateData(
                table: "AuditEvents",
                keyColumn: "Id",
                keyValue: 6,
                column: "Description",
                value: "A token was refreshed.");
        }
    }
}
