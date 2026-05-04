using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PharmaCare.Data.Models;
using PharmaCare.MVC.Extensions;
using PharmaCare.MVC.Models.ViewModels;
using PharmaCare.Services.Interfaces;

namespace PharmaCare.MVC.Controllers
{
    [Authorize]
    public class PatientController : Controller
    {
        private readonly IPatientService _patientService;
        private readonly IMedicalHistoryService _medicalHistoryService;
        private readonly IAllergyService _allergyService;
        private readonly ICurrentMedicationService _currentMedicationService;
        private readonly IConsultationService _consultationService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<PatientController> _logger;
        private readonly IWebHostEnvironment _env;

        public PatientController(
            IPatientService patientService,
            IMedicalHistoryService medicalHistoryService,
            IAllergyService allergyService,
            ICurrentMedicationService currentMedicationService,
            IConsultationService consultationService,
            UserManager<ApplicationUser> userManager,
            ILogger<PatientController> logger,
            IWebHostEnvironment env)
        {
            _patientService = patientService;
            _medicalHistoryService = medicalHistoryService;
            _allergyService = allergyService;
            _currentMedicationService = currentMedicationService;
            _consultationService = consultationService;
            _userManager = userManager;
            _logger = logger;
            _env = env;
           
        }

        // GET: Patient/Profile
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> Profile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var patient = await _patientService.GetPatientByUserIdAsync(userId);

            if (patient == null)
                return NotFound();

            var medicalHistories = (await _medicalHistoryService.GetMedicalHistoriesByPatientIdAsync(patient.PatientId)).ToList();
            var allergies        = (await _allergyService.GetAllergiesByPatientIdAsync(patient.PatientId)).ToList();
            var medications      = (await _currentMedicationService.GetCurrentMedicationsByPatientIdAsync(patient.PatientId)).ToList();

            ViewBag.Age               = CalculateAge(patient.DateOfBirth);
            ViewBag.ProfileCompletion = CalculateProfileCompletion(patient);
            ViewBag.MedicalHistoryCount = medicalHistories.Count;
            ViewBag.AllergiesCount    = allergies.Count;
            ViewBag.MedicationsCount  = medications.Count;

            return View(patient);
        }

        // GET: Patient/Edit
        public async Task<IActionResult> Edit()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return RedirectToAction("Login", "Account");

                var patient = await _patientService.GetPatientByUserIdAsync(user.Id);
                if (patient == null)
                {
                    TempData["Error"] = "Patient profile not found.";
                    return RedirectToAction("Index", "Home");
                }

                return View(patient);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading patient for editing");
                TempData["Error"] = "An error occurred while loading your profile.";
                return RedirectToAction("Profile");
            }
        }

        // POST: Patient/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Patient patient, IFormFile? ProfilePhoto)
        {
            // Remove navigation properties from validation
            ModelState.Remove("User");
            ModelState.Remove("Consultations");
            ModelState.Remove("Allergies");
            ModelState.Remove("CurrentMedications");
            ModelState.Remove("MedicalHistories");

            if (!ModelState.IsValid)
                return View(patient);

            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return RedirectToAction("Login", "Account");

                var existingPatient = await _patientService.GetPatientByUserIdAsync(user.Id);
                if (existingPatient == null || existingPatient.PatientId != patient.PatientId)
                    return Forbid();

                // Handle photo upload
                if (ProfilePhoto != null && ProfilePhoto.Length > 0)
                {
                    var newPhotoUrl = await _patientService.SaveProfilePhotoAsync(ProfilePhoto, _env.WebRootPath);
                    if (newPhotoUrl == null)
                    {
                        ModelState.AddModelError("ProfilePhoto", "Invalid photo. Please use JPG or PNG under 5MB.");
                        return View(patient);
                    }

                    _patientService.DeleteProfilePhoto(user.ProfilePhotoUrl, _env.WebRootPath);
                    user.ProfilePhotoUrl = newPhotoUrl;
                    await _userManager.UpdateAsync(user);
                }

                // Ensure UserId doesn't change
                patient.UserId = existingPatient.UserId;

                var success = await _patientService.UpdatePatientAsync(patient);

                if (success)
                {
                    TempData["Success"] = "Profile updated successfully!";
                    return RedirectToAction(nameof(Profile));
                }

                ModelState.AddModelError("", "Unable to update profile.");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating patient profile");
                ModelState.AddModelError("", "An error occurred while updating your profile.");
            }

            return View(patient);
        }

        // GET: Patient/List (Admin/Pharmacist only)
        [Authorize(Roles = "Admin,Pharmacist")]
        public async Task<IActionResult> List()
        {
            try
            {
                var patients = await _patientService.GetAllPatientsAsync();
                return View(patients);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading patient list");
                TempData["Error"] = "An error occurred while loading patients.";
                return View(new List<Patient>());
            }
        }

        // GET: Patient/Details/5 (Admin/Pharmacist only)
        [Authorize(Roles = "Admin,Pharmacist")]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var patient = await _patientService.GetPatientByIdAsync(id);
                if (patient == null)
                    return NotFound();

                var viewModel = new PatientProfileViewModel
                {
                    Patient            = patient,
                    User               = patient.User,
                    MedicalHistories   = await _medicalHistoryService.GetMedicalHistoriesByPatientIdAsync(id),
                    Allergies          = await _allergyService.GetAllergiesByPatientIdAsync(id),
                    CurrentMedications = await _currentMedicationService.GetActiveMedicationsByPatientIdAsync(id),
                    RecentConsultations = await _consultationService.GetConsultationsByPatientIdAsync(id)
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading patient {id}");
                return NotFound();
            }
        }

        // GET: Patient/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var patient = await _patientService.GetPatientByUserIdAsync(userId);

            if (patient == null)
                return NotFound();

            var medicalHistories = (await _medicalHistoryService.GetMedicalHistoriesByPatientIdAsync(patient.PatientId)).ToList();
            var allergies        = (await _allergyService.GetAllergiesByPatientIdAsync(patient.PatientId)).ToList();
            var medications      = (await _currentMedicationService.GetCurrentMedicationsByPatientIdAsync(patient.PatientId)).ToList();
            var consultations    = (await _consultationService.GetConsultationsByPatientWithDetailsAsync(patient.PatientId)).ToList();

            var viewModel = new PatientDashboardViewModel
            {
                PatientName                 = patient.User.FirstName,
                Age                         = CalculateAge(patient.DateOfBirth),
                BloodType                   = patient.BloodType?.GetDisplayName(),
                ProfileCompletionPercentage = CalculateProfileCompletion(patient),
                MedicalConditionsCount      = medicalHistories.Count,
                ActiveMedicationsCount      = medications.Count,
                RecentMedicationAdded       = medications.Any(m => m.StartDate >= DateTime.Now.AddDays(-7)),
                AllergiesCount              = allergies.Count,
                SevereAllergiesCount        = allergies.Count(a =>
                    a.Severity.GetDisplayName() == Enums.AllergySeverity.Severe.GetDisplayName() ||
                    a.Severity.GetDisplayName() == Enums.AllergySeverity.LifeThreatening.GetDisplayName()),
                TotalConsultationsCount     = consultations.Count,
                PendingConsultationsCount   = consultations.Count(c => c.Status == "Pending" || c.Status == "UnderReview"),
                RecentActivities            = GetRecentActivities(medications, medicalHistories, allergies, consultations),
                HealthAlerts                = GetHealthAlerts(patient, medicalHistories)
            };

            return View(viewModel);
        }

        // GET: Patient/ActivityLog
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> ActivityLog()
        {
            var userId  = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var patient = await _patientService.GetPatientByUserIdAsync(userId);

            if (patient == null)
                return NotFound();

            var medications   = (await _currentMedicationService.GetCurrentMedicationsByPatientIdAsync(patient.PatientId)).ToList();
            var histories     = (await _medicalHistoryService.GetMedicalHistoriesByPatientIdAsync(patient.PatientId)).ToList();
            var allergies     = (await _allergyService.GetAllergiesByPatientIdAsync(patient.PatientId)).ToList();
            var consultations = (await _consultationService.GetConsultationsByPatientWithDetailsAsync(patient.PatientId)).ToList();

            var viewModel = BuildActivityLog(patient, medications, histories, allergies, consultations);
            return View(viewModel);
        }

        // GET: Patient/DownloadAllergiesPDF
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> DownloadAllergiesPDF()
        {
            try
            {
                var userId  = _userManager.GetUserId(User);
                var patient = await _patientService.GetPatientByUserIdAsync(userId);
                if (patient == null)
                {
                    TempData["Error"] = "Patient profile not found.";
                    return RedirectToAction("Allergies");
                }

                TempData["Info"] = "PDF download feature coming soon.";
                return RedirectToAction("Allergies");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating PDF");
                TempData["Error"] = "An error occurred while generating the PDF.";
                return RedirectToAction("Allergies");
            }
        }

        // GET: Patient/PrintWalletCard
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> PrintWalletCard()
        {
            try
            {
                var userId  = _userManager.GetUserId(User);
                var patient = await _patientService.GetPatientByUserIdAsync(userId);
                if (patient == null)
                {
                    TempData["Error"] = "Patient profile not found.";
                    return RedirectToAction("Allergies");
                }

                TempData["Info"] = "Wallet card feature coming soon.";
                return RedirectToAction("Allergies");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating wallet card");
                TempData["Error"] = "An error occurred while generating the wallet card.";
                return RedirectToAction("Allergies");
            }
        }

        // ─── Private Helpers ─────────────────────────────────────────────────────────

        private int CalculateAge(DateTime dateOfBirth)
        {
            var today = DateTime.Today;
            var age   = today.Year - dateOfBirth.Year;
            if (dateOfBirth.Date > today.AddYears(-age)) age--;
            return age;
        }

        private int CalculateAge(DateTime? dateOfBirth)
        {
            if (!dateOfBirth.HasValue) return 0;
            return CalculateAge(dateOfBirth.Value);
        }

        private int CalculateProfileCompletion(Patient patient)
        {
            int totalFields  = 15;
            int filledFields = 0;

            if (!string.IsNullOrEmpty(patient.User?.FirstName))                         filledFields++;
            if (!string.IsNullOrEmpty(patient.User?.LastName))                          filledFields++;
            if (!string.IsNullOrEmpty(patient.User?.Email))                             filledFields++;
            if (!string.IsNullOrEmpty(patient.User?.PhoneNumber))                       filledFields++;
            if (!string.IsNullOrEmpty(patient.AdditionalNotes))                         filledFields++;
            if (!string.IsNullOrEmpty(patient.Address))                                 filledFields++;
            if (!string.IsNullOrEmpty(patient.City))                                    filledFields++;
            if (patient.Height != null)                                                  filledFields++;
            if (patient.Weight != null)                                                  filledFields++;
            if (patient.DateOfBirth != default)                                          filledFields++;
            if (!string.IsNullOrEmpty(patient.Gender))                                  filledFields++;
            if (!string.IsNullOrEmpty(patient.BloodType.GetDisplayName()))              filledFields++;
            if (!string.IsNullOrEmpty(patient.SmokingStatus.GetDisplayName()))          filledFields++;
            if (!string.IsNullOrEmpty(patient.AlcoholConsumption.GetDisplayName()))     filledFields++;
            if (!string.IsNullOrEmpty(patient.ExerciseFrequency.GetDisplayName()))      filledFields++;

            return (int)((double)filledFields / totalFields * 100);
        }

        private List<ActivityItem> GetRecentActivities(
            List<CurrentMedication> medications,
            List<MedicalHistory>    histories,
            List<Allergy>           allergies,
            List<Consultation>      consultations)
        {
            var activities = new List<(DateTime Date, ActivityItem Item)>();

            foreach (var c in consultations.OrderByDescending(c => c.CreatedAt).Take(3))
            {
                string icon, title, description;
                DateTime date;

                if (c.Status == "Completed" && c.CompletedAt.HasValue)
                {
                    var pharmacist = c.Pharmacist != null
                        ? $"{c.Pharmacist.FirstName} {c.Pharmacist.LastName}"
                        : "pharmacist";
                    icon        = "verified_user";
                    title       = "Consultation reviewed";
                    description = $"Reviewed by {pharmacist} • {c.CompletedAt.Value:MMM dd}";
                    date        = c.CompletedAt.Value;
                }
                else if (c.Status == "UnderReview" && c.ReviewedAt.HasValue)
                {
                    icon        = "manage_search";
                    title       = "Consultation under review";
                    description = $"A pharmacist is reviewing your case • {c.ReviewedAt.Value:MMM dd}";
                    date        = c.ReviewedAt.Value;
                }
                else
                {
                    icon        = "chat";
                    title       = "Consultation submitted";
                    description = $"{c.SymptomSeverity ?? "General"} • {c.CreatedAt:MMM dd}";
                    date        = c.CreatedAt;
                }

                activities.Add((date, new ActivityItem
                {
                    Icon        = icon,
                    Title       = title,
                    Description = description,
                    ActionLink  = Url.Action("Details", "Consultation", new { id = c.ConsultationId })
                }));
            }

            foreach (var med in medications.OrderByDescending(m => m.StartDate).Take(2))
            {
                activities.Add((med.StartDate.Value, new ActivityItem
                {
                    Icon        = "pill",
                    Title       = $"Medication added: {med.MedicationName}",
                    Description = $"{med.Dosage} • Started {med.StartDate:MMM dd}",
                    ActionLink  = Url.Action("Index", "CurrentMedication")
                }));
            }

            foreach (var allergy in allergies.OrderByDescending(a => a.CreatedAt).Take(2))
            {
                activities.Add((allergy.CreatedAt, new ActivityItem
                {
                    Icon        = "warning",
                    Title       = $"Allergy recorded: {allergy.AllergenName}",
                    Description = $"{allergy.Severity.GetDisplayName()} • {allergy.CreatedAt:MMM dd}",
                    ActionLink  = Url.Action("Index", "Allergy")
                }));
            }

            foreach (var history in histories.OrderByDescending(h => h.DiagnosedDate).Take(2))
            {
                activities.Add((history.DiagnosedDate.Value, new ActivityItem
                {
                    Icon        = "medical_information",
                    Title       = $"Condition recorded: {history.ConditionName}",
                    Description = $"Diagnosed {history.DiagnosedDate:MMM dd}",
                    ActionLink  = Url.Action("Index", "MedicalHistory")
                }));
            }

            return activities
                .OrderByDescending(a => a.Date)
                .Select(a => a.Item)
                .Take(5)
                .ToList();
        }

        private List<HealthAlert> GetHealthAlerts(Patient patient, List<MedicalHistory> histories)
        {
            var alerts = new List<HealthAlert>();

            if (string.IsNullOrEmpty(patient.User?.FirstName))
            {
                alerts.Add(new HealthAlert
                {
                    Icon    = "priority_high",
                    Message = "Please add emergency contact information for safety"
                });
            }

            if (!histories.Any())
            {
                alerts.Add(new HealthAlert
                {
                    Icon    = "info",
                    Message = "Add your medical history for more accurate AI assessments"
                });
            }

            if (patient.IsPregnant || patient.IsBreastfeeding)
            {
                alerts.Add(new HealthAlert
                {
                    Icon    = "info",
                    Message = "Safety flags are active. AI will consider these in recommendations."
                });
            }

            return alerts;
        }

        private ActivityLogViewModel BuildActivityLog(
            Patient                 patient,
            List<CurrentMedication> medications,
            List<MedicalHistory>    histories,
            List<Allergy>           allergies,
            List<Consultation>      consultations)
        {
            var items = new List<ActivityLogItem>();

            foreach (var c in consultations)
            {
                string icon, iconBg, iconColor, title, description, badgeText, badgeCss;
                DateTime date;

                if (c.Status == "Completed" && c.CompletedAt.HasValue)
                {
                    var pharmacist = c.Pharmacist != null
                        ? $"{c.Pharmacist.FirstName} {c.Pharmacist.LastName}"
                        : "pharmacist";
                    icon        = "verified_user";
                    iconBg      = "bg-green-100 dark:bg-green-500/10";
                    iconColor   = "text-green-600 dark:text-green-400";
                    title       = "Consultation reviewed";
                    description = $"Reviewed by {pharmacist}. Tap to see your recommendation.";
                    badgeText   = "Completed";
                    badgeCss    = "bg-green-100 text-green-700 dark:bg-green-500/10 dark:text-green-400";
                    date        = c.CompletedAt.Value;
                }
                else if (c.Status == "UnderReview" && c.ReviewedAt.HasValue)
                {
                    icon        = "manage_search";
                    iconBg      = "bg-primary/10";
                    iconColor   = "text-primary";
                    title       = "Consultation under review";
                    description = "A pharmacist has picked up your case and is reviewing it now.";
                    badgeText   = "Under Review";
                    badgeCss    = "bg-blue-100 text-blue-700 dark:bg-blue-500/10 dark:text-blue-400";
                    date        = c.ReviewedAt.Value;
                }
                else if (c.Status == "Cancelled")
                {
                    icon        = "cancel";
                    iconBg      = "bg-red-100 dark:bg-red-500/10";
                    iconColor   = "text-red-500";
                    title       = "Consultation cancelled";
                    description = $"Submitted {c.CreatedAt:MMM dd, yyyy}";
                    badgeText   = "Cancelled";
                    badgeCss    = "bg-red-100 text-red-600 dark:bg-red-500/10 dark:text-red-400";
                    date        = c.CreatedAt;
                }
                else
                {
                    icon        = "chat";
                    iconBg      = "bg-amber-100 dark:bg-amber-500/10";
                    iconColor   = "text-amber-600 dark:text-amber-400";
                    title       = "Consultation submitted";
                    description = $"{c.SymptomSeverity ?? "General"} severity · Awaiting pharmacist review.";
                    badgeText   = "Pending";
                    badgeCss    = "bg-amber-100 text-amber-700 dark:bg-amber-500/10 dark:text-amber-400";
                    date        = c.CreatedAt;
                }

                items.Add(new ActivityLogItem
                {
                    Icon        = icon,
                    IconBg      = iconBg,
                    IconColor   = iconColor,
                    Category    = "Consultation",
                    Title       = title,
                    Description = description,
                    TimeAgo     = GetTimeAgo(date),
                    Date        = date,
                    BadgeText   = badgeText,
                    BadgeCss    = badgeCss,
                    ActionLink  = Url.Action("Details", "Consultation", new { id = c.ConsultationId })
                });
            }

            foreach (var med in medications)
            {
                items.Add(new ActivityLogItem
                {
                    Icon        = "pill",
                    IconBg      = "bg-primary/10",
                    IconColor   = "text-primary",
                    Category    = "Medication",
                    Title       = $"Medication added: {med.MedicationName}",
                    Description = $"{med.Dosage} · Started {med.StartDate:MMM dd, yyyy}",
                    TimeAgo     = GetTimeAgo(med.StartDate.Value),
                    Date        = med.StartDate.Value,
                    ActionLink  = Url.Action("Index", "CurrentMedication")
                });
            }

            foreach (var allergy in allergies)
            {
                var isSevere = allergy.Severity.GetDisplayName() == Enums.AllergySeverity.Severe.GetDisplayName()
                            || allergy.Severity.GetDisplayName() == Enums.AllergySeverity.LifeThreatening.GetDisplayName();

                items.Add(new ActivityLogItem
                {
                    Icon        = "warning",
                    IconBg      = isSevere ? "bg-red-100 dark:bg-red-500/10" : "bg-amber-100 dark:bg-amber-500/10",
                    IconColor   = isSevere ? "text-red-500" : "text-amber-600 dark:text-amber-400",
                    Category    = "Allergy",
                    Title       = $"Allergy recorded: {allergy.AllergenName}",
                    Description = $"{allergy.Severity.GetDisplayName()} severity · {allergy.Reaction}",
                    TimeAgo     = GetTimeAgo(allergy.CreatedAt),
                    Date        = allergy.CreatedAt,
                    BadgeText   = isSevere ? allergy.Severity.GetDisplayName() : null,
                    BadgeCss    = "bg-red-100 text-red-600 dark:bg-red-500/10 dark:text-red-400",
                    ActionLink  = Url.Action("Index", "Allergy")
                });
            }

            foreach (var h in histories)
            {
                items.Add(new ActivityLogItem
                {
                    Icon        = "medical_information",
                    IconBg      = "bg-amber-100 dark:bg-amber-500/10",
                    IconColor   = "text-amber-600 dark:text-amber-400",
                    Category    = "Medical History",
                    Title       = $"Condition recorded: {h.ConditionName}",
                    Description = $"Diagnosed {h.DiagnosedDate:MMM dd, yyyy}",
                    TimeAgo     = GetTimeAgo(h.DiagnosedDate.Value),
                    Date        = h.DiagnosedDate.Value,
                    ActionLink  = Url.Action("Index", "MedicalHistory")
                });
            }

            var sorted = items.OrderByDescending(i => i.Date).ToList();

            var groups = sorted
                .GroupBy(i => GetGroupLabel(i.Date))
                .Select(g => new ActivityGroup { Label = g.Key, Items = g.ToList() })
                .ToList();

            return new ActivityLogViewModel
            {
                PatientName         = patient.User.FirstName,
                Groups              = groups,
                TotalActivities     = sorted.Count,
                ConsultationCount   = sorted.Count(i => i.Category == "Consultation"),
                MedicationCount     = sorted.Count(i => i.Category == "Medication"),
                AllergyCount        = sorted.Count(i => i.Category == "Allergy"),
                MedicalHistoryCount = sorted.Count(i => i.Category == "Medical History")
            };
        }

        private static string GetGroupLabel(DateTime date)
        {
            var today = DateTime.Today;
            if (date.Date == today)                                          return "Today";
            if (date.Date == today.AddDays(-1))                              return "Yesterday";
            if (date >= today.AddDays(-7))                                   return "This Week";
            if (date.Month == today.Month && date.Year == today.Year)        return "This Month";
            return date.ToString("MMMM yyyy");
        }

        private static string GetTimeAgo(DateTime date)
        {
            var diff = DateTime.UtcNow - date.ToUniversalTime();
            if (diff.TotalMinutes < 1)  return "Just now";
            if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes}m ago";
            if (diff.TotalHours   < 24) return $"{(int)diff.TotalHours}h ago";
            if (diff.TotalDays    < 7)  return $"{(int)diff.TotalDays}d ago";
            return date.ToString("MMM dd, yyyy");
        }
    }
}