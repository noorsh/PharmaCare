using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace PharmaCare.Data.Models
{
    [Table("pharmacists")]
    public class Pharmacist
    {
        [Key]
        [Column("pharmacist_id")]
        public int PharmacistId { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [Column("first_name")]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [Column("last_name")]
        [StringLength(50)]
        public string LastName { get; set; }

        [Required]
        [Column("phone")]
        [StringLength(20)]
        public string Phone { get; set; }

        [Required]
        [Column("license_number")]
        [StringLength(50)]
        public string LicenseNumber { get; set; }

        [Column("pharmacy_name")]
        [StringLength(100)]
        public string PharmacyName { get; set; }

        [Column("pharmacy_address")]
        [StringLength(200)]
        public string PharmacyAddress { get; set; }

        [Column("specialization")]
        [StringLength(100)]
        public string Specialization { get; set; }

        [Column("years_of_experience")]
        public int? YearsOfExperience { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

       // public ICollection<Consultation> Consultations { get; set; }
    }
}
