using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PharmaCare.Data.Models;
using PharmaCare.Services.Interfaces;

namespace PharmaCare.MVC.Controllers
{
    [Authorize]
    public class MedicalHistoryController : Controller
    {
        private readonly IMedicalHistoryService _medicalHistoryService;
        private readonly IPatientService _patientService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<MedicalHistoryController> _logger;

        public MedicalHistoryController(
            IMedicalHistoryService medicalHistoryService,
            IPatientService patientService,
            UserManager<ApplicationUser> userManager,
            ILogger<MedicalHistoryController> logger)
        {
            _medicalHistoryService = medicalHistoryService;
            _patientService = patientService;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: MedicalHistory/Create
        public async Task<IActionResult> Create(int patientId)
        {
            try
            {
                // Verify the patient belongs to the current user
                var user = await _userManager.GetUserAsync(User);
                var patient = await _patientService.GetPatientByUserIdAsync(user.Id);

                if (patient == null || patient.PatientId != patientId)
                {
                    return Forbid();
                }

                var medicalHistory = new MedicalHistory
                {
                    PatientId = patientId,
                    IsActive = true
                };

                return View(medicalHistory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading create medical history page for patient {patientId}");
                TempData["Error"] = "An error occurred. Please try again.";
                return RedirectToAction("Profile", "Patient");
            }
        }

        // POST: MedicalHistory/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MedicalHistory medicalHistory)
        {
            if (!ModelState.IsValid)
            {
                return View(medicalHistory);
            }

            try
            {
                // Verify the patient belongs to the current user
                var user = await _userManager.GetUserAsync(User);
                var patient = await _patientService.GetPatientByUserIdAsync(user.Id);

                if (patient == null || patient.PatientId != medicalHistory.PatientId)
                {
                    return Forbid();
                }

                await _medicalHistoryService.CreateMedicalHistoryAsync(medicalHistory);

                TempData["Success"] = "Medical condition added successfully!";
                return RedirectToAction("Profile", "Patient");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(medicalHistory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating medical history");
                ModelState.AddModelError("", "An error occurred while adding the condition. Please try again.");
                return View(medicalHistory);
            }
        }

        // GET: MedicalHistory/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var medicalHistory = await _medicalHistoryService.GetMedicalHistoryByIdAsync(id);
                if (medicalHistory == null)
                {
                    return NotFound();
                }

                // Verify the medical history belongs to the current user
                var user = await _userManager.GetUserAsync(User);
                var patient = await _patientService.GetPatientByUserIdAsync(user.Id);

                if (patient == null || medicalHistory.PatientId != patient.PatientId)
                {
                    return Forbid();
                }

                return View(medicalHistory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading medical history {id} for editing");
                TempData["Error"] = "An error occurred. Please try again.";
                return RedirectToAction("Profile", "Patient");
            }
        }

        // POST: MedicalHistory/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MedicalHistory medicalHistory)
        {
            if (id != medicalHistory.MedicalHistoryId)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(medicalHistory);
            }

            try
            {
                // Verify the medical history belongs to the current user
                var user = await _userManager.GetUserAsync(User);
                var patient = await _patientService.GetPatientByUserIdAsync(user.Id);

                if (patient == null || medicalHistory.PatientId != patient.PatientId)
                {
                    return Forbid();
                }

                var success = await _medicalHistoryService.UpdateMedicalHistoryAsync(medicalHistory);

                if (success)
                {
                    TempData["Success"] = "Medical condition updated successfully!";
                    return RedirectToAction("Profile", "Patient");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to update condition.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating medical history {id}");
                ModelState.AddModelError("", "An error occurred while updating the condition. Please try again.");
            }

            return View(medicalHistory);
        }

        // POST: MedicalHistory/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var medicalHistory = await _medicalHistoryService.GetMedicalHistoryByIdAsync(id);
                if (medicalHistory == null)
                {
                    TempData["Error"] = "Medical condition not found.";
                    return RedirectToAction("Profile", "Patient");
                }

                // Verify the medical history belongs to the current user
                var user = await _userManager.GetUserAsync(User);
                var patient = await _patientService.GetPatientByUserIdAsync(user.Id);

                if (patient == null || medicalHistory.PatientId != patient.PatientId)
                {
                    return Forbid();
                }

                var success = await _medicalHistoryService.DeleteMedicalHistoryAsync(id);

                if (success)
                {
                    TempData["Success"] = "Medical condition deleted successfully!";
                }
                else
                {
                    TempData["Error"] = "Unable to delete condition.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting medical history {id}");
                TempData["Error"] = "An error occurred while deleting the condition.";
            }

            return RedirectToAction("Profile", "Patient");
        }
    }
}