using Microsoft.AspNetCore.Mvc;
using PharmaCare.Data.Models;
using PharmaCare.Services.Interfaces;

namespace PharmaCare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AllergiesController : ControllerBase
    {
        private readonly IAllergyService _allergyService;
        private readonly ILogger<AllergiesController> _logger;

        public AllergiesController(
            IAllergyService allergyService,
            ILogger<AllergiesController> logger)
        {
            _allergyService = allergyService;
            _logger = logger;
        }

        // GET: api/allergies
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Allergy>>> GetAllergies()
        {
            try
            {
                var allergies = await _allergyService.GetAllAllergiesAsync();
                return Ok(allergies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving allergies");
                return StatusCode(500, new { message = "An error occurred while retrieving allergies" });
            }
        }

        // GET: api/allergies/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Allergy>> GetAllergy(int id)
        {
            try
            {
                var allergy = await _allergyService.GetAllergyByIdAsync(id);

                if (allergy == null)
                {
                    return NotFound(new { message = $"Allergy with ID {id} not found" });
                }

                return Ok(allergy);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving allergy {id}");
                return StatusCode(500, new { message = "An error occurred while retrieving the allergy" });
            }
        }

        // GET: api/allergies/patient/5
        [HttpGet("patient/{patientId}")]
        public async Task<ActionResult<IEnumerable<Allergy>>> GetAllergiesByPatient(int patientId)
        {
            try
            {
                var allergies = await _allergyService.GetAllergiesByPatientIdAsync(patientId);
                return Ok(allergies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving allergies for patient {patientId}");
                return StatusCode(500, new { message = "An error occurred while retrieving allergies" });
            }
        }

        // GET: api/allergies/patient/5/allergen/penicillin
        [HttpGet("patient/{patientId}/allergen/{allergenName}")]
        public async Task<ActionResult<bool>> CheckPatientAllergy(int patientId, string allergenName)
        {
            try
            {
                var hasAllergy = await _allergyService.PatientHasAllergyToAsync(patientId, allergenName);
                return Ok(new { patientId, allergenName, hasAllergy });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking allergy for patient {patientId}");
                return StatusCode(500, new { message = "An error occurred while checking the allergy" });
            }
        }

        // POST: api/allergies
        [HttpPost]
        public async Task<ActionResult<Allergy>> CreateAllergy(Allergy allergy)
        {
            try
            {
                var created = await _allergyService.CreateAllergyAsync(allergy);
                return CreatedAtAction(
                    nameof(GetAllergy),
                    new { id = created.AllergyId },
                    created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating allergy");
                return StatusCode(500, new { message = "An error occurred while creating the allergy" });
            }
        }

        // PUT: api/allergies/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAllergy(int id, Allergy allergy)
        {
            if (id != allergy.AllergyId)
            {
                return BadRequest(new { message = "Allergy ID mismatch" });
            }

            try
            {
                var success = await _allergyService.UpdateAllergyAsync(allergy);

                if (!success)
                {
                    return NotFound(new { message = $"Allergy with ID {id} not found" });
                }

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating allergy {id}");
                return StatusCode(500, new { message = "An error occurred while updating the allergy" });
            }
        }

        // DELETE: api/allergies/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAllergy(int id)
        {
            try
            {
                var success = await _allergyService.DeleteAllergyAsync(id);

                if (!success)
                {
                    return NotFound(new { message = $"Allergy with ID {id} not found" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting allergy {id}");
                return StatusCode(500, new { message = "An error occurred while deleting the allergy" });
            }
        }
    }
}