using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PharmaCare.Data.Models;
using PharmaCare.Services.Interfaces;

namespace PharmaCare.MVC.Controllers
{
    [Authorize]
    public class AllergyController : Controller
    {
        private readonly IAllergyService _allergyService;
        private readonly IPatientService _patientService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AllergyController> _logger;

        public AllergyController(
            IAllergyService allergyService,
            IPatientService patientService,
            UserManager<ApplicationUser> userManager,
            ILogger<AllergyController> logger)
        {
            _allergyService = allergyService;
            _patientService = patientService;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: Allergy/Create
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

                var allergy = new Allergy
                {
                    PatientId = patientId
                };

                return View(allergy);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading create allergy page for patient {patientId}");
                TempData["Error"] = "An error occurred. Please try again.";
                return RedirectToAction("Profile", "Patient");
            }
        }

        // POST: Allergy/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Allergy allergy)
        {
            if (!ModelState.IsValid)
            {
                return View(allergy);
            }

            try
            {
                // Verify the patient belongs to the current user
                var user = await _userManager.GetUserAsync(User);
                var patient = await _patientService.GetPatientByUserIdAsync(user.Id);

                if (patient == null || patient.PatientId != allergy.PatientId)
                {
                    return Forbid();
                }

                await _allergyService.CreateAllergyAsync(allergy);

                TempData["Success"] = "Allergy added successfully!";
                return RedirectToAction("Profile", "Patient");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(allergy);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating allergy");
                ModelState.AddModelError("", "An error occurred while adding the allergy. Please try again.");
                return View(allergy);
            }
        }

        // GET: Allergy/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var allergy = await _allergyService.GetAllergyByIdAsync(id);
                if (allergy == null)
                {
                    return NotFound();
                }

                // Verify the allergy belongs to the current user
                var user = await _userManager.GetUserAsync(User);
                var patient = await _patientService.GetPatientByUserIdAsync(user.Id);

                if (patient == null || allergy.PatientId != patient.PatientId)
                {
                    return Forbid();
                }

                return View(allergy);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading allergy {id} for editing");
                TempData["Error"] = "An error occurred. Please try again.";
                return RedirectToAction("Profile", "Patient");
            }
        }

        // POST: Allergy/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Allergy allergy)
        {
            if (id != allergy.AllergyId)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(allergy);
            }

            try
            {
                // Verify the allergy belongs to the current user
                var user = await _userManager.GetUserAsync(User);
                var patient = await _patientService.GetPatientByUserIdAsync(user.Id);

                if (patient == null || allergy.PatientId != patient.PatientId)
                {
                    return Forbid();
                }

                // IMPORTANT: Don't fetch the existing allergy here
                // The service will handle it properly
                var success = await _allergyService.UpdateAllergyAsync(allergy);

                if (success)
                {
                    TempData["Success"] = "Allergy updated successfully!";
                    return RedirectToAction("Profile", "Patient");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to update allergy.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating allergy {id}");
                ModelState.AddModelError("", "An error occurred while updating the allergy. Please try again.");
            }

            return View(allergy);
        }


        // POST: Allergy/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var allergy = await _allergyService.GetAllergyByIdAsync(id);
                if (allergy == null)
                {
                    return NotFound();
                }

                // Verify the allergy belongs to the current user
                var user = await _userManager.GetUserAsync(User);
                var patient = await _patientService.GetPatientByUserIdAsync(user.Id);

                if (patient == null || allergy.PatientId != patient.PatientId)
                {
                    return Forbid();
                }

                var success = await _allergyService.DeleteAllergyAsync(id);

                if (success)
                {
                    TempData["Success"] = "Allergy deleted successfully!";
                }
                else
                {
                    TempData["Error"] = "Unable to delete allergy.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting allergy {id}");
                TempData["Error"] = "An error occurred while deleting the allergy. Please try again.";
            }

            return RedirectToAction("Profile", "Patient");
        }
        
        
    }
}