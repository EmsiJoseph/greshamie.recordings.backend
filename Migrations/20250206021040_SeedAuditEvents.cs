using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class SeedAuditEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Action",
                table: "AuditEntries");

            migrationBuilder.AlterColumn<string>(
                name: "Details",
                table: "AuditEntries",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EventId",
                table: "AuditEntries",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "AuditEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditEvents", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "AuditEvents",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "A user successfully logged in.", "UserLoggedIn" },
                    { 2, "A user logged out.", "UserLoggedOut" },
                    { 3, "A new record was played.", "RecordPlayed" },
                    { 4, "An existing record was exported.", "RecordExported" },
                    { 5, "A record was deleted.", "RecordDeleted" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditEntries_EventId",
                table: "AuditEntries",
                column: "EventId");

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
                name: "AuditEvents");

            migrationBuilder.DropIndex(
                name: "IX_AuditEntries_EventId",
                table: "AuditEntries");

            migrationBuilder.DropColumn(
                name: "EventId",
                table: "AuditEntries");

            migrationBuilder.AlterColumn<string>(
                name: "Details",
                table: "AuditEntries",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Action",
                table: "AuditEntries",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
