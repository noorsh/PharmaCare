using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmaCare.Data.Models
{
    public class ConsultationMessage
    {
        [Key]
        public int MessageId { get; set; }

        [Required]
        public int ConsultationId { get; set; }

        [ForeignKey("ConsultationId")]
        public Consultation Consultation { get; set; }

        [Required]
        public string SenderUserId { get; set; }

        [ForeignKey("SenderUserId")]
        public ApplicationUser Sender { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Message { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        [MaxLength(20)]
        public string SenderRole { get; set; } = "Patient"; // "Patient" | "Pharmacist"
    }
}