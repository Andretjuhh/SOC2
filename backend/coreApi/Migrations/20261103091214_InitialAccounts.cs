using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace coreApi.Migrations
{
    public partial class InitialAccounts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Klanten",
                columns: table => new
                {
                    KlantID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    naam = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    telefoonnummer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SLA = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    isActief = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Klanten", x => x.KlantID);
                });

            migrationBuilder.CreateTable(
                name: "Rechten",
                columns: table => new
                {
                    RechtID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Naam = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Beschrijving = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rechten", x => x.RechtID);
                });

            migrationBuilder.CreateTable(
                name: "Rollen",
                columns: table => new
                {
                    RolID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Naam = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Beschrijving = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rollen", x => x.RolID);
                });

            migrationBuilder.CreateTable(
                name: "Gebruikers",
                columns: table => new
                {
                    GebruikerID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Naam = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Gebruikersnaam = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    GenormaliseerdeGebruikersnaam = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WachtwoordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActief = table.Column<bool>(type: "bit", nullable: false),
                    KlantID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RolID = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Gebruikers", x => x.GebruikerID);
                    table.ForeignKey(
                        name: "FK_Gebruikers_Klanten_KlantID",
                        column: x => x.KlantID,
                        principalTable: "Klanten",
                        principalColumn: "KlantID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Gebruikers_Rollen_RolID",
                        column: x => x.RolID,
                        principalTable: "Rollen",
                        principalColumn: "RolID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RechtRol",
                columns: table => new
                {
                    RechtenRechtID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RollenRolID = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RechtRol", x => new { x.RechtenRechtID, x.RollenRolID });
                    table.ForeignKey(
                        name: "FK_RechtRol_Rechten_RechtenRechtID",
                        column: x => x.RechtenRechtID,
                        principalTable: "Rechten",
                        principalColumn: "RechtID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RechtRol_Rollen_RollenRolID",
                        column: x => x.RollenRolID,
                        principalTable: "Rollen",
                        principalColumn: "RolID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Gebruikers_GenormaliseerdeGebruikersnaam",
                table: "Gebruikers",
                column: "GenormaliseerdeGebruikersnaam",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Gebruikers_KlantID",
                table: "Gebruikers",
                column: "KlantID");

            migrationBuilder.CreateIndex(
                name: "IX_Gebruikers_RolID",
                table: "Gebruikers",
                column: "RolID");

            migrationBuilder.CreateIndex(
                name: "IX_Rechten_Naam",
                table: "Rechten",
                column: "Naam",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RechtRol_RollenRolID",
                table: "RechtRol",
                column: "RollenRolID");

            migrationBuilder.CreateIndex(
                name: "IX_Rollen_Naam",
                table: "Rollen",
                column: "Naam",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Gebruikers");

            migrationBuilder.DropTable(
                name: "RechtRol");

            migrationBuilder.DropTable(
                name: "Klanten");

            migrationBuilder.DropTable(
                name: "Rechten");

            migrationBuilder.DropTable(
                name: "Rollen");
        }
    }
}
