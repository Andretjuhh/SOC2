using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace coreApi.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketHistorie : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TicketHistorie",
                columns: table => new
                {
                    TicketHistorieID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TicketID = table.Column<int>(type: "int", nullable: false),
                    Actie = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Omschrijving = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GebruikerID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Tijdstip = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketHistorie", x => x.TicketHistorieID);
                    table.ForeignKey(
                        name: "FK_TicketHistorie_Tickets_TicketID",
                        column: x => x.TicketID,
                        principalTable: "Tickets",
                        principalColumn: "TicketID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TicketHistorie_TicketID",
                table: "TicketHistorie",
                column: "TicketID");

                            migrationBuilder.Sql(@"
            CREATE TRIGGER TR_TicketHistorie_Onwijzigbaar
            ON TicketHistorie
            INSTEAD OF UPDATE, DELETE
            AS
            BEGIN
                THROW 50000, 'Ticket-historie mag niet worden gewijzigd of verwijderd.', 1;
            END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TicketHistorie");
        
            migrationBuilder.Sql("DROP TRIGGER TR_TicketHistorie_Onwijzigbaar");
        }
    }
}
