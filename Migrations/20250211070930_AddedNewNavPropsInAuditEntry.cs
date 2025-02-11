using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddedNewNavPropsInAuditEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.AlterColumn<string>(
                name: "RecordId",
                table: "AuditEntries",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

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

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "bc1380a7-931c-4554-918e-a5f4dde1ec2b", "0468ec16-4ce3-4a5a-807c-0da5e0c6ca52" });

            migrationBuilder.AddForeignKey(
                name: "FK_AuditEntries_SyncedRecordings_RecordId",
                table: "AuditEntries",
                column: "RecordId",
                principalTable: "SyncedRecordings",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditEntries_SyncedRecordings_RecordId",
                table: "AuditEntries");

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

            migrationBuilder.AlterColumn<string>(
                name: "RecordId",
                table: "AuditEntries",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

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
                name: "FK_AuditEntries_SyncedRecordings_RecordId",
                table: "AuditEntries",
                column: "RecordId",
                principalTable: "SyncedRecordings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
