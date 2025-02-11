using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddedEventType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TypeId",
                table: "AuditEvents",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "AuditEventTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NormalizedName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditEventTypes", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "903281ca-54d2-4d9f-8e9e-ef529d0afd4c",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 2, 11, 10, 4, 2, 758, DateTimeKind.Utc).AddTicks(1563), new DateTime(2025, 2, 11, 10, 4, 2, 758, DateTimeKind.Utc).AddTicks(1567) });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b7ee85d4-608f-4671-9a00-71cb05ffc2d4",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 2, 11, 10, 4, 2, 758, DateTimeKind.Utc).AddTicks(2678), new DateTime(2025, 2, 11, 10, 4, 2, 758, DateTimeKind.Utc).AddTicks(2678) });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "ccb2c35c-99a6-4d9c-8ee0-d0aec4a9a48a",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "d3b8fade-4f09-4d80-993f-d6e17b26ea48", "AQAAAAIAAYagAAAAEB61LPkXf6px8MpqV4BXrAVbufHhW6gSzyUm0gbZK5Hnz0z1qwH0qsESv5eR+cQnGA==", "26c13de7-923b-4c68-8347-30278af7c44e" });

            migrationBuilder.InsertData(
                table: "AuditEventTypes",
                columns: new[] { "Id", "Description", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { 1, "Events related to user sessions.", "Session", "SESSION" },
                    { 2, "Events related to call recordings.", "Recording", "RECORDING" }
                });

            migrationBuilder.UpdateData(
                table: "AuditEvents",
                keyColumn: "Id",
                keyValue: 1,
                column: "TypeId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "AuditEvents",
                keyColumn: "Id",
                keyValue: 2,
                column: "TypeId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "AuditEvents",
                keyColumn: "Id",
                keyValue: 3,
                column: "TypeId",
                value: 2);

            migrationBuilder.UpdateData(
                table: "AuditEvents",
                keyColumn: "Id",
                keyValue: 4,
                column: "TypeId",
                value: 2);

            migrationBuilder.UpdateData(
                table: "AuditEvents",
                keyColumn: "Id",
                keyValue: 5,
                column: "TypeId",
                value: 2);

            migrationBuilder.UpdateData(
                table: "AuditEvents",
                keyColumn: "Id",
                keyValue: 6,
                column: "TypeId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "AuditEvents",
                keyColumn: "Id",
                keyValue: 7,
                column: "TypeId",
                value: 2);

            migrationBuilder.UpdateData(
                table: "AuditEvents",
                keyColumn: "Id",
                keyValue: 8,
                column: "TypeId",
                value: 2);

            migrationBuilder.CreateIndex(
                name: "IX_AuditEvents_TypeId",
                table: "AuditEvents",
                column: "TypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditEvents_AuditEventTypes_TypeId",
                table: "AuditEvents",
                column: "TypeId",
                principalTable: "AuditEventTypes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditEvents_AuditEventTypes_TypeId",
                table: "AuditEvents");

            migrationBuilder.DropTable(
                name: "AuditEventTypes");

            migrationBuilder.DropIndex(
                name: "IX_AuditEvents_TypeId",
                table: "AuditEvents");

            migrationBuilder.DropColumn(
                name: "TypeId",
                table: "AuditEvents");

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
        }
    }
}
