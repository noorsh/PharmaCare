using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaCare.Business.Services.Interfaces;
using PharmaCare.Services.Interfaces;

namespace PharmaCare.MVC.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly IConsultationService _consultationService;
        private readonly IInventoryService    _inventoryService;
        private readonly IPatientService      _patientService;

        public NotificationController(
            IConsultationService consultationService,
            IInventoryService    inventoryService,
            IPatientService      patientService)
        {
            _consultationService = consultationService;
            _inventoryService    = inventoryService;
            _patientService      = patientService;
        }

        // ─────────────────────────────────────────────────────────────
        // PHARMACIST / ADMIN
        // GET: /Notification/GetNotifications
        // ─────────────────────────────────────────────────────────────
        [HttpGet]
        [Authorize(Roles = "Pharmacist,Admin")]
        public async Task<IActionResult> GetNotifications()
        {
            var notifications = new List<object>();

            var pending     = await _consultationService.GetPendingConsultationsWithDetailsAsync();
            var pendingList = pending.Where(c => c.Status == "Pending").ToList();

            foreach (var c in pendingList.Take(5))
            {
                var name = c.Patient?.User != null
                    ? $"{c.Patient.User.FirstName} {c.Patient.User.LastName}"
                    : "Unknown Patient";

                notifications.Add(new
                {
                    type    = "consultation",
                    icon    = "chat",
                    color   = "blue",
                    title   = "New Consultation",
                    message = $"{name} submitted a {c.SymptomSeverity?.ToLower() ?? "new"} consultation",
                    time    = GetTimeAgo(c.CreatedAt),
                    link    = Url.Action("Queue", "Consultation")
                });
            }

            var lowStock = await _inventoryService.GetLowStockItemsAsync();
            foreach (var item in lowStock.Take(3))
            {
                notifications.Add(new
                {
                    type    = "inventory",
                    icon    = "inventory",
                    color   = "amber",
                    title   = "Low Stock Alert",
                    message = $"{(string)item.MedicineName} — only {(int)item.QuantityInStock} units left",
                    time    = "Now",
                    link    = Url.Action("Index", "Inventory")
                });
            }

            return Json(new
            {
                count         = pendingList.Count + lowStock.Count(),
                notifications = notifications.Take(8)
            });
        }

        // ─────────────────────────────────────────────────────────────
        // PATIENT
        // GET: /Notification/GetPatientNotifications
        // ─────────────────────────────────────────────────────────────
        [HttpGet]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> GetPatientNotifications()
        {
            var userId  = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var patient = await _patientService.GetPatientByUserIdAsync(userId);

            if (patient == null)
                return Json(new { count = 0, notifications = Array.Empty<object>() });

            var consultations = await _consultationService
                .GetConsultationsByPatientWithDetailsAsync(patient.PatientId);

            var notifications = new List<object>();

            foreach (var c in consultations
                .Where(c => c.Status == "Completed" || c.Status == "UnderReview")
                .OrderByDescending(c => c.CompletedAt ?? c.ReviewedAt ?? c.CreatedAt))
            {
                if (c.Status == "Completed" && c.CompletedAt.HasValue)
                {
                    var pharmacistName = c.Pharmacist != null
                        ? $"{c.Pharmacist.FirstName} {c.Pharmacist.LastName}"
                        : "your pharmacist";

                    notifications.Add(new
                    {
                        type    = "completed",
                        icon    = "verified_user",
                        color   = "green",
                        title   = "Consultation Reviewed",
                        message = $"Reviewed by {pharmacistName}. Tap to see your recommendation.",
                        time    = GetTimeAgo(c.CompletedAt.Value),
                        link    = Url.Action("Details", "Consultation", new { id = c.ConsultationId })
                    });
                }
                else if (c.Status == "UnderReview" && c.ReviewedAt.HasValue)
                {
                    notifications.Add(new
                    {
                        type    = "underreview",
                        icon    = "manage_search",
                        color   = "blue",
                        title   = "Under Review",
                        message = "A pharmacist is reviewing your consultation now.",
                        time    = GetTimeAgo(c.ReviewedAt.Value),
                        link    = Url.Action("Details", "Consultation", new { id = c.ConsultationId })
                    });
                }
            }

            return Json(new
            {
                count         = notifications.Count,
                notifications = notifications.Take(8)
            });
        }

        // ─────────────────────────────────────────────────────────────
        private static string GetTimeAgo(DateTime dateTime)
        {
            var diff = DateTime.UtcNow - dateTime;
            if (diff.TotalMinutes < 1)  return "Just now";
            if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes}m ago";
            if (diff.TotalHours   < 24) return $"{(int)diff.TotalHours}h ago";
            return $"{(int)diff.TotalDays}d ago";
        }
    }
}