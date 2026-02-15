using PharmaCare.Data.Models;

namespace PharmaCare.MVC.Models.ViewModels
{
    public class PatientProfileViewModel
    {
        public Patient Patient { get; set; }
        public ApplicationUser User { get; set; }
        public IEnumerable<MedicalHistory> MedicalHistories { get; set; }
        public IEnumerable<Allergy> Allergies { get; set; }
        public IEnumerable<CurrentMedication> CurrentMedications { get; set; }
        public IEnumerable<Consultation> RecentConsultations { get; set; }
    }
}