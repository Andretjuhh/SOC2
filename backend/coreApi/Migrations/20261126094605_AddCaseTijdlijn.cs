using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace coreApi.Migrations
{
    /// <inheritdoc />
    public partial class AddCaseTijdlijn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AanvalsTijdlijn",
                table: "Cases");

            migrationBuilder.CreateTable(
                name: "CaseTijdlijn",
                columns: table => new
                {
                    CaseTijdlijnItemID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CaseID = table.Column<int>(type: "int", nullable: false),
                    Tijdstip = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Beschrijving = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Systeem = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Fase = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ToegevoegdDoor = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ToegevoegdOp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseTijdlijn", x => x.CaseTijdlijnItemID);
                    table.ForeignKey(
                        name: "FK_CaseTijdlijn_Cases_CaseID",
                        column: x => x.CaseID,
                        principalTable: "Cases",
                        principalColumn: "CaseID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CaseTijdlijn_CaseID_Tijdstip",
                table: "CaseTijdlijn",
                columns: new[] { "CaseID", "Tijdstip" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CaseTijdlijn");

            migrationBuilder.AddColumn<string>(
                name: "AanvalsTijdlijn",
                table: "Cases",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
