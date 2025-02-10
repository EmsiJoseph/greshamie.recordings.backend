using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class syncRecordings : Migration
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

            migrationBuilder.CreateTable(
                name: "SyncedRecordings",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BlobUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RecordingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncedRecordings", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "CreatedAt", "Description", "IsActive", "Level", "Name", "NormalizedName", "UpdatedAt" },
                values: new object[,]
                {
                    { "3269c9b3-59be-4826-96b2-4a30a4a17fc0", null, new DateTime(2025, 2, 10, 3, 30, 39, 24, DateTimeKind.Utc).AddTicks(4926), "User", true, 90, "User", "USER", new DateTime(2025, 2, 10, 3, 30, 39, 24, DateTimeKind.Utc).AddTicks(4930) },
                    { "e0fc6b82-199a-41f4-9d8a-e27b4177dcc6", null, new DateTime(2025, 2, 10, 3, 30, 39, 24, DateTimeKind.Utc).AddTicks(3050), "Administrator", true, 100, "Admin", "ADMIN", new DateTime(2025, 2, 10, 3, 30, 39, 24, DateTimeKind.Utc).AddTicks(3054) }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ClarifyGoAccessToken", "ClarifyGoAccessTokenExpiry", "ConcurrencyStamp", "Email", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "RefreshToken", "RefreshTokenExpiry", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "bacf610e-b5d9-4df4-b044-12e4692bc94d", 0, null, null, "d1dcfe87-ef6e-4cd1-ac31-d2ec18d62711", null, false, false, null, null, null, "AQAAAAIAAYagAAAAENa15A8Pk5/DrbbtvdkycszWKWP04i5qrc0IaxN/xQ1+SjTdeuzBE6hUgGTDEfxLGA==", null, false, null, null, "30a3b93f-a02a-44b8-983a-5acc68aea53f", false, "GHIE-API" });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "e0fc6b82-199a-41f4-9d8a-e27b4177dcc6", "bacf610e-b5d9-4df4-b044-12e4692bc94d" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SyncedRecordings");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "3269c9b3-59be-4826-96b2-4a30a4a17fc0");

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "e0fc6b82-199a-41f4-9d8a-e27b4177dcc6", "bacf610e-b5d9-4df4-b044-12e4692bc94d" });

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "e0fc6b82-199a-41f4-9d8a-e27b4177dcc6");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "bacf610e-b5d9-4df4-b044-12e4692bc94d");

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
