using PharmaCare.Data.Models;

namespace PharmaCare.MVC.Models.ViewModels
{
    public class PatientListViewModel
    {
        public IEnumerable<PatientRowViewModel> Patients { get; set; } = new List<PatientRowViewModel>();
        public int TotalCount { get; set; }
        public string? SearchQuery { get; set; }
        public string? ActiveTab { get; set; } = "All";
    }

    public class PatientRowViewModel
    {
        public int PatientId { get; set; }
        public string FullName { get; set; } = "";
        public string Initials { get; set; } = "";
        public string? City { get; set; }
        public string? BloodType { get; set; }
        public int Age { get; set; }
        public bool IsPregnant { get; set; }
        public bool IsBreastfeeding { get; set; }
        public bool IsSmoker { get; set; }
        public bool HasKidneyDisease { get; set; }
        public bool HasLiverDisease { get; set; }
        public int ConsultationCount { get; set; }
        public int AllergyCount { get; set; }
        public int MedicationCount { get; set; }

        // Expanded panel data
        public IEnumerable<Allergy> Allergies { get; set; } = new List<Allergy>();
        public IEnumerable<CurrentMedication> CurrentMedications { get; set; } = new List<CurrentMedication>();
        public IEnumerable<MedicalHistory> MedicalHistories { get; set; } = new List<MedicalHistory>();
        public Consultation? LastConsultation { get; set; }
    }
}