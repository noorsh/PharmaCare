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

        public PatientController(
            IPatientService patientService,
            IMedicalHistoryService medicalHistoryService,
            IAllergyService allergyService,
            ICurrentMedicationService currentMedicationService,
            IConsultationService consultationService,
            UserManager<ApplicationUser> userManager,
            ILogger<PatientController> logger)
        {
            _patientService = patientService;
            _medicalHistoryService = medicalHistoryService;
            _allergyService = allergyService;
            _currentMedicationService = currentMedicationService;
            _consultationService = consultationService;
            _userManager = userManager;
            _logger = logger;
        }

     // Add this to your PatientController.cs

[Authorize(Roles = "Patient")]
public async Task<IActionResult> Profile()
{
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    var patient = await _patientService.GetPatientByUserIdAsync(userId);
    
    if (patient == null)
        return NotFound();

    // Get counts for quick links
    var medicalHistories = (await _medicalHistoryService.GetMedicalHistoriesByPatientIdAsync(patient.PatientId)).ToList();
    var allergies = (await _allergyService.GetAllergiesByPatientIdAsync(patient.PatientId)).ToList();
    var medications = (await _currentMedicationService.GetCurrentMedicationsByPatientIdAsync(patient.PatientId)).ToList();

    // Calculate age
    ViewBag.Age = CalculateAge(patient.DateOfBirth);
    
    // Calculate profile completion
    ViewBag.ProfileCompletion = CalculateProfileCompletion(patient);
    
    // Set counts for quick links
    ViewBag.MedicalHistoryCount = medicalHistories.Count;
    ViewBag.AllergiesCount = allergies.Count;
    ViewBag.MedicationsCount = medications.Count;

    return View(patient);
}

private int CalculateAge(DateTime dateOfBirth)
{
    var today = DateTime.Today;
    var age = today.Year - dateOfBirth.Year;
    if (dateOfBirth.Date > today.AddYears(-age)) age--;
    return age;
}

        // GET: Patient/Edit
        public async Task<IActionResult> Edit()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

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
        public async Task<IActionResult> Edit(Patient patient)
        {
            if (!ModelState.IsValid)
            {
                return View(patient);
            }

            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var existingPatient = await _patientService.GetPatientByUserIdAsync(user.Id);
                if (existingPatient == null || existingPatient.PatientId != patient.PatientId)
                {
                    return Forbid();
                }

                // Ensure UserId doesn't change
                patient.UserId = existingPatient.UserId;

                var success = await _patientService.UpdatePatientAsync(patient);

                if (success)
                {
                    TempData["Success"] = "Profile updated successfully!";
                    return RedirectToAction(nameof(Profile));
                }
                else
                {
                    ModelState.AddModelError("", "Unable to update profile.");
                }
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
                {
                    return NotFound();
                }

                var viewModel = new PatientProfileViewModel
                {
                    Patient = patient,
                    User = patient.User,
                    MedicalHistories = await _medicalHistoryService.GetMedicalHistoriesByPatientIdAsync(id),
                    Allergies = await _allergyService.GetAllergiesByPatientIdAsync(id),
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


    public async Task<IActionResult> Dashboard()
    {
        // Get current user ID
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        // Get patient with User navigation property
        var patient = await _patientService.GetPatientByUserIdAsync(userId);
        
        if (patient == null)
            return NotFound();

        // Get all data
        var medicalHistories =( await _medicalHistoryService.GetMedicalHistoriesByPatientIdAsync(patient.PatientId)).ToList();
        var allergies =( await _allergyService.GetAllergiesByPatientIdAsync(patient.PatientId)).ToList();
        var medications = (await _currentMedicationService.GetCurrentMedicationsByPatientIdAsync(patient.PatientId))
            .ToList();
        var viewModel = new PatientDashboardViewModel
        {
            // Patient Info - FROM ApplicationUser
            PatientName = patient.User.FirstName,
            Age = CalculateAge(patient.DateOfBirth),
            BloodType = patient.BloodType?.GetDisplayName(),
            ProfileCompletionPercentage = CalculateProfileCompletion(patient),

            // Stats - Simple counts
            ActiveMedicationsCount = medications.Count(),
            RecentMedicationAdded = medications.Any(m => m.StartDate >= DateTime.Now.AddDays(-7)),
            AllergiesCount = allergies.Count(),
            SevereAllergiesCount = allergies.Count(a => a.Severity.GetDisplayName()== Enums.AllergySeverity.Severe.GetDisplayName() || a.Severity.GetDisplayName() == Enums.AllergySeverity.LifeThreatening.GetDisplayName()),
          //  MedicalConditionsCount = medicalHistories.Count(h => h.Status == "Active" || h.Status == "Chronic"),
            TotalConsultationsCount = 0, // TODO: Implement when you build Consultation
            PendingConsultationsCount = 0, // TODO: Implement when you build Consultation

            // Recent Activities - Simple implementation
            RecentActivities = GetRecentActivities(medications, medicalHistories),

            // Health Alerts
            HealthAlerts = GetHealthAlerts(patient, medicalHistories)
        };

        return View(viewModel);
    }

    private int CalculateAge(DateTime? dateOfBirth)
    {
        if (!dateOfBirth.HasValue) return 0;
        var today = DateTime.Today;
        var age = today.Year - dateOfBirth.Value.Year;
        if (dateOfBirth.Value.Date > today.AddYears(-age)) age--;
        return age;
    }

    private int CalculateProfileCompletion(Patient patient)
    {
        int totalFields = 14;
        int filledFields = 0;
        
        // ApplicationUser fields
        if (!string.IsNullOrEmpty(patient.User?.FirstName)) filledFields++;
        if (!string.IsNullOrEmpty(patient.User?.Email)) filledFields++;
        if (!string.IsNullOrEmpty(patient.User?.PhoneNumber)) filledFields++;
        
        // Patient fields
        if (patient.DateOfBirth!=null) filledFields++;
        if (!string.IsNullOrEmpty(patient.Gender)) filledFields++;
        if (!string.IsNullOrEmpty(patient.BloodType.GetDisplayName())) filledFields++;
        // if (!string.IsNullOrEmpty(patient.EmergencyContactName)) filledFields++;
        // if (!string.IsNullOrEmpty(patient.EmergencyContactPhone)) filledFields++;
        // if (!string.IsNullOrEmpty(patient.EmergencyContactRelationship)) filledFields++;
        if (!string.IsNullOrEmpty(patient.SmokingStatus.GetDisplayName())) filledFields++;
        if (!string.IsNullOrEmpty(patient.AlcoholConsumption.GetDisplayName())) filledFields++;
        if (!string.IsNullOrEmpty(patient.ExerciseFrequency.GetDisplayName())) filledFields++;
        
        // Safety flags (considered filled if set either way)
        filledFields++; // IsPregnant (bool always has value)
        filledFields++; // IsBreastfeeding (bool always has value)
        
        return (int)((double)filledFields / totalFields * 100);
    }

    private List<ActivityItem> GetRecentActivities(
        List<CurrentMedication> medications, 
        List<MedicalHistory> histories)
    {
        var activities = new List<ActivityItem>();
        
        // Recent medications
        var recentMeds = medications
            .OrderByDescending(m => m.StartDate)
            .Take(2);
        
        foreach (var med in recentMeds)
        {
            activities.Add(new ActivityItem
            {
                Icon = "pill",
                Title = $"Medication added: {med.MedicationName}",
                Description = $"{med.Dosage} • Started {med.StartDate:MMM dd}",
                ActionLink = Url.Action("CurrentMedications", "Patient")
            });
        }
        
        // Recent medical history
        var recentHistory = histories
            .OrderByDescending(h => h.DiagnosedDate)
            .Take(2);
        
        foreach (var history in recentHistory)
        {
            activities.Add(new ActivityItem
            {
                Icon = "medical_information",
                Title = $"Condition added: {history.ConditionName}",
                Description = $"Diagnosed {history.DiagnosedDate:MMM dd}",
                ActionLink = Url.Action("MedicalHistory", "Patient")
            });
        }
        
        return activities.OrderByDescending(a => a.Description).Take(5).ToList();
    }

    private List<HealthAlert> GetHealthAlerts(Patient patient, List<MedicalHistory> histories)
    {
        var alerts = new List<HealthAlert>();
        
        // Check emergency contact
        if (string.IsNullOrEmpty(patient.User?.FirstName))
        {
            alerts.Add(new HealthAlert
            {
                Icon = "priority_high",
                Message = "Please add emergency contact information for safety"
            });
        }
        
        // Check medical history
        if (!histories.Any())
        {
            alerts.Add(new HealthAlert
            {
                Icon = "info",
                Message = "Add your medical history for more accurate AI assessments"
            });
        }
        
        // Check safety flags
        if (patient.IsPregnant || patient.IsBreastfeeding)
        {
            alerts.Add(new HealthAlert
            {
                Icon = "info",
                Message = "Safety flags are active. AI will consider these in recommendations."
            });
        }
        
        return alerts;
    }
    
    
[Authorize(Roles = "Patient")]
public async Task<IActionResult> Allergies()
{
    try
    {
        // Get current patient
        var userId = _userManager.GetUserId(User);
        var patient = await _patientService.GetPatientByUserIdAsync(userId);

        if (patient == null)
        {
            TempData["Error"] = "Patient profile not found.";
            return RedirectToAction("Dashboard");
        }

        // Pass PatientId to view for "Add" button
        ViewBag.PatientId = patient.PatientId;

        // Get allergies for this patient
        var allergies = await _allergyService.GetAllergiesByPatientIdAsync(patient.PatientId);

        return View(allergies);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error loading allergies");
        TempData["Error"] = "An error occurred while loading your allergies.";
        return RedirectToAction("Dashboard");
    }
}

// Optional: PDF and Wallet Card features (placeholders for now)
[Authorize(Roles = "Patient")]
public async Task<IActionResult> DownloadAllergiesPDF()
{
    try
    {
        var userId = _userManager.GetUserId(User);
        var patient = await _patientService.GetPatientByUserIdAsync(userId);

        if (patient == null)
        {
            TempData["Error"] = "Patient profile not found.";
            return RedirectToAction("Allergies");
        }

        var allergies = await _allergyService.GetAllergiesByPatientIdAsync(patient.PatientId);

        // TODO: Generate PDF using a PDF library (iTextSharp, QuestPDF, etc.)
        // For now, return a placeholder
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

[Authorize(Roles = "Patient")]
public async Task<IActionResult> PrintWalletCard()
{
    try
    {
        var userId = _userManager.GetUserId(User);
        var patient = await _patientService.GetPatientByUserIdAsync(userId);

        if (patient == null)
        {
            TempData["Error"] = "Patient profile not found.";
            return RedirectToAction("Allergies");
        }

        var allergies = await _allergyService.GetAllergiesByPatientIdAsync(patient.PatientId);

        // TODO: Generate printable wallet card
        // For now, return a placeholder
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
    }
}