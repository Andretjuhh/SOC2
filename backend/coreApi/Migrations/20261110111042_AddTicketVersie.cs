using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace coreApi.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketVersie : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "Versie",
                table: "Tickets",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Versie",
                table: "Tickets");
        }
    }
}
