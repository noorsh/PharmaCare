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
        [Authorize(Roles = "Pharmacist,Admin")]
public async Task<IActionResult> Patients(string? search, string? tab)
{
    try
    {
        var allPatients = await _patientService.GetAllPatientsWithDetailsAsync();

        // Apply search
        if (!string.IsNullOrWhiteSpace(search))
        {
            var q = search.ToLower();
            allPatients = allPatients.Where(p =>
                (p.User?.FirstName + " " + p.User?.LastName).ToLower().Contains(q) ||
                (p.City ?? "").ToLower().Contains(q) ||
                (p.User?.PhoneNumber ?? "").Contains(q));
        }

        // Apply tab filter
        if (tab == "Flagged")
            allPatients = allPatients.Where(p => p.IsPregnant || p.IsBreastfeeding);
        else if (tab == "HighRisk")
            allPatients = allPatients.Where(p => p.Allergies.Any() || p.HasKidneyDisease || p.HasLiverDisease);

        var rows = allPatients.Select(p => new PatientRowViewModel
        {
            PatientId         = p.PatientId,
            FullName          = $"{p.User?.FirstName} {p.User?.LastName}",
            Initials          = $"{p.User?.FirstName?[0]}{p.User?.LastName?[0]}".ToUpper(),
            City              = p.City,
            BloodType         = p.BloodType.HasValue
                                    ? GetBloodTypeDisplay(p.BloodType.Value)
                                    : null,
            Age               = p.DateOfBirth != default
                                    ? (int)((DateTime.UtcNow - p.DateOfBirth).TotalDays / 365.25)
                                    : 0,
            IsPregnant        = p.IsPregnant,
            IsBreastfeeding   = p.IsBreastfeeding,
            IsSmoker          = p.SmokingStatus == Enums.SmokingStatus.Current,
            HasKidneyDisease  = p.HasKidneyDisease,
            HasLiverDisease   = p.HasLiverDisease,
            ConsultationCount = p.Consultations.Count,
            AllergyCount      = p.Allergies.Count,
            MedicationCount   = p.CurrentMedications.Count,
            Allergies         = p.Allergies,
            CurrentMedications = p.CurrentMedications,
            MedicalHistories  = p.MedicalHistories.Where(m => m.IsActive),
            LastConsultation  = p.Consultations.OrderByDescending(c => c.CreatedAt).FirstOrDefault()
        }).ToList();

        
        var viewModel = new PatientListViewModel
        {
            Patients     = rows,
            TotalCount   = rows.Count,
            SearchQuery  = search,
            ActiveTab    = tab ?? "All"
        };

        return View(viewModel);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error loading patient list");
        TempData["Error"] = "An error occurred while loading patients.";
        return View(new PatientListViewModel());
    }
}

    private static string GetBloodTypeDisplay(Enums.BloodType bloodType) => bloodType switch
    {
        Enums.BloodType.A_Positive  => "A+",
        Enums.BloodType.A_Negative  => "A-",
        Enums.BloodType.B_Positive  => "B+",
        Enums.BloodType.B_Negative  => "B-",
        Enums.BloodType.AB_Positive => "AB+",
        Enums.BloodType.AB_Negative => "AB-",
        Enums.BloodType.O_Positive  => "O+",
        Enums.BloodType.O_Negative  => "O-",
        _ => "—"
    };
    }
}