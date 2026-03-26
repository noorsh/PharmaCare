
namespace PharmaCare.MVC.Models.ViewModels
{
    public class PharmacistHistoryViewModel
    {
        // Filters
        public string? SearchQuery { get; set; }
        public string? StatusFilter { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        // Pagination
        public int CurrentPage { get; set; } = 1;
        public int HistoryPageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / HistoryPageSize);

        // Data
        public IEnumerable<PharmacistConsultationRow> Consultations { get; set; } =
            new List<PharmacistConsultationRow>();
    }

    public class PharmacistConsultationRow
    {
        public int ConsultationId { get; set; }
        public string PatientName { get; set; } = "";
        public string PatientInitials { get; set; } = "";
        public string? PatientCity { get; set; }
        public string Symptoms { get; set; } = "";
        public string? SymptomSeverity { get; set; }
        public string Status { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string PharmacistName { get; set; } = "—";

        public string SeverityClass => SymptomSeverity switch
        {
            "Severe" => "text-red-600 bg-red-50 dark:bg-red-900/20",
            "Moderate" => "text-amber-600 bg-amber-50 dark:bg-amber-900/20",
            _ => "text-emerald-600 bg-emerald-50 dark:bg-emerald-900/20"
        };

        public string SeverityDot => SymptomSeverity switch
        {
            "Severe" => "bg-red-600",
            "Moderate" => "bg-amber-600",
            _ => "bg-emerald-600"
        };

        public string StatusClass => Status switch
        {
            "Completed" => "text-emerald-600 bg-emerald-50 dark:bg-emerald-900/20",
            _ => "text-slate-500 bg-slate-100 dark:bg-slate-800"
        };
    }
}