using Microsoft.Extensions.Logging;
using PharmaCare.Data.Models;
using PharmaCare.Data.Repositories.Interfaces;
using PharmaCare.Services.Interfaces;

namespace PharmaCare.Services.Implementations
{
    public class AllergyService : IAllergyService
    {
        private readonly IAllergyRepository _allergyRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly ILogger<AllergyService> _logger;

        public AllergyService(
            IAllergyRepository allergyRepository,
            IPatientRepository patientRepository,
            ILogger<AllergyService> logger)
        {
            _allergyRepository = allergyRepository;
            _patientRepository = patientRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<Allergy>> GetAllAllergiesAsync()
        {
            try
            {
                return await _allergyRepository.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all allergies");
                throw;
            }
        }

        public async Task<Allergy?> GetAllergyByIdAsync(int id)
        {
            try
            {
                return await _allergyRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving allergy {id}");
                throw;
            }
        }

        public async Task<IEnumerable<Allergy>> GetAllergiesByPatientIdAsync(int patientId)
        {
            try
            {
                return await _allergyRepository.FindAsync(a => a.PatientId == patientId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving allergies for patient {patientId}");
                throw;
            }
        }

        public async Task<Allergy> CreateAllergyAsync(Allergy allergy)
        {
            try
            {
                // Validate patient exists
                var patientExists = await _patientRepository.AnyAsync(p => p.PatientId == allergy.PatientId);
                if (!patientExists)
                {
                    throw new InvalidOperationException($"Patient with ID {allergy.PatientId} does not exist.");
                }

                // Check for duplicate allergy - FIXED: Use ToLower() instead of StringComparison
                var duplicateExists = await _allergyRepository.AnyAsync(a => 
                    a.PatientId == allergy.PatientId && 
                    a.AllergenName.ToLower() == allergy.AllergenName.ToLower());

                if (duplicateExists)
                {
                    throw new InvalidOperationException($"Allergy to {allergy.AllergenName} already exists for this patient.");
                }

                allergy.CreatedAt = DateTime.UtcNow;

                await _allergyRepository.AddAsync(allergy);
                await _allergyRepository.SaveChangesAsync();

                _logger.LogInformation($"Allergy created: ID {allergy.AllergyId} for Patient {allergy.PatientId}");
                return allergy;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating allergy");
                throw new InvalidOperationException("An error occurred while creating the allergy record.", ex);
            }
        }

        public async Task<bool> UpdateAllergyAsync(Allergy allergy)
        {
            try
            {
                // Get the existing entity
                var existingAllergy = await _allergyRepository.GetByIdAsync(allergy.AllergyId);
                if (existingAllergy == null)
                {
                    _logger.LogWarning($"Attempted to update non-existent allergy: ID {allergy.AllergyId}");
                    return false;
                }

                // Update properties manually (this prevents tracking issues)
                existingAllergy.AllergenName = allergy.AllergenName;
                existingAllergy.AllergyType = allergy.AllergyType;
                existingAllergy.Severity = allergy.Severity;
                existingAllergy.Reaction = allergy.Reaction;
                // CreatedAt stays the same (don't update)

                _allergyRepository.Update(existingAllergy);
                await _allergyRepository.SaveChangesAsync();

                _logger.LogInformation($"Allergy updated: ID {allergy.AllergyId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating allergy {allergy.AllergyId}");
                throw new InvalidOperationException("An error occurred while updating the allergy record.", ex);
            }
        }
        public async Task<bool> DeleteAllergyAsync(int id)
        {
            try
            {
                var allergy = await _allergyRepository.GetByIdAsync(id);
                if (allergy == null)
                {
                    _logger.LogWarning($"Attempted to delete non-existent allergy: ID {id}");
                    return false;
                }

                _allergyRepository.Remove(allergy);
                await _allergyRepository.SaveChangesAsync();

                _logger.LogInformation($"Allergy deleted: ID {id}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting allergy {id}");
                throw new InvalidOperationException("An error occurred while deleting the allergy record.", ex);
            }
        }

        public async Task<bool> PatientHasAllergyToAsync(int patientId, string allergenName)
        {
            try
            {
              
                return await _allergyRepository.AnyAsync(a => 
                    a.PatientId == patientId && 
                    a.AllergenName.ToLower().Contains(allergenName.ToLower()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking allergy for patient {patientId}");
                throw;
            }
        }
    }
}