using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddedCallType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditEntries_AuditEvents_EventId",
                table: "AuditEntries");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddForeignKey(
                name: "FK_AuditEntries_AuditEvents_EventId",
                table: "AuditEntries",
                column: "EventId",
                principalTable: "AuditEvents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
