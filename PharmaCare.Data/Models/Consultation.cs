using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmaCare.Data.Models
{
    public class Consultation
    {
        [Key]
        public int ConsultationId { get; set; }

        [Required]
        public int PatientId { get; set; }

        [ForeignKey("PatientId")]
        public Patient Patient { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Symptoms { get; set; } // Patient's description of symptoms

        [MaxLength(50)]
        public string? SymptomDuration { get; set; } // How long they've had symptoms

        [MaxLength(50)]
        public string? SymptomSeverity { get; set; } // Mild, Moderate, Severe

        [MaxLength(1000)]
        public string? AdditionalInformation { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } // Pending, UnderReview, Completed, Cancelled

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? CompletedAt { get; set; }

        // Navigation Properties
        public AIAssessment? AIAssessment { get; set; }
        public Recommendation? Recommendation { get; set; }
        public MedicationOrder? MedicationOrder { get; set; }
        public ICollection<ConsultationAttachment> Attachments { get; set; } = new List<ConsultationAttachment>();
        public ICollection<ConsultationMessage> Messages { get; set; } = new List<ConsultationMessage>();

        public string? PharmacistId { get; set; } // ASP.NET Identity UserId (string)

        [ForeignKey("PharmacistId")]
        public ApplicationUser? Pharmacist { get; set; }

        public DateTime? ReviewedAt { get; set; }
        
    }
}