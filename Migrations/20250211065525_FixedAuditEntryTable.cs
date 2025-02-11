using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class FixedAuditEntryTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditEntries_AspNetUsers_UserId",
                table: "AuditEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_AuditEntries_AuditEvents_EventId",
                table: "AuditEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_AuditEntries_SyncedRecordings_RecordId",
                table: "AuditEntries");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "fc6842d3-abc1-4467-9729-19a30955fec4");

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "1321eaaf-4c9e-440c-853f-039aed0f2b96", "6d8c6bb2-416a-4d63-a05f-4906d841b1ae" });

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1321eaaf-4c9e-440c-853f-039aed0f2b96");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "6d8c6bb2-416a-4d63-a05f-4906d841b1ae");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "CreatedAt", "Description", "IsActive", "Level", "Name", "NormalizedName", "UpdatedAt" },
                values: new object[,]
                {
                    { "67905f15-26cf-4ef1-b1e7-58a2f929b61c", null, new DateTime(2025, 2, 11, 6, 55, 25, 201, DateTimeKind.Utc).AddTicks(5500), "Standard user role with limited access", true, 90, "User", "USER", new DateTime(2025, 2, 11, 6, 55, 25, 201, DateTimeKind.Utc).AddTicks(5502) },
                    { "ee7237da-b067-4533-9256-3aa178d5e344", null, new DateTime(2025, 2, 11, 6, 55, 25, 201, DateTimeKind.Utc).AddTicks(3719), "Administrator role with full access", true, 100, "Admin", "ADMIN", new DateTime(2025, 2, 11, 6, 55, 25, 201, DateTimeKind.Utc).AddTicks(3723) }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ClarifyGoAccessToken", "ClarifyGoAccessTokenExpiry", "ConcurrencyStamp", "Email", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "RefreshToken", "RefreshTokenExpiry", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "d52ea5f6-8d4a-4404-bda2-58e59e060ffe", 0, null, null, "07f2cd10-afeb-46eb-be4f-bac5850a9900", null, false, false, null, null, "GHIE-API", "AQAAAAIAAYagAAAAEJNcJ9NG96qWVUhkYuIFpU3MeU4JIvu49pqgYm55zIOvAjk3IBSTJIu9g8n9Qk1FMA==", null, false, null, null, "132fee62-6372-4b2e-bf58-a8d2f34afc20", false, "GHIE-API" });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "ee7237da-b067-4533-9256-3aa178d5e344", "d52ea5f6-8d4a-4404-bda2-58e59e060ffe" });

            migrationBuilder.AddForeignKey(
                name: "FK_AuditEntries_AspNetUsers_UserId",
                table: "AuditEntries",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AuditEntries_AuditEvents_EventId",
                table: "AuditEntries",
                column: "EventId",
                principalTable: "AuditEvents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AuditEntries_SyncedRecordings_RecordId",
                table: "AuditEntries",
                column: "RecordId",
                principalTable: "SyncedRecordings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditEntries_AspNetUsers_UserId",
                table: "AuditEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_AuditEntries_AuditEvents_EventId",
                table: "AuditEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_AuditEntries_SyncedRecordings_RecordId",
                table: "AuditEntries");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "67905f15-26cf-4ef1-b1e7-58a2f929b61c");

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "ee7237da-b067-4533-9256-3aa178d5e344", "d52ea5f6-8d4a-4404-bda2-58e59e060ffe" });

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "ee7237da-b067-4533-9256-3aa178d5e344");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "d52ea5f6-8d4a-4404-bda2-58e59e060ffe");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "CreatedAt", "Description", "IsActive", "Level", "Name", "NormalizedName", "UpdatedAt" },
                values: new object[,]
                {
                    { "1321eaaf-4c9e-440c-853f-039aed0f2b96", null, new DateTime(2025, 2, 11, 6, 23, 49, 218, DateTimeKind.Utc).AddTicks(3606), "Administrator role with full access", true, 100, "Admin", "ADMIN", new DateTime(2025, 2, 11, 6, 23, 49, 218, DateTimeKind.Utc).AddTicks(3609) },
                    { "fc6842d3-abc1-4467-9729-19a30955fec4", null, new DateTime(2025, 2, 11, 6, 23, 49, 218, DateTimeKind.Utc).AddTicks(4539), "Standard user role with limited access", true, 90, "User", "USER", new DateTime(2025, 2, 11, 6, 23, 49, 218, DateTimeKind.Utc).AddTicks(4540) }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ClarifyGoAccessToken", "ClarifyGoAccessTokenExpiry", "ConcurrencyStamp", "Email", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "RefreshToken", "RefreshTokenExpiry", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "6d8c6bb2-416a-4d63-a05f-4906d841b1ae", 0, null, null, "beb08295-cd4f-45de-9613-a9f8cfa78299", null, false, false, null, null, "GHIE-API", "AQAAAAIAAYagAAAAECX/P7mYcQN5Xk37CY2ZDF3oNgyvVZ1eJfE79c6xXAF3ZR7+ascyrJuBL43mE2z6Kg==", null, false, null, null, "98c9b6a2-ed64-44fd-a94a-9f287514af98", false, "GHIE-API" });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "1321eaaf-4c9e-440c-853f-039aed0f2b96", "6d8c6bb2-416a-4d63-a05f-4906d841b1ae" });

            migrationBuilder.AddForeignKey(
                name: "FK_AuditEntries_AspNetUsers_UserId",
                table: "AuditEntries",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AuditEntries_AuditEvents_EventId",
                table: "AuditEntries",
                column: "EventId",
                principalTable: "AuditEvents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AuditEntries_SyncedRecordings_RecordId",
                table: "AuditEntries",
                column: "RecordId",
                principalTable: "SyncedRecordings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
