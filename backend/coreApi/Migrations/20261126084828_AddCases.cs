using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace coreApi.Migrations
{
    /// <inheritdoc />
    public partial class AddCases : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AlertID",
                table: "Tickets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CaseID",
                table: "Tickets",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Cases",
                columns: table => new
                {
                    CaseID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titel = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Beschrijving = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Prioriteit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RootCause = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AanvalsTijdlijn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KlantID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AangemaaktDoor = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AangemaaktOp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AfgeslotenOp = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Versie = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cases", x => x.CaseID);
                    table.ForeignKey(
                        name: "FK_Cases_Klanten_KlantID",
                        column: x => x.KlantID,
                        principalTable: "Klanten",
                        principalColumn: "KlantID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_AlertID",
                table: "Tickets",
                column: "AlertID",
                unique: true,
                filter: "[AlertID] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_CaseID",
                table: "Tickets",
                column: "CaseID");

            migrationBuilder.CreateIndex(
                name: "IX_Cases_KlantID",
                table: "Cases",
                column: "KlantID");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Alerts_AlertID",
                table: "Tickets",
                column: "AlertID",
                principalTable: "Alerts",
                principalColumn: "AlertID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Cases_CaseID",
                table: "Tickets",
                column: "CaseID",
                principalTable: "Cases",
                principalColumn: "CaseID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Alerts_AlertID",
                table: "Tickets");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Cases_CaseID",
                table: "Tickets");

            migrationBuilder.DropTable(
                name: "Cases");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_AlertID",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_CaseID",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "AlertID",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "CaseID",
                table: "Tickets");
        }
    }
}
