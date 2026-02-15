namespace PharmaCare.MVC.Models.ViewModels
{
    public class PatientDashboardViewModel
    {
        // Patient Info
        public string PatientName { get; set; }
        public int? Age { get; set; }
        public string BloodType { get; set; }
        public int ProfileCompletionPercentage { get; set; }

        // Stats
        public int ActiveMedicationsCount { get; set; }
        public bool RecentMedicationAdded { get; set; }
        public int AllergiesCount { get; set; }
        public int SevereAllergiesCount { get; set; }
        public int MedicalConditionsCount { get; set; }
        public int TotalConsultationsCount { get; set; }
        public int PendingConsultationsCount { get; set; }

        // Recent Activities
        public List<ActivityItem> RecentActivities { get; set; } = new();

        // Health Alerts
        public List<HealthAlert> HealthAlerts { get; set; } = new();
    }

    public class ActivityItem
    {
        public string Icon { get; set; } // Material Symbol name
        public string Title { get; set; }
        public string Description { get; set; }
        public string ActionLink { get; set; }
    }

    public class HealthAlert
    {
        public string Icon { get; set; } // Material Symbol name (priority_high, info, etc.)
        public string Message { get; set; }
    }
}