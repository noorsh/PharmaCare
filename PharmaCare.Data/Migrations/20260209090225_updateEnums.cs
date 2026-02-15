using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PharmaCare.Data.Migrations
{
    public partial class updateEnums : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1️⃣ SmokingStatus
            migrationBuilder.AddColumn<int>(
                name: "SmokingStatus_Temp",
                table: "Patients",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE ""Patients""
                SET ""SmokingStatus_Temp"" =
                    CASE ""SmokingStatus""
                        WHEN 'Never' THEN 1
                        WHEN 'Former' THEN 2
                        WHEN 'Current' THEN 3
                    END
            ");

            migrationBuilder.DropColumn(
                name: "SmokingStatus",
                table: "Patients");

            migrationBuilder.RenameColumn(
                name: "SmokingStatus_Temp",
                table: "Patients",
                newName: "SmokingStatus");

            // 2️⃣ ExerciseFrequency
            migrationBuilder.AddColumn<int>(
                name: "ExerciseFrequency_Temp",
                table: "Patients",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE ""Patients""
                SET ""ExerciseFrequency_Temp"" =
                    CASE ""ExerciseFrequency""
                        WHEN 'Sedentary' THEN 1
                        WHEN 'Light' THEN 2
                        WHEN 'Moderate' THEN 3
                        WHEN 'Active' THEN 4
                    END
            ");

            migrationBuilder.DropColumn(
                name: "ExerciseFrequency",
                table: "Patients");

            migrationBuilder.RenameColumn(
                name: "ExerciseFrequency_Temp",
                table: "Patients",
                newName: "ExerciseFrequency");

            // 3️⃣ AlcoholConsumption
            migrationBuilder.AddColumn<int>(
                name: "AlcoholConsumption_Temp",
                table: "Patients",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE ""Patients""
                SET ""AlcoholConsumption_Temp"" =
                    CASE ""AlcoholConsumption""
                        WHEN 'None' THEN 1
                        WHEN 'Occasional' THEN 2
                        WHEN 'Regular' THEN 3
                    END
            ");

            migrationBuilder.DropColumn(
                name: "AlcoholConsumption",
                table: "Patients");

            migrationBuilder.RenameColumn(
                name: "AlcoholConsumption_Temp",
                table: "Patients",
                newName: "AlcoholConsumption");

            // 4️⃣ BloodType
            migrationBuilder.AddColumn<int>(
                name: "BloodType_Temp",
                table: "Patients",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE ""Patients""
                SET ""BloodType_Temp"" =
                    CASE ""BloodType""
                        WHEN 'A+' THEN 1
                        WHEN 'A-' THEN 2
                        WHEN 'B+' THEN 3
                        WHEN 'B-' THEN 4
                        WHEN 'AB+' THEN 5
                        WHEN 'AB-' THEN 6
                        WHEN 'O+' THEN 7
                        WHEN 'O-' THEN 8
                    END
            ");

            migrationBuilder.DropColumn(
                name: "BloodType",
                table: "Patients");

            migrationBuilder.RenameColumn(
                name: "BloodType_Temp",
                table: "Patients",
                newName: "BloodType");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Optional: reverse conversion (int -> string)
            // Only needed if you plan to rollback the migration
        }
    }
}
