using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PharmaCare.Data.Models;
using PharmaCare.MVC.Models.ViewModels;
using PharmaCare.Services.Interfaces;

namespace PharmaCare.MVC.Controllers
{
    [Authorize(Roles = "Pharmacist,Admin")]
    public class PharmacistController : Controller
    {
        private readonly IConsultationService _consultationService;
        private readonly IPatientService _patientService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<PharmacistController> _logger;

        public PharmacistController(
            IConsultationService consultationService,
            IPatientService patientService,
            UserManager<ApplicationUser> userManager,
            ILogger<PharmacistController> logger)
        {
            _consultationService = consultationService;
            _patientService = patientService;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _userManager.FindByIdAsync(userId);

                // All active consultations
                var allActive = await _consultationService.GetPendingConsultationsWithDetailsAsync();
                var pending = allActive.Where(c => c.Status == "Pending").ToList();
                var underReview = allActive.Where(c => c.Status == "UnderReview").ToList();

                // Recently completed (last 5)
                var recentCompleted = await _consultationService.GetRecentConsultationsWithDetailsAsync(10);
                var completedList = recentCompleted
                    .Where(c => c.Status == "Completed")
                    .OrderByDescending(c => c.CompletedAt)
                    .Take(5)
                    .ToList();

                // Completed today
                var completedToday = recentCompleted
                    .Count(c => c.Status == "Completed" &&
                                c.CompletedAt.HasValue &&
                                c.CompletedAt.Value.Date == DateTime.UtcNow.Date);

                // Total unique patients served
                var allPatients = await _patientService.GetAllPatientsAsync();

                var viewModel = new PharmacistDashboardViewModel
                {
                    PharmacistFirstName  = user?.FirstName ?? "Pharmacist",
                    PendingCount         = pending.Count,
                    UnderReviewCount     = underReview.Count,
                    CompletedTodayCount  = completedToday,
                    TotalPatientsServed  = allPatients.Count(),
                    PendingConsultations = pending.Take(5),
                    RecentlyCompleted    = completedList
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading pharmacist dashboard");
                TempData["Error"] = "An error occurred while loading the dashboard.";
                return View(new PharmacistDashboardViewModel());
            }
        }
    }
}