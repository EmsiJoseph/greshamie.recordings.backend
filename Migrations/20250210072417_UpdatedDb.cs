using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "36a698a5-b0fe-43ef-8ebf-2380f8a5b610");

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "d2b60933-1257-42c9-8a2c-53673a823c6b", "aa11a2f2-ca6f-4c4f-afcd-722a0bb83c50" });

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d2b60933-1257-42c9-8a2c-53673a823c6b");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "aa11a2f2-ca6f-4c4f-afcd-722a0bb83c50");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "CreatedAt", "Description", "IsActive", "Level", "Name", "NormalizedName", "UpdatedAt" },
                values: new object[,]
                {
                    { "1b352a78-3d5d-45b1-bce4-5f35cffaade3", null, new DateTime(2025, 2, 10, 7, 24, 17, 166, DateTimeKind.Utc).AddTicks(8875), "User", true, 90, "User", "USER", new DateTime(2025, 2, 10, 7, 24, 17, 166, DateTimeKind.Utc).AddTicks(8875) },
                    { "73b9cc76-5efb-4804-8090-9e23daef9fe4", null, new DateTime(2025, 2, 10, 7, 24, 17, 166, DateTimeKind.Utc).AddTicks(8016), "Administrator", true, 100, "Admin", "ADMIN", new DateTime(2025, 2, 10, 7, 24, 17, 166, DateTimeKind.Utc).AddTicks(8019) }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ClarifyGoAccessToken", "ClarifyGoAccessTokenExpiry", "ConcurrencyStamp", "Email", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "RefreshToken", "RefreshTokenExpiry", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "21bacca7-2dee-4ef8-a41d-121acf3c8717", 0, null, null, "14488aef-d849-4563-9e3f-862e410f718b", null, false, false, null, null, null, "AQAAAAIAAYagAAAAECr/Lj6miUiTrqPOhzdA7zWj3jheHIKP7meNCsFxdYdrbrN0Z9cP2eScw+q54n1KFA==", null, false, null, null, "f05ab707-cba9-422b-9c43-25dd739ada56", false, "GHIE-API" });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "73b9cc76-5efb-4804-8090-9e23daef9fe4", "21bacca7-2dee-4ef8-a41d-121acf3c8717" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1b352a78-3d5d-45b1-bce4-5f35cffaade3");

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "73b9cc76-5efb-4804-8090-9e23daef9fe4", "21bacca7-2dee-4ef8-a41d-121acf3c8717" });

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "73b9cc76-5efb-4804-8090-9e23daef9fe4");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "21bacca7-2dee-4ef8-a41d-121acf3c8717");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "CreatedAt", "Description", "IsActive", "Level", "Name", "NormalizedName", "UpdatedAt" },
                values: new object[,]
                {
                    { "36a698a5-b0fe-43ef-8ebf-2380f8a5b610", null, new DateTime(2025, 2, 7, 9, 3, 14, 596, DateTimeKind.Utc).AddTicks(9784), "User", true, 90, "User", "USER", new DateTime(2025, 2, 7, 9, 3, 14, 596, DateTimeKind.Utc).AddTicks(9785) },
                    { "d2b60933-1257-42c9-8a2c-53673a823c6b", null, new DateTime(2025, 2, 7, 9, 3, 14, 596, DateTimeKind.Utc).AddTicks(8705), "Administrator", true, 100, "Admin", "ADMIN", new DateTime(2025, 2, 7, 9, 3, 14, 596, DateTimeKind.Utc).AddTicks(8708) }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ClarifyGoAccessToken", "ClarifyGoAccessTokenExpiry", "ConcurrencyStamp", "Email", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "RefreshToken", "RefreshTokenExpiry", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "aa11a2f2-ca6f-4c4f-afcd-722a0bb83c50", 0, null, null, "69b28c4e-111e-4c2b-a5b0-aae0a34113ba", null, false, false, null, null, null, "AQAAAAIAAYagAAAAEPJ0mdHulCSLgRv6JJwIC5yL1SOzDt/QDgqxN3Hhl505jJV8U7jPqvZrSkQAo7FsXQ==", null, false, null, null, "9d774300-c3d0-4f82-aa57-d8ed37ccc096", false, "GHIE-API" });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "d2b60933-1257-42c9-8a2c-53673a823c6b", "aa11a2f2-ca6f-4c4f-afcd-722a0bb83c50" });
        }
    }
}
