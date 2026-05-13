using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmaCare.Data.Models
{
    public class ConsultationAttachment
    {
        [Key]
        public int AttachmentId { get; set; }

        [Required]
        public int ConsultationId { get; set; }

        [ForeignKey("ConsultationId")]
        public Consultation Consultation { get; set; }

        [Required]
        public string UploadedByUserId { get; set; }

        [ForeignKey("UploadedByUserId")]
        public ApplicationUser UploadedBy { get; set; }

        [Required]
        [MaxLength(255)]
        public string FileName { get; set; }

        [Required]
        [MaxLength(1000)]
        public string FilePath { get; set; }

        [Required]
        [MaxLength(50)]
        public string FileType { get; set; } = "Other"; // Prescription, Lab Result, Photo, Other

        [Required]
        [MaxLength(100)]
        public string MimeType { get; set; }

        public long FileSizeBytes { get; set; }

        // Pharmacist request fields
        public string? RequestedByUserId { get; set; }

        [ForeignKey("RequestedByUserId")]
        public ApplicationUser? RequestedBy { get; set; }

        [MaxLength(500)]
        public string? RequestNote { get; set; }

        public bool IsRequestFulfilled { get; set; } = false;

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}