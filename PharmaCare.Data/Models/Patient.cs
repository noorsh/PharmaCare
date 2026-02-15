using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmaCare.Data.Models
{
    public class Patient 
    {
        [Key]
        public int PatientId { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }  // Make nullable

        // Personal Information
        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [MaxLength(10)]
        public string Gender { get; set; } // Male, Female, Other

        [MaxLength(200)]
        public string? Address { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }

        [MaxLength(20)]
        public string? EmergencyContact { get; set; }

        // Medical Profile
        [Column(TypeName = "decimal(5,2)")]
        public decimal? Height { get; set; } // in cm

        [Column(TypeName = "decimal(5,2)")]
        public decimal? Weight { get; set; } // in kg

        public Enums.BloodType? BloodType { get; set; }


        public Enums.SmokingStatus? SmokingStatus { get; set; }
        public Enums.AlcoholConsumption? AlcoholConsumption { get; set; }
        public Enums.ExerciseFrequency? ExerciseFrequency { get; set; }


        public bool IsPregnant { get; set; } = false;

        public bool IsBreastfeeding { get; set; } = false;

        // Critical Safety Flags
        [Display(Name = "Has Chronic Kidney Disease")]
        public bool HasKidneyDisease { get; set; } = false;

        [Display(Name = "Has Chronic Liver Disease")]
        public bool HasLiverDisease { get; set; } = false;

        [Display(Name = "Has Known Drug Allergies")]
        public bool HasDrugAllergies { get; set; } = false;

        [MaxLength(500)]
        [Display(Name = "Drug Allergy Details")]
        public string? DrugAllergyDetails { get; set; }

        // Additional Notes
        [MaxLength(1000)]
        public string? AdditionalNotes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        public ICollection<MedicalHistory> MedicalHistories { get; set; } = new List<MedicalHistory>();
        public ICollection<Allergy> Allergies { get; set; } = new List<Allergy>();
        public ICollection<CurrentMedication> CurrentMedications { get; set; } = new List<CurrentMedication>();
        public ICollection<Consultation> Consultations { get; set; } = new List<Consultation>();
    }
}