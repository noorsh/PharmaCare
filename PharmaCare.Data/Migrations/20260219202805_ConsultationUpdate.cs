using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PharmaCare.Data.Migrations
{
    /// <inheritdoc />
    public partial class ConsultationUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PharmacistId",
                table: "Consultations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAt",
                table: "Consultations",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Consultations_PharmacistId",
                table: "Consultations",
                column: "PharmacistId");

            migrationBuilder.AddForeignKey(
                name: "FK_Consultations_AspNetUsers_PharmacistId",
                table: "Consultations",
                column: "PharmacistId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Consultations_AspNetUsers_PharmacistId",
                table: "Consultations");

            migrationBuilder.DropIndex(
                name: "IX_Consultations_PharmacistId",
                table: "Consultations");

            migrationBuilder.DropColumn(
                name: "PharmacistId",
                table: "Consultations");

            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                table: "Consultations");
        }
    }
}
