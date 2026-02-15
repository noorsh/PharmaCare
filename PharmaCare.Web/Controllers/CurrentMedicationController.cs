using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PharmaCare.Data.Models;
using PharmaCare.Services.Interfaces;

namespace PharmaCare.MVC.Controllers
{
    [Authorize]
    public class CurrentMedicationController : Controller
    {
        private readonly ICurrentMedicationService _currentMedicationService;
        private readonly IPatientService _patientService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<CurrentMedicationController> _logger;

        public CurrentMedicationController(
            ICurrentMedicationService currentMedicationService,
            IPatientService patientService,
            UserManager<ApplicationUser> userManager,
            ILogger<CurrentMedicationController> logger)
        {
            _currentMedicationService = currentMedicationService;
            _patientService = patientService;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            // Verify the patient belongs to the current user
            var user = await _userManager.GetUserAsync(User);
            var patient = await _patientService.GetPatientByUserIdAsync(user.Id);
            var currentMedications = await _currentMedicationService.GetCurrentMedicationsByPatientIdAsync(patient.PatientId);
            ViewBag.PatientId = patient.PatientId;
            return View(currentMedications);
        }
        // GET: CurrentMedication/Create
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

                var medication = new CurrentMedication
                {
                    PatientId = patientId,
                    IsActive = true
                };

                return View(medication);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading create medication page for patient {patientId}");
                TempData["Error"] = "An error occurred. Please try again.";
                return RedirectToAction("Profile", "Patient");
            }
        }

        // POST: CurrentMedication/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CurrentMedication medication)
        {
            if (!ModelState.IsValid)
            {
                return View(medication);
            }

            try
            {
                // Verify the patient belongs to the current user
                var user = await _userManager.GetUserAsync(User);
                var patient = await _patientService.GetPatientByUserIdAsync(user.Id);

                if (patient == null || patient.PatientId != medication.PatientId)
                {
                    return Forbid();
                }

                await _currentMedicationService.CreateCurrentMedicationAsync(medication);

                TempData["Success"] = "Medication added successfully!";
                return RedirectToAction("Profile", "Patient");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(medication);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating medication");
                ModelState.AddModelError("", "An error occurred while adding the medication. Please try again.");
                return View(medication);
            }
        }

        // GET: CurrentMedication/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var medication = await _currentMedicationService.GetCurrentMedicationByIdAsync(id);
                if (medication == null)
                {
                    return NotFound();
                }

                // Verify the medication belongs to the current user
                var user = await _userManager.GetUserAsync(User);
                var patient = await _patientService.GetPatientByUserIdAsync(user.Id);

                if (patient == null || medication.PatientId != patient.PatientId)
                {
                    return Forbid();
                }

                return View(medication);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading medication {id} for editing");
                TempData["Error"] = "An error occurred. Please try again.";
                return RedirectToAction("Profile", "Patient");
            }
        }

        // POST: CurrentMedication/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CurrentMedication medication)
        {
            if (id != medication.CurrentMedicationId)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(medication);
            }

            try
            {
                // Verify the medication belongs to the current user
                var user = await _userManager.GetUserAsync(User);
                var patient = await _patientService.GetPatientByUserIdAsync(user.Id);

                if (patient == null || medication.PatientId != patient.PatientId)
                {
                    return Forbid();
                }

                var success = await _currentMedicationService.UpdateCurrentMedicationAsync(medication);

                if (success)
                {
                    TempData["Success"] = "Medication updated successfully!";
                    return RedirectToAction("Profile", "Patient");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to update medication.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating medication {id}");
                ModelState.AddModelError("", "An error occurred while updating the medication. Please try again.");
            }

            return View(medication);
        }



        // POST: CurrentMedication/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var medication = await _currentMedicationService.GetCurrentMedicationByIdAsync(id);
                if (medication == null)
                {
                    return NotFound();
                }

                // Verify the medication belongs to the current user
                var user = await _userManager.GetUserAsync(User);
                var patient = await _patientService.GetPatientByUserIdAsync(user.Id);

                if (patient == null || medication.PatientId != patient.PatientId)
                {
                    return Forbid();
                }

                var success = await _currentMedicationService.DeleteCurrentMedicationAsync(id);

                if (success)
                {
                    TempData["Success"] = "Medication deleted successfully!";
                }
                else
                {
                    TempData["Error"] = "Unable to delete medication.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting medication {id}");
                TempData["Error"] = "An error occurred while deleting the medication. Please try again.";
            }

            return RedirectToAction("Profile", "Patient");
        }
    }
}