using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PharmaCare.Data.Migrations
{
    public partial class UpdateAllergyTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // =========================
            // AllergyType (string -> int)
            // =========================
            migrationBuilder.AddColumn<int>(
                name: "AllergyType_Temp",
                table: "Allergies",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.Sql(@"
                UPDATE ""Allergies""
                SET ""AllergyType_Temp"" =
                    CASE TRIM(""AllergyType"")
                        WHEN 'Medication' THEN 1
                        WHEN 'Food' THEN 2
                        WHEN 'Environmental' THEN 3
                        WHEN 'Other' THEN 4
                        ELSE 1
                    END
            ");

            migrationBuilder.DropColumn(
                name: "AllergyType",
                table: "Allergies");

            migrationBuilder.RenameColumn(
                name: "AllergyType_Temp",
                table: "Allergies",
                newName: "AllergyType");

            // =========================
            // Severity (string -> int)
            // =========================
            migrationBuilder.AddColumn<int>(
                name: "Severity_Temp",
                table: "Allergies",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.Sql(@"
                UPDATE ""Allergies""
                SET ""Severity_Temp"" =
                    CASE TRIM(""Severity"")
                        WHEN 'Mild' THEN 1
                        WHEN 'Moderate' THEN 2
                        WHEN 'Severe' THEN 3
                        WHEN 'LifeThreatening' THEN 4
                        WHEN 'Life Threatening' THEN 4
                        ELSE 1
                    END
            ");

            migrationBuilder.DropColumn(
                name: "Severity",
                table: "Allergies");

            migrationBuilder.RenameColumn(
                name: "Severity_Temp",
                table: "Allergies",
                newName: "Severity");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // =========================
            // Rollback AllergyType
            // =========================
            migrationBuilder.AddColumn<string>(
                name: "AllergyType_Temp",
                table: "Allergies",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Medication");

            migrationBuilder.Sql(@"
                UPDATE ""Allergies""
                SET ""AllergyType_Temp"" =
                    CASE ""AllergyType""
                        WHEN 1 THEN 'Medication'
                        WHEN 2 THEN 'Food'
                        WHEN 3 THEN 'Environmental'
                        WHEN 4 THEN 'Other'
                        ELSE 'Medication'
                    END
            ");

            migrationBuilder.DropColumn("AllergyType", "Allergies");

            migrationBuilder.RenameColumn(
                name: "AllergyType_Temp",
                table: "Allergies",
                newName: "AllergyType");

            // =========================
            // Rollback Severity
            // =========================
            migrationBuilder.AddColumn<string>(
                name: "Severity_Temp",
                table: "Allergies",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Mild");

            migrationBuilder.Sql(@"
                UPDATE ""Allergies""
                SET ""Severity_Temp"" =
                    CASE ""Severity""
                        WHEN 1 THEN 'Mild'
                        WHEN 2 THEN 'Moderate'
                        WHEN 3 THEN 'Severe'
                        WHEN 4 THEN 'Life Threatening'
                        ELSE 'Mild'
                    END
            ");

            migrationBuilder.DropColumn("Severity", "Allergies");

            migrationBuilder.RenameColumn(
                name: "Severity_Temp",
                table: "Allergies",
                newName: "Severity");
        }
    }
}
