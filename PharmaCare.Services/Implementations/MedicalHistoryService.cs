using Microsoft.Extensions.Logging;
using PharmaCare.Data.Models;
using PharmaCare.Data.Repositories.Interfaces;
using PharmaCare.Services.Interfaces;

namespace PharmaCare.Services.Implementations
{
    public class MedicalHistoryService : IMedicalHistoryService
    {
        private readonly IMedicalHistoryRepository _medicalHistoryRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly ILogger<MedicalHistoryService> _logger;

        public MedicalHistoryService(
            IMedicalHistoryRepository medicalHistoryRepository,
            IPatientRepository patientRepository,
            ILogger<MedicalHistoryService> logger)
        {
            _medicalHistoryRepository = medicalHistoryRepository;
            _patientRepository = patientRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<MedicalHistory>> GetAllMedicalHistoriesAsync()
        {
            try
            {
                return await _medicalHistoryRepository.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all medical histories");
                throw;
            }
        }

        public async Task<MedicalHistory?> GetMedicalHistoryByIdAsync(int id)
        {
            try
            {
                return await _medicalHistoryRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving medical history {id}");
                throw;
            }
        }

        public async Task<IEnumerable<MedicalHistory>> GetMedicalHistoriesByPatientIdAsync(int patientId)
        {
            try
            {
                return await _medicalHistoryRepository.FindAsync(mh => mh.PatientId == patientId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving medical histories for patient {patientId}");
                throw;
            }
        }

        public async Task<MedicalHistory> CreateMedicalHistoryAsync(MedicalHistory medicalHistory)
        {
            try
            {
                // Validate patient exists
                var patientExists = await _patientRepository.AnyAsync(p => p.PatientId == medicalHistory.PatientId);
                if (!patientExists)
                {
                    throw new InvalidOperationException($"Patient with ID {medicalHistory.PatientId} does not exist.");
                }

                // Set timestamp
                medicalHistory.CreatedAt = DateTime.UtcNow;

                await _medicalHistoryRepository.AddAsync(medicalHistory);
                await _medicalHistoryRepository.SaveChangesAsync();

                _logger.LogInformation($"Medical history created: ID {medicalHistory.MedicalHistoryId} for Patient {medicalHistory.PatientId}");
                return medicalHistory;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating medical history");
                throw new InvalidOperationException("An error occurred while creating the medical history.", ex);
            }
        }

        public async Task<bool> UpdateMedicalHistoryAsync(MedicalHistory medicalHistory)
        {
            try
            {
                var existingHistory = await _medicalHistoryRepository.GetByIdAsync(medicalHistory.MedicalHistoryId);
                if (existingHistory == null)
                {
                    _logger.LogWarning($"Attempted to update non-existent medical history: ID {medicalHistory.MedicalHistoryId}");
                    return false;
                }

                // Update properties
                existingHistory.ConditionName = medicalHistory.ConditionName;
                existingHistory.DiagnosedDate = medicalHistory.DiagnosedDate;
                existingHistory.Notes = medicalHistory.Notes;
                existingHistory.IsActive = medicalHistory.IsActive;
                // CreatedAt stays the same

                _medicalHistoryRepository.Update(existingHistory);
                await _medicalHistoryRepository.SaveChangesAsync();

                _logger.LogInformation($"Medical history updated: ID {medicalHistory.MedicalHistoryId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating medical history {medicalHistory.MedicalHistoryId}");
                throw new InvalidOperationException("An error occurred while updating the medical history.", ex);
            }
        }

        public async Task<bool> DeleteMedicalHistoryAsync(int id)
        {
            try
            {
                var medicalHistory = await _medicalHistoryRepository.GetByIdAsync(id);
                if (medicalHistory == null)
                {
                    _logger.LogWarning($"Attempted to delete non-existent medical history: ID {id}");
                    return false;
                }

                _medicalHistoryRepository.Remove(medicalHistory);
                await _medicalHistoryRepository.SaveChangesAsync();

                _logger.LogInformation($"Medical history deleted: ID {id}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting medical history {id}");
                throw new InvalidOperationException("An error occurred while deleting the medical history.", ex);
            }
        }

        public async Task<bool> PatientHasConditionAsync(int patientId, string conditionName)
        {
            try
            {
                return await _medicalHistoryRepository.AnyAsync(mh => 
                    mh.PatientId == patientId && 
                    mh.ConditionName.Contains(conditionName, StringComparison.OrdinalIgnoreCase) &&
                    mh.IsActive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking condition for patient {patientId}");
                throw;
            }
        }
    }
}