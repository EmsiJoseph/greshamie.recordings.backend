using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class FixedDbContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditEntries_AuditEvent_EventId",
                table: "AuditEntries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AuditEvent",
                table: "AuditEvent");

            migrationBuilder.RenameTable(
                name: "AuditEvent",
                newName: "AuditEvents");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AuditEvents",
                table: "AuditEvents",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "CallTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CallTypes", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_AuditEntries_AuditEvents_EventId",
                table: "AuditEntries",
                column: "EventId",
                principalTable: "AuditEvents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditEntries_AuditEvents_EventId",
                table: "AuditEntries");

            migrationBuilder.DropTable(
                name: "CallTypes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AuditEvents",
                table: "AuditEvents");

            migrationBuilder.RenameTable(
                name: "AuditEvents",
                newName: "AuditEvent");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AuditEvent",
                table: "AuditEvent",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditEntries_AuditEvent_EventId",
                table: "AuditEntries",
                column: "EventId",
                principalTable: "AuditEvent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
