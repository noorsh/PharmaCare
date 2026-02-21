namespace PharmaCare.MVC.Models.ViewModels
{
    public class ConsultationHistoryViewModel
    {
        // Stats
        public int TotalConsultations { get; set; }
        public int PendingCount { get; set; }
        public int CompletedCount { get; set; }

        // Filters
        public string? StatusFilter { get; set; }
        public string? SearchQuery { get; set; }

        // Pagination
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 8;
        public int TotalResults { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalResults / PageSize);

        // Data
        public IEnumerable<ConsultationSummary> Consultations { get; set; } = new List<ConsultationSummary>();
    }

    public class ConsultationSummary
    {
        public int ConsultationId { get; set; }
        public string Symptoms { get; set; } = "";
        public string? SymptomDuration { get; set; }
        public string? SymptomSeverity { get; set; }
        public string Status { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public bool HasUrgentFlag { get; set; }

        public string SeverityBadgeColor => SymptomSeverity switch
        {
            "Severe"   => "red",
            "Moderate" => "amber",
            "Mild"     => "emerald",
            _          => "slate"
        };

        public string StatusBadgeColor => Status switch
        {
            "Completed"   => "emerald",
            "Pending"     => "amber",
            "UnderReview" => "blue",
            "Cancelled"   => "slate",
            _             => "slate"
        };

        public string StatusLabel => Status switch
        {
            "UnderReview" => "In Progress",
            _             => Status
        };
    }
}