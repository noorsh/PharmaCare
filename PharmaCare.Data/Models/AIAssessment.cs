using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmaCare.Data.Models
{
    public class AIAssessment
    {
        [Key]
        public int AIAssessmentId { get; set; }

        [Required]
        public int ConsultationId { get; set; }

        [ForeignKey("ConsultationId")]
        public Consultation Consultation { get; set; }

        [Required]
        [MaxLength(2000)]
        public string AssessmentReport { get; set; } // AI-generated assessment

        [MaxLength(500)]
        public string? PossibleConditions { get; set; } // JSON or comma-separated

        [MaxLength(1000)]
        public string? SuggestedQuestions { get; set; } // Questions for pharmacist to ask

        [Column(TypeName = "decimal(5,2)")]
        public decimal? ConfidenceScore { get; set; } // AI confidence 0-100

        [MaxLength(1000)]
        public string? RedFlags { get; set; } // Warning signs for pharmacist

        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }
}