namespace PharmaCare.MVC.Models.ViewModels
{
    public class ReportsViewModel
    {
        // ── Filters (bound from form) ─────────────────────────────────────
        public string   ReportType  { get; set; } = "consultations"; // consultations | users | inventory
        public DateTime DateFrom    { get; set; } = DateTime.UtcNow.AddDays(-30);
        public DateTime DateTo      { get; set; } = DateTime.UtcNow;
        public string?  StatusFilter { get; set; }

        // ── Consultation Report ───────────────────────────────────────────
        public int TotalConsultations    { get; set; }
        public int CompletedConsultations { get; set; }
        public int PendingConsultations  { get; set; }
        public int CancelledConsultations { get; set; }
        public int UnderReviewConsultations { get; set; }
        public double AvgCompletionHours { get; set; }

        public List<ConsultationReportRow> ConsultationRows { get; set; } = new();

        // ── User Report ───────────────────────────────────────────────────
        public int TotalUsers      { get; set; }
        public int TotalPatients   { get; set; }
        public int TotalPharmacists { get; set; }
        public int TotalAdmins     { get; set; }
        public int NewUsersInRange { get; set; }

        public List<UserReportRow> UserRows { get; set; } = new();

        // ── Inventory Report ──────────────────────────────────────────────
        public int TotalMedications  { get; set; }
        public int LowStockCount     { get; set; }
        public int OutOfStockCount   { get; set; }

        public List<InventoryReportRow> InventoryRows { get; set; } = new();

        // ── Chart data (JSON strings for JS) ─────────────────────────────
        public string ConsultationsByDayJson   { get; set; } = "[]";
        public string ConsultationsByStatusJson { get; set; } = "[]";
    }

    public class ConsultationReportRow
    {
        public int      ConsultationId  { get; set; }
        public string   PatientName     { get; set; }
        public string   PatientEmail    { get; set; }
        public string   Symptoms        { get; set; }
        public string   Severity        { get; set; }
        public string   Status          { get; set; }
        public string?  PharmacistName  { get; set; }
        public DateTime CreatedAt       { get; set; }
        public DateTime? CompletedAt    { get; set; }
        public double?  HoursToComplete { get; set; }
    }

    public class UserReportRow
    {
        public string   UserId    { get; set; }
        public string   FullName  { get; set; }
        public string   Email     { get; set; }
        public string   Role      { get; set; }
        public string?  Phone     { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class InventoryReportRow
    {
        public int     InventoryId    { get; set; }
        public string  MedicineName   { get; set; }
        public string? Category       { get; set; }
        public int     Quantity        { get; set; }
        public string  StockStatus    { get; set; }  // OK | Low | Out
        public string? ExpiryDate     { get; set; }
    }
}