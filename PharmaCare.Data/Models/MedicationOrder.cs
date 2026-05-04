using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmaCare.Data.Models
{
    public class MedicationOrder
    {
        [Key]
        public int OrderId { get; set; }

        [Required]
        public int ConsultationId { get; set; }

        [ForeignKey("ConsultationId")]
        public Consultation Consultation { get; set; }

        [Required]
        public int InventoryId { get; set; }

        [ForeignKey("InventoryId")]
        public Inventory Inventory { get; set; }

        [Required]
        public string PatientUserId { get; set; }

        [ForeignKey("PatientUserId")]
        public ApplicationUser Patient { get; set; }

        [Required]
        [MaxLength(500)]
        public string DeliveryAddress { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Dispatched, Cancelled

        public DateTime OrderedAt { get; set; } = DateTime.UtcNow;

        public DateTime? DispatchedAt { get; set; }

        [MaxLength(500)]
        public string? PharmacistNotes { get; set; }
    }
}