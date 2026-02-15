using Microsoft.AspNetCore.Mvc;
using PharmaCare.Data.Models;
using PharmaCare.Services.Interfaces;

namespace PharmaCare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConsultationsController : ControllerBase
    {
        private readonly IConsultationService _consultationService;
        private readonly ILogger<ConsultationsController> _logger;

        public ConsultationsController(
            IConsultationService consultationService,
            ILogger<ConsultationsController> logger)
        {
            _consultationService = consultationService;
            _logger = logger;
        }

        // GET: api/consultations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Consultation>>> GetConsultations()
        {
            try
            {
                var consultations = await _consultationService.GetAllConsultationsAsync();
                return Ok(consultations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving consultations");
                return StatusCode(500, new { message = "An error occurred while retrieving consultations" });
            }
        }

        // GET: api/consultations/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Consultation>> GetConsultation(int id)
        {
            try
            {
                var consultation = await _consultationService.GetConsultationByIdAsync(id);

                if (consultation == null)
                {
                    return NotFound(new { message = $"Consultation with ID {id} not found" });
                }

                return Ok(consultation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving consultation {id}");
                return StatusCode(500, new { message = "An error occurred while retrieving the consultation" });
            }
        }

        // GET: api/consultations/patient/5
        [HttpGet("patient/{patientId}")]
        public async Task<ActionResult<IEnumerable<Consultation>>> GetConsultationsByPatient(int patientId)
        {
            try
            {
                var consultations = await _consultationService.GetConsultationsByPatientIdAsync(patientId);
                return Ok(consultations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving consultations for patient {patientId}");
                return StatusCode(500, new { message = "An error occurred while retrieving consultations" });
            }
        }

        // GET: api/consultations/pending
        [HttpGet("pending")]
        public async Task<ActionResult<IEnumerable<Consultation>>> GetPendingConsultations()
        {
            try
            {
                var consultations = await _consultationService.GetPendingConsultationsAsync();
                return Ok(consultations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending consultations");
                return StatusCode(500, new { message = "An error occurred while retrieving pending consultations" });
            }
        }

        // POST: api/consultations
        [HttpPost]
        public async Task<ActionResult<Consultation>> CreateConsultation(Consultation consultation)
        {
            try
            {
                var createdConsultation = await _consultationService.CreateConsultationAsync(consultation);
                return CreatedAtAction(
                    nameof(GetConsultation),
                    new { id = createdConsultation.ConsultationId },
                    createdConsultation);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating consultation");
                return StatusCode(500, new { message = "An error occurred while creating the consultation" });
            }
        }

        // PUT: api/consultations/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateConsultation(int id, Consultation consultation)
        {
            if (id != consultation.ConsultationId)
            {
                return BadRequest(new { message = "Consultation ID mismatch" });
            }

            try
            {
                var success = await _consultationService.UpdateConsultationAsync(consultation);

                if (!success)
                {
                    return NotFound(new { message = $"Consultation with ID {id} not found" });
                }

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating consultation {id}");
                return StatusCode(500, new { message = "An error occurred while updating the consultation" });
            }
        }

        // DELETE: api/consultations/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteConsultation(int id)
        {
            try
            {
                var success = await _consultationService.DeleteConsultationAsync(id);

                if (!success)
                {
                    return NotFound(new { message = $"Consultation with ID {id} not found" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting consultation {id}");
                return StatusCode(500, new { message = "An error occurred while deleting the consultation" });
            }
        }
    }
}