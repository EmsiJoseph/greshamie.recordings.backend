using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddedNewAuditEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "709c8d10-f076-4dfa-8888-0defcea3542d");

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "bc1380a7-931c-4554-918e-a5f4dde1ec2b", "0468ec16-4ce3-4a5a-807c-0da5e0c6ca52" });

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "bc1380a7-931c-4554-918e-a5f4dde1ec2b");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "0468ec16-4ce3-4a5a-807c-0da5e0c6ca52");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "CreatedAt", "Description", "IsActive", "Level", "Name", "NormalizedName", "UpdatedAt" },
                values: new object[,]
                {
                    { "31ccbd99-32f0-4c7b-aa52-68a470d9aab1", null, new DateTime(2025, 2, 11, 7, 41, 16, 869, DateTimeKind.Utc).AddTicks(2084), "Standard user role with limited access", true, 90, "User", "USER", new DateTime(2025, 2, 11, 7, 41, 16, 869, DateTimeKind.Utc).AddTicks(2085) },
                    { "f645179d-213a-447f-af1f-801019c8611c", null, new DateTime(2025, 2, 11, 7, 41, 16, 869, DateTimeKind.Utc).AddTicks(1097), "Administrator role with full access", true, 100, "Admin", "ADMIN", new DateTime(2025, 2, 11, 7, 41, 16, 869, DateTimeKind.Utc).AddTicks(1100) }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ClarifyGoAccessToken", "ClarifyGoAccessTokenExpiry", "ConcurrencyStamp", "Email", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "RefreshToken", "RefreshTokenExpiry", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "55cb182c-295d-461a-b0f0-202e3a4410af", 0, null, null, "ae306374-e40b-4fb8-ab22-a71ad5b835ab", null, false, false, null, null, "GHIE-API", "AQAAAAIAAYagAAAAEG3uR/jD24uGhtNSy6hLj1GeMDKzwLABVnvnCmIiEjKunMIx4UBaVMQ8EAxAVQ0pcA==", null, false, null, null, "fa1a8b97-e583-4473-8723-a17d8f86d2d6", false, "GHIE-API" });

            migrationBuilder.UpdateData(
                table: "AuditEvents",
                keyColumn: "Id",
                keyValue: 6,
                column: "Description",
                value: "A token was refreshed.");

            migrationBuilder.InsertData(
                table: "AuditEvents",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 7, "A manual sync was performed.", "ManualSync" },
                    { 8, "An auto sync was performed.", "AutoSync" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "f645179d-213a-447f-af1f-801019c8611c", "55cb182c-295d-461a-b0f0-202e3a4410af" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "31ccbd99-32f0-4c7b-aa52-68a470d9aab1");

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "f645179d-213a-447f-af1f-801019c8611c", "55cb182c-295d-461a-b0f0-202e3a4410af" });

            migrationBuilder.DeleteData(
                table: "AuditEvents",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "AuditEvents",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "f645179d-213a-447f-af1f-801019c8611c");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "55cb182c-295d-461a-b0f0-202e3a4410af");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "CreatedAt", "Description", "IsActive", "Level", "Name", "NormalizedName", "UpdatedAt" },
                values: new object[,]
                {
                    { "709c8d10-f076-4dfa-8888-0defcea3542d", null, new DateTime(2025, 2, 11, 7, 9, 29, 891, DateTimeKind.Utc).AddTicks(2879), "Standard user role with limited access", true, 90, "User", "USER", new DateTime(2025, 2, 11, 7, 9, 29, 891, DateTimeKind.Utc).AddTicks(2880) },
                    { "bc1380a7-931c-4554-918e-a5f4dde1ec2b", null, new DateTime(2025, 2, 11, 7, 9, 29, 891, DateTimeKind.Utc).AddTicks(1864), "Administrator role with full access", true, 100, "Admin", "ADMIN", new DateTime(2025, 2, 11, 7, 9, 29, 891, DateTimeKind.Utc).AddTicks(1869) }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ClarifyGoAccessToken", "ClarifyGoAccessTokenExpiry", "ConcurrencyStamp", "Email", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "RefreshToken", "RefreshTokenExpiry", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "0468ec16-4ce3-4a5a-807c-0da5e0c6ca52", 0, null, null, "3ebc75a8-0b11-4d4f-ae09-44a1aeb420cc", null, false, false, null, null, "GHIE-API", "AQAAAAIAAYagAAAAEFHjH4RblopmRhgMMSuC8BevzapX6xTqUiavWXNjk98NMgFeVey9j69+Y3Rq67V0yg==", null, false, null, null, "19236dbd-3655-476e-8a7f-3c413cf40de0", false, "GHIE-API" });

            migrationBuilder.UpdateData(
                table: "AuditEvents",
                keyColumn: "Id",
                keyValue: 6,
                column: "Description",
                value: "");

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "bc1380a7-931c-4554-918e-a5f4dde1ec2b", "0468ec16-4ce3-4a5a-807c-0da5e0c6ca52" });
        }
    }
}
