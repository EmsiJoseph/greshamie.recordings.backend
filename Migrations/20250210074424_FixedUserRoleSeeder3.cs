using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class FixedUserRoleSeeder3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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
                    { "7195bb7d-4b88-4749-9eb2-3768ef599058", null, new DateTime(2025, 2, 10, 7, 44, 24, 360, DateTimeKind.Utc).AddTicks(7881), "Administrator", true, 100, "Admin", "ADMIN", new DateTime(2025, 2, 10, 7, 44, 24, 360, DateTimeKind.Utc).AddTicks(7885) },
                    { "de0addf8-9690-44de-a108-f8882c9dc9f2", null, new DateTime(2025, 2, 10, 7, 44, 24, 360, DateTimeKind.Utc).AddTicks(8892), "User", true, 90, "User", "USER", new DateTime(2025, 2, 10, 7, 44, 24, 360, DateTimeKind.Utc).AddTicks(8892) }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ClarifyGoAccessToken", "ClarifyGoAccessTokenExpiry", "ConcurrencyStamp", "Email", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "RefreshToken", "RefreshTokenExpiry", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "ba9f0fd7-febb-4016-990e-2fe1159a6fc8", 0, null, null, "c546c281-c759-4041-921d-49e5b2cf3e9b", null, false, false, null, null, "GHIE-API", "AQAAAAIAAYagAAAAEA7lAehUHMNhhazdeLapuE/DPFF4YRXocnEbTVraDAMnnDvwsHUjriFPA6NLiNO8jg==", null, false, null, null, "b1abee8a-9103-4d35-968d-57b350e23f75", false, "GHIE-API" });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "7195bb7d-4b88-4749-9eb2-3768ef599058", "ba9f0fd7-febb-4016-990e-2fe1159a6fc8" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "de0addf8-9690-44de-a108-f8882c9dc9f2");

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "7195bb7d-4b88-4749-9eb2-3768ef599058", "ba9f0fd7-febb-4016-990e-2fe1159a6fc8" });

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "7195bb7d-4b88-4749-9eb2-3768ef599058");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "ba9f0fd7-febb-4016-990e-2fe1159a6fc8");

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
    }
}
