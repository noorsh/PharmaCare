using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PharmaCare.Data.Models;
using PharmaCare.MVC.Models.ViewModels;
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


        public ConsultationController(
            IConsultationService consultationService,
            IAIAssessmentService aiAssessmentService,
            IPatientService patientService,
            UserManager<ApplicationUser> userManager,
            ILogger<ConsultationController> logger,
            ConsultationChatService chatService)
        {
            _consultationService = consultationService;
            _aiAssessmentService = aiAssessmentService;
            _patientService = patientService;
            _userManager = userManager;
            _logger = logger;
            _chatService = chatService;
        }

        // ─── PATIENT ACTIONS ────────────────────────────────────────────
        

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

                // Create consultation
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

                // Generate AI assessment (placeholder)
                var assessment = await _aiAssessmentService.GenerateAssessmentAsync(created);
                assessment.ConsultationId = created.ConsultationId;

                // TODO: Save assessment to DB in next step
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

        // GET: Consultation/Confirmation/5
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> Confirmation(int id)
        {
            var consultation = await _consultationService.GetConsultationWithDetailsAsync(id);
            if (consultation == null) return NotFound();

            // Verify ownership
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

        // Stats — always from full unfiltered list
        var totalConsultations = all.Count();
        var pendingCount = all.Count(c => c.Status == "Pending" || c.Status == "UnderReview");
        var completedCount = all.Count(c => c.Status == "Completed");

        // Apply filters
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

            // Patients can only see their own
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
                var myReviewing = await _consultationService.GetConsultationsByPharmacistAsync(userId);
                var recentCompleted = await _consultationService.GetRecentConsultationsAsync(5);

                var viewModel = new PharmacistQueueViewModel
                {
                    PendingConsultations = allPending.Where(c => c.Status == "Pending"),
                    UnderReviewConsultations = allPending.Where(c => c.Status == "UnderReview"),
                    RecentlyCompleted = recentCompleted.Where(c => c.Status == "Completed"),
                    TotalPending = allPending.Count(c => c.Status == "Pending"),
                    TotalUnderReview = allPending.Count(c => c.Status == "UnderReview")
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
            var consultation = await _consultationService.GetConsultationWithDetailsAsync(id);
            if (consultation == null) return NotFound();

            var viewModel = new ReviewConsultationViewModel
            {
                Consultation = consultation,
                AIAssessment = consultation.AIAssessment
            };

            return View(viewModel);
        }

        // POST: Consultation/Complete/5
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

// POST: Consultation/NextStep
[HttpPost]
[Authorize(Roles = "Patient")]
public IActionResult NextStep([FromBody] NextStepRequest request)
{
    // Emergency check
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
    {
        // All steps done — return summary signal
        return Ok(new { isComplete = true });
    }

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
public async Task<IActionResult> SubmitChat(SubmitChatRequest request)
{
    try
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var patient = await _patientService.GetPatientByUserIdAsync(userId);
        if (patient == null) return RedirectToAction("Dashboard", "Patient");

        var answers = System.Text.Json.JsonSerializer.Deserialize<List<StepAnswer>>(request.AnswersJson);

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

        // Placeholder AI assessment
        var assessment = await _aiAssessmentService.GenerateAssessmentAsync(created);
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
public class NextStepRequest
{
    public int CurrentStep { get; set; }
    public string Answer { get; set; }
}

public class SubmitChatRequest
{
    public string AnswersJson { get; set; }
}
    }
}