using Microsoft.Extensions.Logging;
using PharmaCare.Data.Models;
using PharmaCare.Data.Repositories.Interfaces;
using PharmaCare.Services.Interfaces;

namespace PharmaCare.Services.Implementations
{
    public class CurrentMedicationService : ICurrentMedicationService
    {
        private readonly ICurrentMedicationRepository _currentMedicationRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly ILogger<CurrentMedicationService> _logger;

        public CurrentMedicationService(
            ICurrentMedicationRepository currentMedicationRepository,
            IPatientRepository patientRepository,
            ILogger<CurrentMedicationService> logger)
        {
            _currentMedicationRepository = currentMedicationRepository;
            _patientRepository = patientRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<CurrentMedication>> GetAllCurrentMedicationsAsync()
        {
            try
            {
                return await _currentMedicationRepository.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all current medications");
                throw;
            }
        }

        public async Task<CurrentMedication?> GetCurrentMedicationByIdAsync(int id)
        {
            try
            {
                return await _currentMedicationRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving current medication {id}");
                throw;
            }
        }

        public async Task<IEnumerable<CurrentMedication>> GetCurrentMedicationsByPatientIdAsync(int patientId)
        {
            try
            {
                return await _currentMedicationRepository.FindAsync(cm => cm.PatientId == patientId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving current medications for patient {patientId}");
                throw;
            }
        }

        public async Task<IEnumerable<CurrentMedication>> GetActiveMedicationsByPatientIdAsync(int patientId)
        {
            try
            {
                return await _currentMedicationRepository.FindAsync(cm => 
                    cm.PatientId == patientId && 
                    cm.IsActive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving active medications for patient {patientId}");
                throw;
            }
        }

        public async Task<CurrentMedication> CreateCurrentMedicationAsync(CurrentMedication currentMedication)
        {
            try
            {
                // Validate patient exists
                var patientExists = await _patientRepository.AnyAsync(p => p.PatientId == currentMedication.PatientId);
                if (!patientExists)
                {
                    throw new InvalidOperationException($"Patient with ID {currentMedication.PatientId} does not exist.");
                }

                currentMedication.CreatedAt = DateTime.UtcNow;

                await _currentMedicationRepository.AddAsync(currentMedication);
                await _currentMedicationRepository.SaveChangesAsync();

                _logger.LogInformation($"Current medication created: ID {currentMedication.CurrentMedicationId} for Patient {currentMedication.PatientId}");
                return currentMedication;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating current medication");
                throw new InvalidOperationException("An error occurred while creating the medication record.", ex);
            }
        }

        public async Task<bool> UpdateCurrentMedicationAsync(CurrentMedication currentMedication)
        {
            try
            {
                var existingMedication = await _currentMedicationRepository.GetByIdAsync(currentMedication.CurrentMedicationId);
                if (existingMedication == null)
                {
                    _logger.LogWarning($"Attempted to update non-existent medication: ID {currentMedication.CurrentMedicationId}");
                    return false;
                }

                // Update properties
                existingMedication.MedicationName = currentMedication.MedicationName;
                existingMedication.Dosage = currentMedication.Dosage;
                existingMedication.Frequency = currentMedication.Frequency;
                existingMedication.StartDate = currentMedication.StartDate;
                existingMedication.Purpose = currentMedication.Purpose;
                existingMedication.IsActive = currentMedication.IsActive;
                // CreatedAt stays the same

                _currentMedicationRepository.Update(existingMedication);
                await _currentMedicationRepository.SaveChangesAsync();

                _logger.LogInformation($"Current medication updated: ID {currentMedication.CurrentMedicationId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating medication {currentMedication.CurrentMedicationId}");
                throw new InvalidOperationException("An error occurred while updating the medication record.", ex);
            }
        }
        public async Task<bool> DeleteCurrentMedicationAsync(int id)
        {
            try
            {
                var medication = await _currentMedicationRepository.GetByIdAsync(id);
                if (medication == null)
                {
                    _logger.LogWarning($"Attempted to delete non-existent medication: ID {id}");
                    return false;
                }

                _currentMedicationRepository.Remove(medication);
                await _currentMedicationRepository.SaveChangesAsync();

                _logger.LogInformation($"Current medication deleted: ID {id}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting medication {id}");
                throw new InvalidOperationException("An error occurred while deleting the medication record.", ex);
            }
        }

        public async Task<bool> PatientIsTakingMedicationAsync(int patientId, string medicationName)
        {
            try
            {
                return await _currentMedicationRepository.AnyAsync(cm => 
                    cm.PatientId == patientId && 
                    cm.MedicationName.Contains(medicationName, StringComparison.OrdinalIgnoreCase) &&
                    cm.IsActive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking medication for patient {patientId}");
                throw;
            }
        }
    }
}