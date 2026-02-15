using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmaCare.Data.Models
{
    public class Allergy
    {
        [Key]
        public int AllergyId { get; set; }

        [Required]
        public int PatientId { get; set; }

        [ForeignKey("PatientId")]
        public Patient? Patient { get; set; }  // Make nullable

        [Required(ErrorMessage = "Allergen name is required")]
        [MaxLength(200)]
        public string AllergenName { get; set; }

        [Required]
        public Enums.AllergyType AllergyType { get; set; }

        [Required]
        public Enums.AllergySeverity Severity { get; set; }

        [MaxLength(500)]
        public string? Reaction { get; set; } // Description of reaction

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}