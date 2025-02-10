using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class SeederUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<int>(
                name: "IdFromClarify",
                table: "CallTypes",
                type: "int",
                nullable: false,
                defaultValue: 0);

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

            migrationBuilder.UpdateData(
                table: "CallTypes",
                keyColumn: "Id",
                keyValue: 1,
                column: "IdFromClarify",
                value: 0);

            migrationBuilder.UpdateData(
                table: "CallTypes",
                keyColumn: "Id",
                keyValue: 2,
                column: "IdFromClarify",
                value: 1);

            migrationBuilder.UpdateData(
                table: "CallTypes",
                keyColumn: "Id",
                keyValue: 3,
                column: "IdFromClarify",
                value: 2);

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "d2b60933-1257-42c9-8a2c-53673a823c6b", "aa11a2f2-ca6f-4c4f-afcd-722a0bb83c50" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropColumn(
                name: "IdFromClarify",
                table: "CallTypes");

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
    }
}
