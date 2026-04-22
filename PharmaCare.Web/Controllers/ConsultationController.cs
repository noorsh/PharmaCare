using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PharmaCare.Business.Services.Interfaces;
using PharmaCare.Data.Models;
using PharmaCare.MVC.Models.ViewModels;
using PharmaCare.Services;
using PharmaCare.Services.Implementations;
using PharmaCare.Services.Interfaces;

namespace PharmaCare.MVC.Controllers
{
    [Authorize]
    public class ConsultationController : Controller
    {
        private readonly IConsultationService _consultationService;
        private readonly IAIAssessmentService _aiAssessmentService;
        private readonly IPatientService _patientService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ConsultationController> _logger;
        private readonly ConsultationChatService _chatService;
        private readonly IInventoryService _inventoryService;
        private readonly IEmailService _emailService;

        public ConsultationController(
            IConsultationService consultationService,
            IAIAssessmentService aiAssessmentService,
            IPatientService patientService,
            UserManager<ApplicationUser> userManager,
            ILogger<ConsultationController> logger,
            ConsultationChatService chatService,
            IEmailService emailService,
            IInventoryService inventoryService)
        {
            _consultationService = consultationService;
            _aiAssessmentService = aiAssessmentService;
            _patientService = patientService;
            _userManager = userManager;
            _logger = logger;
            _chatService = chatService;
            _inventoryService = inventoryService;
            _emailService = emailService;
        }

        // ─── PATIENT ACTIONS ────────────────────────────────────────────

        // GET: Consultation/Start
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> Start()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var patient = await _patientService.GetPatientByUserIdAsync(userId);
            if (patient == null) return RedirectToAction("Dashboard", "Patient");

            ViewBag.PatientName = patient.User?.FirstName;
            ViewBag.TotalSteps = _chatService.TotalSteps;
            return View();
        }

        // POST: Consultation/Start
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> Start(SubmitConsultationViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var patient = await _patientService.GetPatientByUserIdAsync(userId);

                if (patient == null)
                {
                    TempData["Error"] = "Patient profile not found.";
                    return RedirectToAction("Dashboard", "Patient");
                }

                var consultation = new Consultation
                {
                    PatientId = patient.PatientId,
                    Symptoms = model.Symptoms,
                    SymptomDuration = model.SymptomDuration,
                    SymptomSeverity = model.SymptomSeverity,
                    AdditionalInformation = model.AdditionalInformation,
                    Status = "Pending"
                };

                var created = await _consultationService.CreateConsultationAsync(consultation);

                var assessment = await _aiAssessmentService.GenerateAssessmentAsync(created);
                await _aiAssessmentService.SaveAssessmentAsync(assessment); // fixed: added await

                TempData["AssessmentReport"] = assessment.AssessmentReport;
                TempData["PossibleConditions"] = assessment.PossibleConditions;
                TempData["RedFlags"] = assessment.RedFlags;
                TempData["ConfidenceScore"] = assessment.ConfidenceScore?.ToString("F0");

                _logger.LogInformation($"Consultation {created.ConsultationId} submitted by patient {patient.PatientId}");

                return RedirectToAction(nameof(Confirmation), new { id = created.ConsultationId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting consultation");
                ModelState.AddModelError("", "An error occurred while submitting your consultation. Please try again.");
                return View(model);
            }
        }

        // POST: Consultation/NextStep
        [HttpPost]
        [Authorize(Roles = "Patient")]
        public IActionResult NextStep([FromBody] NextStepRequest request)
        {
            if (_chatService.IsEmergency(request.Answer))
            {
                return Ok(new
                {
                    isEmergency = true,
                    message = "⚠️ This sounds like a medical emergency. Please call emergency services (140 in Lebanon) or go to the nearest hospital immediately. Do not wait for a pharmacist."
                });
            }

            var nextStepNumber = request.CurrentStep + 1;

            if (_chatService.IsLastStep(request.CurrentStep))
                return Ok(new { isComplete = true });

            var nextStep = _chatService.GetStep(nextStepNumber);
            if (nextStep == null)
                return Ok(new { isComplete = true });

            return Ok(new
            {
                isComplete = false,
                isEmergency = false,
                step = new
                {
                    stepNumber = nextStep.StepNumber,
                    question = nextStep.Question,
                    inputType = nextStep.InputType,
                    options = nextStep.Options,
                    placeholder = nextStep.Placeholder
                }
            });
        }

        // POST: Consultation/SubmitChat
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> SubmitChat([FromForm] SubmitChatRequest request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var patient = await _patientService.GetPatientByUserIdAsync(userId);
                if (patient == null) return RedirectToAction("Dashboard", "Patient");

                if (string.IsNullOrWhiteSpace(request?.AnswersJson))
                {
                    _logger.LogWarning("SubmitChat called with empty AnswersJson");
                    TempData["Error"] = "No answers were submitted. Please try again.";
                    return RedirectToAction(nameof(Start));
                }

                var jsonOptions = new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var answers = System.Text.Json.JsonSerializer.Deserialize<List<StepAnswer>>(request.AnswersJson, jsonOptions) ?? new List<StepAnswer>();

                var mainComplaint = answers?.FirstOrDefault(a => a.StepNumber == 1)?.Answer ?? "";
                var duration = answers?.FirstOrDefault(a => a.StepNumber == 2)?.Answer;
                var severity = answers?.FirstOrDefault(a => a.StepNumber == 3)?.Answer ?? "Moderate";
                var transcript = _chatService.BuildSummary(answers ?? new());

                var consultation = new Consultation
                {
                    PatientId = patient.PatientId,
                    Symptoms = mainComplaint,
                    SymptomDuration = duration,
                    SymptomSeverity = severity,
                    AdditionalInformation = transcript,
                    Status = "Pending"
                };

                var created = await _consultationService.CreateConsultationAsync(consultation);

                var assessment = await _aiAssessmentService.GenerateAssessmentAsync(created);
                await _aiAssessmentService.SaveAssessmentAsync(assessment); // fixed: added await

                TempData["AssessmentReport"] = assessment.AssessmentReport;
                TempData["PossibleConditions"] = assessment.PossibleConditions;
                TempData["RedFlags"] = assessment.RedFlags;
                TempData["ConfidenceScore"] = assessment.ConfidenceScore?.ToString("F0");

                return RedirectToAction(nameof(Confirmation), new { id = created.ConsultationId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting consultation");
                TempData["Error"] = "An error occurred while submitting. Please try again.";
                return RedirectToAction(nameof(Start));
            }
        }

        // GET: Consultation/Confirmation/5
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> Confirmation(int id)
        {
            var consultation = await _consultationService.GetConsultationWithDetailsAsync(id);
            if (consultation == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var patient = await _patientService.GetPatientByUserIdAsync(userId);
            if (patient == null || consultation.PatientId != patient.PatientId)
                return Forbid();

            var viewModel = new ConsultationViewModel
            {
                Consultation = consultation,
                AIAssessment = consultation.AIAssessment,
                IsPatientView = true
            };

            return View(viewModel);
        }

        // GET: Consultation/MyConsultations
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> MyConsultations(string? status, string? search, int page = 1)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var patient = await _patientService.GetPatientByUserIdAsync(userId);
                if (patient == null) return NotFound();

                var all = await _consultationService.GetConsultationsByPatientWithDetailsAsync(patient.PatientId);

                var totalConsultations = all.Count();
                var pendingCount = all.Count(c => c.Status == "Pending" || c.Status == "UnderReview");
                var completedCount = all.Count(c => c.Status == "Completed");

                var filtered = all.AsEnumerable();

                if (!string.IsNullOrWhiteSpace(status) && status != "All")
                {
                    filtered = status == "Pending"
                        ? filtered.Where(c => c.Status == "Pending" || c.Status == "UnderReview")
                        : filtered.Where(c => c.Status == status);
                }

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var q = search.ToLower();
                    filtered = filtered.Where(c =>
                        c.ConsultationId.ToString().Contains(q) ||
                        c.Symptoms.ToLower().Contains(q));
                }

                var filteredList = filtered.OrderByDescending(c => c.CreatedAt).ToList();
                const int pageSize = 8;
                var totalResults = filteredList.Count;

                var paged = filteredList
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(c => new ConsultationSummary
                    {
                        ConsultationId  = c.ConsultationId,
                        Symptoms        = c.Symptoms,
                        SymptomDuration = c.SymptomDuration,
                        SymptomSeverity = c.SymptomSeverity,
                        Status          = c.Status,
                        CreatedAt       = c.CreatedAt,
                        CompletedAt     = c.CompletedAt,
                        HasUrgentFlag   = c.SymptomSeverity == "Severe"
                    });

                var viewModel = new ConsultationHistoryViewModel
                {
                    TotalConsultations = totalConsultations,
                    PendingCount       = pendingCount,
                    CompletedCount     = completedCount,
                    StatusFilter       = status,
                    SearchQuery        = search,
                    CurrentPage        = page,
                    PageSize           = pageSize,
                    TotalResults       = totalResults,
                    Consultations      = paged
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading patient consultations");
                TempData["Error"] = "An error occurred while loading your consultations.";
                return View(new ConsultationHistoryViewModel());
            }
        }

        // GET: Consultation/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var consultation = await _consultationService.GetConsultationWithDetailsAsync(id);
            if (consultation == null) return NotFound();

            if (User.IsInRole("Patient"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var patient = await _patientService.GetPatientByUserIdAsync(userId);
                if (patient == null || consultation.PatientId != patient.PatientId)
                    return Forbid();
            }

            var viewModel = new ConsultationViewModel
            {
                Consultation = consultation,
                AIAssessment = consultation.AIAssessment,
                IsPatientView = User.IsInRole("Patient")
            };

            return View(viewModel);
        }

        // POST: Consultation/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var patient = await _patientService.GetPatientByUserIdAsync(userId);
                var consultation = await _consultationService.GetConsultationByIdAsync(id);

                if (consultation == null || patient == null || consultation.PatientId != patient.PatientId)
                    return Forbid();

                await _consultationService.CancelConsultationAsync(id);
                TempData["Success"] = "Consultation cancelled successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error cancelling consultation {id}");
                TempData["Error"] = "An error occurred while cancelling the consultation.";
            }

            return RedirectToAction(nameof(MyConsultations));
        }

        // ─── PHARMACIST ACTIONS ─────────────────────────────────────────

        // GET: Consultation/Queue
        [Authorize(Roles = "Pharmacist,Admin")]
        public async Task<IActionResult> Queue()
        {
            try
            {
                var allPending = await _consultationService.GetPendingConsultationsWithDetailsAsync();
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var recentCompleted = await _consultationService.GetRecentConsultationsWithDetailsAsync(5);

                var viewModel = new PharmacistQueueViewModel
                {
                    PendingConsultations     = allPending.Where(c => c.Status == "Pending"),
                    UnderReviewConsultations = allPending.Where(c => c.Status == "UnderReview"),
                    RecentlyCompleted        = recentCompleted.Where(c => c.Status == "Completed"),
                    TotalPending             = allPending.Count(c => c.Status == "Pending"),
                    TotalUnderReview         = allPending.Count(c => c.Status == "UnderReview")
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading consultation queue");
                TempData["Error"] = "An error occurred while loading the consultation queue.";
                return View(new PharmacistQueueViewModel());
            }
        }

        // POST: Consultation/Assign/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Pharmacist,Admin")]
        public async Task<IActionResult> Assign(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var success = await _consultationService.AssignPharmacistAsync(id, userId);

                if (success)
                {
                    TempData["Success"] = "Consultation assigned to you. You can now review it.";
                    return RedirectToAction(nameof(Review), new { id });
                }

                TempData["Error"] = "Unable to assign consultation. It may have already been assigned.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error assigning consultation {id}");
                TempData["Error"] = "An error occurred while assigning the consultation.";
            }

            return RedirectToAction(nameof(Queue));
        }

        // GET: Consultation/Review/5
        [Authorize(Roles = "Pharmacist,Admin")]
        public async Task<IActionResult> Review(int id)
        {
            try
            {
                var consultation = await _consultationService.GetConsultationWithDetailsAsync(id);
                if (consultation == null) return NotFound();

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (consultation.PharmacistId != userId && !User.IsInRole("Admin"))
                    return Forbid();

                var patient = await _patientService.GetPatientWithDetailsAsync(consultation.PatientId);
                var inventory = await _inventoryService.GetAllInventoryAsync();

                var viewModel = new ReviewConsultationViewModel
                {
                    Consultation         = consultation,
                    AIAssessment         = consultation.AIAssessment,
                    Patient              = patient,
                    CurrentMedications   = patient?.CurrentMedications ?? new List<CurrentMedication>(),
                    Allergies            = patient?.Allergies ?? new List<Allergy>(),
                    MedicalHistory       = patient?.MedicalHistories ?? new List<MedicalHistory>(),
                    AvailableMedications = inventory.Where(i => i.IsActive && !i.IsExpired)
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading review for consultation {id}");
                TempData["Error"] = "An error occurred while loading the consultation.";
                return RedirectToAction(nameof(Queue));
            }
        }

        // POST: Consultation/SubmitRecommendation/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Pharmacist,Admin")]
        public async Task<IActionResult> SubmitRecommendation(int id, ReviewConsultationViewModel model)
        {
            ModelState.Remove("Consultation");
            ModelState.Remove("AIAssessment");
            ModelState.Remove("Patient");
            ModelState.Remove("AvailableMedications");
            ModelState.Remove("CurrentMedications");
            ModelState.Remove("Allergies");
            ModelState.Remove("MedicalHistory");

            if (!ModelState.IsValid)
            {
                var consultation = await _consultationService.GetConsultationWithDetailsAsync(id);
                var patient = await _patientService.GetPatientWithDetailsAsync(consultation!.PatientId);
                var inventory = await _inventoryService.GetAllInventoryAsync();

                model.Consultation       = consultation;
                model.AIAssessment       = consultation.AIAssessment;
                model.Patient            = patient;
                model.CurrentMedications = patient?.CurrentMedications ?? new List<CurrentMedication>();
                model.Allergies          = patient?.Allergies ?? new List<Allergy>();
                model.MedicalHistory     = patient?.MedicalHistories ?? new List<MedicalHistory>();
                model.AvailableMedications = inventory.Where(i => i.IsActive && !i.IsExpired).DistinctBy(x=>x.MedicineName);

                return View("Review", model);
            }

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // If an inventory item was selected, ensure it exists to avoid FK violations
                if (model.InventoryId.HasValue)
                {
                    var inv = await _inventoryService.GetInventoryByIdAsync(model.InventoryId.Value);
                    if (inv == null)
                    {
                        // Reload required view data and show validation error
                        ModelState.AddModelError("InventoryId", "Selected medication not found in inventory.");
                        var consultation = await _consultationService.GetConsultationWithDetailsAsync(id);
                        var patient = await _patientService.GetPatientWithDetailsAsync(consultation!.PatientId);
                        var inventory = await _inventoryService.GetAllInventoryAsync();

                        model.Consultation       = consultation;
                        model.AIAssessment       = consultation.AIAssessment;
                        model.Patient            = patient;
                        model.CurrentMedications = patient?.CurrentMedications ?? new List<CurrentMedication>();
                        model.Allergies          = patient?.Allergies ?? new List<Allergy>();
                        model.MedicalHistory     = patient?.MedicalHistories ?? new List<MedicalHistory>();
                        model.AvailableMedications = inventory.Where(i => i.IsActive && !i.IsExpired).DistinctBy(x=>x.MedicineName);

                        return View("Review", model);
                    }
                }

                var recommendation = new Recommendation
                {
                    PharmacistNotes = model.PharmacistNotes,
                    InventoryId     = model.InventoryId,
                    Dosage          = model.Dosage,
                    Instructions    = model.Instructions,
                    Warnings        = model.Warnings,
                    ReferToDoctor   = model.ReferToDoctor,
                    ReferralReason  = model.ReferralReason
                };

                var success = await _consultationService.SubmitRecommendationAsync(id, userId, recommendation);

                if (success)
                {
                    // Send email notification to patient
                    var consultation = await _consultationService.GetConsultationWithDetailsAsync(id);
                    if (consultation?.Patient?.User?.Email != null)
                    {
                        var fullName = $"{consultation.Patient.User.FirstName} {consultation.Patient.User.LastName}";
                        var consultationLink = Url.Action("Details", "Consultation", 
                            new { id = id }, Request.Scheme);

                        await _emailService.SendEmailAsync(
                            consultation.Patient.User.Email,
                            fullName,
                            "PharmaCare Consultation Completed",
                            ConsultationCompletedEmail(fullName, consultationLink ?? "")
                        );
                    }

                    TempData["Success"] = "Recommendation submitted successfully.";
                    return RedirectToAction(nameof(Queue));
                }

                TempData["Error"] = "Unable to submit. Make sure this consultation is assigned to you.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error submitting recommendation for consultation {id}");
                TempData["Error"] = "An error occurred while submitting the recommendation.";
            }

            return RedirectToAction(nameof(Review), new { id });
        }

        // POST: Consultation/Complete/5 (kept for legacy, not used by UI)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Pharmacist,Admin")]
        public async Task<IActionResult> Complete(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var success = await _consultationService.CompleteConsultationAsync(id, userId);

                if (success)
                {
                    TempData["Success"] = "Consultation marked as completed.";
                    return RedirectToAction(nameof(Queue));
                }

                TempData["Error"] = "Unable to complete consultation. Make sure it is assigned to you.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error completing consultation {id}");
                TempData["Error"] = "An error occurred.";
            }

            return RedirectToAction(nameof(Queue));
        }
        // ─── INNER CLASSES ───────────────────────────────────────────────

        public class NextStepRequest
        {
            public int CurrentStep { get; set; }
            public string? Answer { get; set; }
        }

        public class SubmitChatRequest
        {
            public string? AnswersJson { get; set; }
        }
            // GET: Consultation/History
    [Authorize(Roles = "Pharmacist,Admin")]
    public async Task<IActionResult> History(string? search, string? status, DateTime? dateFrom, DateTime? dateTo, int page = 1)
    {
        try
        {
            var all = await _consultationService.GetRecentConsultationsWithDetailsAsync(500);

            var filtered = all
                .Where(c => c.Status == "Completed" || c.Status == "Cancelled")
                .AsEnumerable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var q = search.ToLower();
                filtered = filtered.Where(c =>
                    c.ConsultationId.ToString().Contains(q) ||
                    ($"{c.Patient?.User?.FirstName} {c.Patient?.User?.LastName}").ToLower().Contains(q));
            }

            if (!string.IsNullOrWhiteSpace(status) && status != "All")
                filtered = filtered.Where(c => c.Status == status);

            if (dateFrom.HasValue)
                filtered = filtered.Where(c => c.CreatedAt.Date >= dateFrom.Value.Date);

            if (dateTo.HasValue)
                filtered = filtered.Where(c => c.CreatedAt.Date <= dateTo.Value.Date);

            var ordered = filtered.OrderByDescending(c => c.CreatedAt).ToList();
            const int pageSize = 10;

            var paged = ordered
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new PharmacistConsultationRow
                {
                    ConsultationId  = c.ConsultationId,
                    PatientName     = $"{c.Patient?.User?.FirstName} {c.Patient?.User?.LastName}".Trim(),
                    PatientInitials = $"{c.Patient?.User?.FirstName?[0]}{c.Patient?.User?.LastName?[0]}".ToUpper(),
                    PatientCity     = c.Patient?.City,
                    Symptoms        = c.Symptoms,
                    SymptomSeverity = c.SymptomSeverity,
                    Status          = c.Status,
                    CreatedAt       = c.CreatedAt,
                    CompletedAt     = c.CompletedAt,
                    PharmacistName  = c.Pharmacist != null
                        ? $"{c.Pharmacist.FirstName} {c.Pharmacist.LastName}".Trim()
                        : "—"
                })
                .ToList();

            var viewModel = new PharmacistHistoryViewModel
            {
                Consultations = paged,
                TotalCount    = ordered.Count,
                CurrentPage   = page,
                HistoryPageSize = pageSize,
                SearchQuery   = search,
                StatusFilter  = status,
                DateFrom      = dateFrom,
                DateTo        = dateTo
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading consultation history");
            TempData["Error"] = "An error occurred while loading history.";
            return View(new PharmacistHistoryViewModel());
        }
    }


     private string ConsultationCompletedEmail(string fullName, string consultationLink) => $"""
        <div style="font-family: Arial, sans-serif; max-width: 600px; margin: auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px;">
            
            <div style="text-align: center; margin-bottom: 24px;">
                <h2 style="color: #1392ec; margin-bottom: 4px;">Consultation Completed ✓</h2>
                <p style="color: #617789; font-size: 14px; margin: 0;">Your PharmaCare consultation summary</p>
            </div>

            <p>Hi <strong>{fullName}</strong>,</p>
            <p style="color: #333;">
                Your consultation has been completed. Our pharmacist has reviewed your case and 
                their notes and recommendations are now available for you to review.
            </p>

            <div style="background: #f0f8ff; border-left: 4px solid #1392ec; padding: 16px; border-radius: 4px; margin: 24px 0;">
                <p style="margin: 0; color: #1392ec; font-weight: bold;">What's next?</p>
                <ul style="margin: 10px 0 0 0; padding-left: 20px; color: #333; line-height: 1.8;">
                    <li>Log in to your PharmaCare account to view your consultation notes.</li>
                    <li>Follow any recommendations provided by your pharmacist.</li>
                    <li>Contact us if you have any further questions.</li>
                </ul>
            </div>

            <div style="text-align: center; margin: 30px 0;">
                <a href="{consultationLink}"
                   style="background: #1392ec; color: white; padding: 14px 28px; border-radius: 8px; text-decoration: none; font-weight: bold; font-size: 16px;">
                    View Consultation
                </a>
            </div>

            <p style="color: #617789; font-size: 13px;">
                If you have any urgent medical concerns, please consult a healthcare professional 
                or contact emergency services immediately.
            </p>

            <hr style="border: none; border-top: 1px solid #e0e0e0; margin: 20px 0;" />
            <p style="color: #888; font-size: 12px; text-align: center;">
                PharmaCare — Your health, our priority.<br/>
                <span style="font-size: 11px;">If you did not request this consultation, please contact our support team.</span>
            </p>
        </div>
        """;
    }
        
}

