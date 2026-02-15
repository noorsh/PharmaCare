using Microsoft.AspNetCore.Mvc;
using PharmaCare.Data.Models;
using PharmaCare.Services.Interfaces;

namespace PharmaCare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MedicalHistoriesController : ControllerBase
    {
        private readonly IMedicalHistoryService _medicalHistoryService;
        private readonly ILogger<MedicalHistoriesController> _logger;

        public MedicalHistoriesController(
            IMedicalHistoryService medicalHistoryService,
            ILogger<MedicalHistoriesController> logger)
        {
            _medicalHistoryService = medicalHistoryService;
            _logger = logger;
        }

        // GET: api/medicalhistories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MedicalHistory>>> GetMedicalHistories()
        {
            try
            {
                var histories = await _medicalHistoryService.GetAllMedicalHistoriesAsync();
                return Ok(histories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving medical histories");
                return StatusCode(500, new { message = "An error occurred while retrieving medical histories" });
            }
        }

        // GET: api/medicalhistories/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MedicalHistory>> GetMedicalHistory(int id)
        {
            try
            {
                var history = await _medicalHistoryService.GetMedicalHistoryByIdAsync(id);

                if (history == null)
                {
                    return NotFound(new { message = $"Medical history with ID {id} not found" });
                }

                return Ok(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving medical history {id}");
                return StatusCode(500, new { message = "An error occurred while retrieving the medical history" });
            }
        }

        // GET: api/medicalhistories/patient/5
        [HttpGet("patient/{patientId}")]
        public async Task<ActionResult<IEnumerable<MedicalHistory>>> GetMedicalHistoriesByPatient(int patientId)
        {
            try
            {
                var histories = await _medicalHistoryService.GetMedicalHistoriesByPatientIdAsync(patientId);
                return Ok(histories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving medical histories for patient {patientId}");
                return StatusCode(500, new { message = "An error occurred while retrieving medical histories" });
            }
        }

        // GET: api/medicalhistories/patient/5/condition/diabetes
        [HttpGet("patient/{patientId}/condition/{conditionName}")]
        public async Task<ActionResult<bool>> CheckPatientCondition(int patientId, string conditionName)
        {
            try
            {
                var hasCondition = await _medicalHistoryService.PatientHasConditionAsync(patientId, conditionName);
                return Ok(new { patientId, conditionName, hasCondition });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking condition for patient {patientId}");
                return StatusCode(500, new { message = "An error occurred while checking the condition" });
            }
        }

        // POST: api/medicalhistories
        [HttpPost]
        public async Task<ActionResult<MedicalHistory>> CreateMedicalHistory(MedicalHistory medicalHistory)
        {
            try
            {
                var created = await _medicalHistoryService.CreateMedicalHistoryAsync(medicalHistory);
                return CreatedAtAction(
                    nameof(GetMedicalHistory),
                    new { id = created.MedicalHistoryId },
                    created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating medical history");
                return StatusCode(500, new { message = "An error occurred while creating the medical history" });
            }
        }

        // PUT: api/medicalhistories/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMedicalHistory(int id, MedicalHistory medicalHistory)
        {
            if (id != medicalHistory.MedicalHistoryId)
            {
                return BadRequest(new { message = "Medical history ID mismatch" });
            }

            try
            {
                var success = await _medicalHistoryService.UpdateMedicalHistoryAsync(medicalHistory);

                if (!success)
                {
                    return NotFound(new { message = $"Medical history with ID {id} not found" });
                }

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating medical history {id}");
                return StatusCode(500, new { message = "An error occurred while updating the medical history" });
            }
        }

        // DELETE: api/medicalhistories/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMedicalHistory(int id)
        {
            try
            {
                var success = await _medicalHistoryService.DeleteMedicalHistoryAsync(id);

                if (!success)
                {
                    return NotFound(new { message = $"Medical history with ID {id} not found" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting medical history {id}");
                return StatusCode(500, new { message = "An error occurred while deleting the medical history" });
            }
        }
    }
}