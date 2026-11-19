using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace coreApi.Migrations
{
    /// <inheritdoc />
    public partial class AddAlertAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AssignedUserId",
                table: "Alerts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_AssignedUserId",
                table: "Alerts",
                column: "AssignedUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Alerts_Gebruikers_AssignedUserId",
                table: "Alerts",
                column: "AssignedUserId",
                principalTable: "Gebruikers",
                principalColumn: "GebruikerID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Alerts_Gebruikers_AssignedUserId",
                table: "Alerts");

            migrationBuilder.DropIndex(
                name: "IX_Alerts_AssignedUserId",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "AssignedUserId",
                table: "Alerts");
        }
    }
}
