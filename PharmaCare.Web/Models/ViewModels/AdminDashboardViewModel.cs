using PharmaCare.Data.Models;

namespace PharmaCare.MVC.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        // Statistics
        public int TotalUsers { get; set; }
        public int UserGrowthPercent { get; set; }
        public int ActivePatients { get; set; }
        public int PatientGrowthPercent { get; set; }
        public int TotalPharmacists { get; set; }
        public int ActivePharmacists { get; set; }
        public int InactivePharmacists { get; set; }
        public int PharmacistGrowthPercent { get; set; }
        public int PendingConsultations { get; set; }
        public decimal SystemUptime { get; set; }

        // Today's Activity
        public int TodayNewUsers { get; set; }
        public string UserSignupTrend { get; set; } = string.Empty;
        public int TodayConsultations { get; set; }

        // Recent Users (for table)
        public List<UserSummary> RecentUsers { get; set; } = new();

        // Recent Activities (for timeline)
        public List<AdminActivity> RecentActivities { get; set; } = new();

        // System Alerts
        public List<SystemAlert> SystemAlerts { get; set; } = new();
    }

    public class UserSummary
    {
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string Role { get; set; } = string.Empty; // "Patient", "Pharmacist", "Admin"
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string LastActive { get; set; } = string.Empty;
    }

    public class AdminActivity
    {
        public string AdminName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // "Create", "Update", "Delete", "Approve"
        public string TimeAgo { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public class SystemAlert
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty; // "High", "Medium", "Low"
        public DateTime CreatedAt { get; set; }
    }
}