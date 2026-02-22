using System.ComponentModel.DataAnnotations;
using PharmaCare.Data.Models;

namespace PharmaCare.MVC.Models.ViewModels
{
    public class ConsultationViewModel
    {
        public Consultation Consultation { get; set; }
        public AIAssessment? AIAssessment { get; set; }
        public bool IsPatientView { get; set; } = true;
    }

    public class PharmacistQueueViewModel
    {
        public IEnumerable<Consultation> PendingConsultations { get; set; } = new List<Consultation>();
        public IEnumerable<Consultation> UnderReviewConsultations { get; set; } = new List<Consultation>();
        public IEnumerable<Consultation> RecentlyCompleted { get; set; } = new List<Consultation>();
        public int TotalPending { get; set; }
        public int TotalUnderReview { get; set; }
    }

   
        public class ReviewConsultationViewModel
        {
            // ─── Read-only data (populated by controller) ───────────────────

            public Consultation Consultation { get; set; }
            public AIAssessment? AIAssessment { get; set; }
            public Patient? Patient { get; set; }

            // Patient medical context for the sidebar
            public IEnumerable<Allergy> Allergies { get; set; } = new List<Allergy>();
            public IEnumerable<CurrentMedication> CurrentMedications { get; set; } = new List<CurrentMedication>();
            public IEnumerable<MedicalHistory> MedicalHistory { get; set; } = new List<MedicalHistory>();

            // Available medications for the dropdown — from Inventory
            public IEnumerable<Inventory> AvailableMedications { get; set; } = new List<Inventory>();

            [Required(ErrorMessage = "Please provide your clinical notes.")]
            [MaxLength(2000)]
            [Display(Name = "Pharmacist Notes")]
            public string PharmacistNotes { get; set; } = "";

           
            [Display(Name = "Recommended Medication")]
            public int? InventoryId { get; set; }

            [MaxLength(500)]
            [Display(Name = "Dosage Instructions")]
            public string? Dosage { get; set; }

            [MaxLength(1000)]
            [Display(Name = "Usage Instructions")]
            public string? Instructions { get; set; }

            [MaxLength(1000)]
            [Display(Name = "Warnings / Precautions")]
            public string? Warnings { get; set; }

            [Display(Name = "Refer to Doctor")]
            public bool ReferToDoctor { get; set; } = false;

            [MaxLength(500)]
            [Display(Name = "Referral Reason")]
            public string? ReferralReason { get; set; }
        }
    
}