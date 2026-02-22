using PharmaCare.Data.Models;

namespace PharmaCare.MVC.Models.ViewModels
{
    public class PharmacistDashboardViewModel
    {
        // Stats
        public int PendingCount { get; set; }
        public int UnderReviewCount { get; set; }
        public int CompletedTodayCount { get; set; }
        public int TotalPatientsServed { get; set; }

        // Pharmacist name for greeting
        public string PharmacistFirstName { get; set; } = "";

        // Table: up to 5 pending consultations
        public IEnumerable<Consultation> PendingConsultations { get; set; } = new List<Consultation>();

        // Sidebar: up to 5 recently completed
        public IEnumerable<Consultation> RecentlyCompleted { get; set; } = new List<Consultation>();
    }
}