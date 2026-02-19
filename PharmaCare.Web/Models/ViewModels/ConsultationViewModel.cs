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
        public Consultation Consultation { get; set; }
        public AIAssessment? AIAssessment { get; set; }

        [System.ComponentModel.DataAnnotations.MaxLength(2000)]
        public string? PharmacistNotes { get; set; }
    }
}