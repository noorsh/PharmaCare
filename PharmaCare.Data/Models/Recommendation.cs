using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PharmaCare.Data.Models;

public class Recommendation
{
    [Key]
    public int RecommendationId { get; set; }

    [Required]
    public int ConsultationId { get; set; }

    [ForeignKey("ConsultationId")]
    public Consultation Consultation { get; set; }

    [Required]
    public string PharmacistId { get; set; }

    [ForeignKey("PharmacistId")]
    public ApplicationUser Pharmacist { get; set; }

    public int? InventoryId { get; set; }

    [ForeignKey("InventoryId")]
    public Inventory? Inventory { get; set; }

    [Required]
    [MaxLength(2000)]
    public string PharmacistNotes { get; set; }

    [MaxLength(500)]
    public string? Dosage { get; set; }

    [MaxLength(1000)]
    public string? Instructions { get; set; }

    [MaxLength(1000)]
    public string? Warnings { get; set; }

    public bool ReferToDoctor { get; set; } = false;

    [MaxLength(500)]
    public string? ReferralReason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
