using Microsoft.AspNetCore.Mvc;
using PharmaCare.Data.Models;
using PharmaCare.Services.Interfaces;

namespace PharmaCare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientsController : ControllerBase
    {
        private readonly IPatientService _patientService;
        private readonly ILogger<PatientsController> _logger;

        public PatientsController(
            IPatientService patientService,
            ILogger<PatientsController> logger)
        {
            _patientService = patientService;
            _logger = logger;
        }

        // GET: api/patients
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Patient>>> GetPatients()
        {
            try
            {
                var patients = await _patientService.GetAllPatientsAsync();
                return Ok(patients);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving patients");
                return StatusCode(500, "An error occurred while retrieving patients");
            }
        }

        // GET: api/patients/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Patient>> GetPatient(int id)
        {
            try
            {
                var patient = await _patientService.GetPatientByIdAsync(id);

                if (patient == null)
                {
                    return NotFound(new { message = $"Patient with ID {id} not found" });
                }

                return Ok(patient);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving patient {id}");
                return StatusCode(500, "An error occurred while retrieving the patient");
            }
        }

        // POST: api/patients
        [HttpPost]
        public async Task<ActionResult<Patient>> CreatePatient(Patient patient)
        {
            try
            {
                var createdPatient = await _patientService.CreatePatientAsync(patient);
                return CreatedAtAction(
                    nameof(GetPatient),
                    new { id = createdPatient.PatientId },
                    createdPatient);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating patient");
                return StatusCode(500, "An error occurred while creating the patient");
            }
        }

        // PUT: api/patients/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePatient(int id, Patient patient)
        {
            if (id != patient.PatientId)
            {
                return BadRequest(new { message = "Patient ID mismatch" });
            }

            try
            {
                var success = await _patientService.UpdatePatientAsync(patient);

                if (!success)
                {
                    return NotFound(new { message = $"Patient with ID {id} not found" });
                }

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating patient {id}");
                return StatusCode(500, "An error occurred while updating the patient");
            }
        }

        // DELETE: api/patients/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePatient(int id)
        {
            try
            {
                var success = await _patientService.DeletePatientAsync(id);

                if (!success)
                {
                    return NotFound(new { message = $"Patient with ID {id} not found" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting patient {id}");
                return StatusCode(500, "An error occurred while deleting the patient");
            }
        }

        // GET: api/patients/search?term=john
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Patient>>> SearchPatients([FromQuery] string term)
        {
            try
            {
                var patients = await _patientService.SearchPatientsAsync(term);
                return Ok(patients);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error searching patients with term: {term}");
                return StatusCode(500, "An error occurred while searching patients");
            }
        }
    }
}