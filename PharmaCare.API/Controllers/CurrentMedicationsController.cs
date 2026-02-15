using Microsoft.AspNetCore.Mvc;
using PharmaCare.Data.Models;
using PharmaCare.Services.Interfaces;

namespace PharmaCare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CurrentMedicationsController : ControllerBase
    {
        private readonly ICurrentMedicationService _currentMedicationService;
        private readonly ILogger<CurrentMedicationsController> _logger;

        public CurrentMedicationsController(
            ICurrentMedicationService currentMedicationService,
            ILogger<CurrentMedicationsController> logger)
        {
            _currentMedicationService = currentMedicationService;
            _logger = logger;
        }

        // GET: api/currentmedications
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CurrentMedication>>> GetCurrentMedications()
        {
            try
            {
                var medications = await _currentMedicationService.GetAllCurrentMedicationsAsync();
                return Ok(medications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving current medications");
                return StatusCode(500, new { message = "An error occurred while retrieving medications" });
            }
        }

        // GET: api/currentmedications/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CurrentMedication>> GetCurrentMedication(int id)
        {
            try
            {
                var medication = await _currentMedicationService.GetCurrentMedicationByIdAsync(id);

                if (medication == null)
                {
                    return NotFound(new { message = $"Medication with ID {id} not found" });
                }

                return Ok(medication);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving medication {id}");
                return StatusCode(500, new { message = "An error occurred while retrieving the medication" });
            }
        }

        // GET: api/currentmedications/patient/5
        [HttpGet("patient/{patientId}")]
        public async Task<ActionResult<IEnumerable<CurrentMedication>>> GetMedicationsByPatient(int patientId)
        {
            try
            {
                var medications = await _currentMedicationService.GetCurrentMedicationsByPatientIdAsync(patientId);
                return Ok(medications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving medications for patient {patientId}");
                return StatusCode(500, new { message = "An error occurred while retrieving medications" });
            }
        }

        // GET: api/currentmedications/patient/5/active
        [HttpGet("patient/{patientId}/active")]
        public async Task<ActionResult<IEnumerable<CurrentMedication>>> GetActiveMedicationsByPatient(int patientId)
        {
            try
            {
                var medications = await _currentMedicationService.GetActiveMedicationsByPatientIdAsync(patientId);
                return Ok(medications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving active medications for patient {patientId}");
                return StatusCode(500, new { message = "An error occurred while retrieving active medications" });
            }
        }

        // GET: api/currentmedications/patient/5/medication/aspirin
        [HttpGet("patient/{patientId}/medication/{medicationName}")]
        public async Task<ActionResult<bool>> CheckPatientMedication(int patientId, string medicationName)
        {
            try
            {
                var isTaking = await _currentMedicationService.PatientIsTakingMedicationAsync(patientId, medicationName);
                return Ok(new { patientId, medicationName, isTaking });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking medication for patient {patientId}");
                return StatusCode(500, new { message = "An error occurred while checking the medication" });
            }
        }

        // POST: api/currentmedications
        [HttpPost]
        public async Task<ActionResult<CurrentMedication>> CreateCurrentMedication(CurrentMedication currentMedication)
        {
            try
            {
                var created = await _currentMedicationService.CreateCurrentMedicationAsync(currentMedication);
                return CreatedAtAction(
                    nameof(GetCurrentMedication),
                    new { id = created.CurrentMedicationId },
                    created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating medication");
                return StatusCode(500, new { message = "An error occurred while creating the medication" });
            }
        }

        // PUT: api/currentmedications/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCurrentMedication(int id, CurrentMedication currentMedication)
        {
            if (id != currentMedication.CurrentMedicationId)
            {
                return BadRequest(new { message = "Medication ID mismatch" });
            }

            try
            {
                var success = await _currentMedicationService.UpdateCurrentMedicationAsync(currentMedication);

                if (!success)
                {
                    return NotFound(new { message = $"Medication with ID {id} not found" });
                }

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating medication {id}");
                return StatusCode(500, new { message = "An error occurred while updating the medication" });
            }
        }

        // DELETE: api/currentmedications/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCurrentMedication(int id)
        {
            try
            {
                var success = await _currentMedicationService.DeleteCurrentMedicationAsync(id);

                if (!success)
                {
                    return NotFound(new { message = $"Medication with ID {id} not found" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting medication {id}");
                return StatusCode(500, new { message = "An error occurred while deleting the medication" });
            }
        }
    }
}