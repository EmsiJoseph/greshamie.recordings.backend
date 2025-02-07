using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class FixedUserRoleSeeder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "0054ea5e-7751-4e14-b49d-33b6a8418a19");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "952d75bb-1e47-4e65-b0ee-3502849a8cbd");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "CreatedAt", "Description", "IsActive", "Level", "Name", "NormalizedName", "UpdatedAt" },
                values: new object[,]
                {
                    { "c28b2613-0ff4-4bec-ad07-4a5dc19fa044", null, new DateTime(2025, 2, 7, 8, 49, 27, 320, DateTimeKind.Utc).AddTicks(213), "User", true, 90, "User", "USER", new DateTime(2025, 2, 7, 8, 49, 27, 320, DateTimeKind.Utc).AddTicks(214) },
                    { "c2bac7d5-125f-4e87-8efe-c1113e15a1ac", null, new DateTime(2025, 2, 7, 8, 49, 27, 319, DateTimeKind.Utc).AddTicks(9135), "Administrator", true, 100, "Admin", "ADMIN", new DateTime(2025, 2, 7, 8, 49, 27, 319, DateTimeKind.Utc).AddTicks(9137) }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ClarifyGoAccessToken", "ClarifyGoAccessTokenExpiry", "ConcurrencyStamp", "Email", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "RefreshToken", "RefreshTokenExpiry", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "16a50dcc-6e55-4375-a258-cd876d15bf74", 0, null, null, "e5658b52-3b7b-43ad-80a1-d68c7768902e", null, false, false, null, null, null, "AQAAAAIAAYagAAAAEIJQ+jW55X+JokL7Xb9+vrLlYvtRDmFEPapkMLfj638Y1dENVKvNT7+PIO0dvNYIJQ==", null, false, null, null, "56361ac7-34f6-47ff-98fc-8c896075e845", false, "GHIE-API" });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "c2bac7d5-125f-4e87-8efe-c1113e15a1ac", "16a50dcc-6e55-4375-a258-cd876d15bf74" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c28b2613-0ff4-4bec-ad07-4a5dc19fa044");

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "c2bac7d5-125f-4e87-8efe-c1113e15a1ac", "16a50dcc-6e55-4375-a258-cd876d15bf74" });

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c2bac7d5-125f-4e87-8efe-c1113e15a1ac");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "16a50dcc-6e55-4375-a258-cd876d15bf74");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "CreatedAt", "Description", "IsActive", "Level", "Name", "NormalizedName", "UpdatedAt" },
                values: new object[,]
                {
                    { "0054ea5e-7751-4e14-b49d-33b6a8418a19", null, new DateTime(2025, 2, 7, 8, 36, 41, 610, DateTimeKind.Utc).AddTicks(4261), "User of Gresham Recordings", true, 90, "User", "USER", new DateTime(2025, 2, 7, 8, 36, 41, 610, DateTimeKind.Utc).AddTicks(4262) },
                    { "952d75bb-1e47-4e65-b0ee-3502849a8cbd", null, new DateTime(2025, 2, 7, 8, 36, 41, 610, DateTimeKind.Utc).AddTicks(3066), "Administrator of Gresham Recordings", true, 90, "Admin", "ADMIN", new DateTime(2025, 2, 7, 8, 36, 41, 610, DateTimeKind.Utc).AddTicks(3072) }
                });
        }
    }
}
