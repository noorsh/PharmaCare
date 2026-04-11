namespace PharmaCare.MVC.Models.ViewModels
{
    public class ActivityLogViewModel
    {
        public string PatientName { get; set; }

        public List<ActivityGroup> Groups { get; set; } = new();

        // Summary counts
        public int TotalActivities     { get; set; }
        public int ConsultationCount   { get; set; }
        public int MedicationCount     { get; set; }
        public int AllergyCount        { get; set; }
        public int MedicalHistoryCount { get; set; }
    }

    public class ActivityGroup
    {
        public string Label { get; set; }        // e.g. "Today", "Yesterday", "March 2025"
        public List<ActivityLogItem> Items { get; set; } = new();
    }

    public class ActivityLogItem
    {
        public string Icon        { get; set; }
        public string IconBg      { get; set; }  // Tailwind bg class  e.g. "bg-primary/10"
        public string IconColor   { get; set; }  // Tailwind text class e.g. "text-primary"
        public string Category    { get; set; }  // "Consultation" | "Medication" | "Allergy" | "Medical History"
        public string Title       { get; set; }
        public string Description { get; set; }
        public string TimeAgo     { get; set; }
        public DateTime Date      { get; set; }
        public string? ActionLink { get; set; }
        public string? BadgeText  { get; set; }  // optional status badge e.g. "Completed"
        public string? BadgeCss   { get; set; }  // Tailwind classes for the badge
    }
}
