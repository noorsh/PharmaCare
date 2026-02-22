using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PharmaCare.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRecommendationWithInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MedicationId",
                table: "Recommendations",
                newName: "InventoryId");

            migrationBuilder.RenameIndex(
                name: "IX_Recommendations_MedicationId",
                table: "Recommendations",
                newName: "IX_Recommendations_InventoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Recommendations_Inventories_InventoryId",
                table: "Recommendations",
                column: "InventoryId",
                principalTable: "Inventories",
                principalColumn: "InventoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Recommendations_Inventories_InventoryId",
                table: "Recommendations");

            migrationBuilder.RenameColumn(
                name: "InventoryId",
                table: "Recommendations",
                newName: "MedicationId");

            migrationBuilder.RenameIndex(
                name: "IX_Recommendations_InventoryId",
                table: "Recommendations",
                newName: "IX_Recommendations_MedicationId");
        }
    }
}