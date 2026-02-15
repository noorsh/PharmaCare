using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmaCare.Data.Models
{
    public class MedicalHistory
    {
        [Key]
        public int MedicalHistoryId { get; set; }

        [Required]
        public int PatientId { get; set; }

        [ForeignKey("PatientId")]
        public Patient? Patient { get; set; }  // Make nullable

        [Required(ErrorMessage = "Condition name is required")]
        [MaxLength(200)]
        public string ConditionName { get; set; }

        public DateTime? DiagnosedDate { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true; // Still ongoing condition

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
    }
}