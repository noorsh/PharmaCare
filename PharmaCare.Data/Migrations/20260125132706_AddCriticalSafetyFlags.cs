using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PharmaCare.Data.Migrations
{
    public partial class AddCriticalSafetyFlags : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Columns already added manually via SQL
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "HasKidneyDisease", table: "Patients");
            migrationBuilder.DropColumn(name: "HasLiverDisease", table: "Patients");
            migrationBuilder.DropColumn(name: "HasDrugAllergies", table: "Patients");
            migrationBuilder.DropColumn(name: "DrugAllergyDetails", table: "Patients");
        }
    }
}
