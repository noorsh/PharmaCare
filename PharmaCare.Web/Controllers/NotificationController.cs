using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaCare.Business.Services.Interfaces;
using PharmaCare.Services.Interfaces;

namespace PharmaCare.MVC.Controllers
{
    [Authorize(Roles = "Pharmacist,Admin")]
    public class NotificationController : Controller
    {
        private readonly IConsultationService _consultationService;
        private readonly IInventoryService _inventoryService;

        public NotificationController(
            IConsultationService consultationService,
            IInventoryService inventoryService)
        {
            _consultationService = consultationService;
            _inventoryService = inventoryService;
        }

        // GET: /Notification/GetNotifications
        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var notifications = new List<object>();

            // New pending consultations
            var pending = await _consultationService.GetPendingConsultationsWithDetailsAsync();
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

            // Low stock alerts
            var lowStock = await _inventoryService.GetLowStockItemsAsync();
            foreach (var item in lowStock.Take(3))
            {
                notifications.Add(new
                {
                    type    = "inventory",
                    icon    = "inventory",
                    color   = "amber",
                    title   = "Low Stock Alert",
                    message = $"{item.MedicineName} — only {item.QuantityInStock} units left",
                    time    = "Now",
                    link    = Url.Action("Index", "Inventory")
                });
            }

            var totalCount = pendingList.Count + lowStock.Count();

            return Json(new
            {
                count         = totalCount,
                notifications = notifications.Take(8)
            });
        }

        private static string GetTimeAgo(DateTime dateTime)
        {
            var diff = DateTime.UtcNow - dateTime;
            if (diff.TotalMinutes < 1)   return "Just now";
            if (diff.TotalMinutes < 60)  return $"{(int)diff.TotalMinutes}m ago";
            if (diff.TotalHours < 24)    return $"{(int)diff.TotalHours}h ago";
            return $"{(int)diff.TotalDays}d ago";
        }
    }
}