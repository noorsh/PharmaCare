using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmaCare.Data.Models
{
    public class CurrentMedication
    {
        [Key]
        public int CurrentMedicationId { get; set; }

        [Required]
        public int PatientId { get; set; }

        [ForeignKey("PatientId")]
        public Patient? Patient { get; set; }  // Make nullable

        [Required(ErrorMessage = "Medication name is required")]
        [MaxLength(200)]
        public string MedicationName { get; set; }

        [MaxLength(100)]
        public string? Dosage { get; set; }

        [MaxLength(100)]
        public string? Frequency { get; set; } // Once daily, Twice daily, etc.

        public DateTime? StartDate { get; set; }

        [MaxLength(500)]
        public string? Purpose { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}