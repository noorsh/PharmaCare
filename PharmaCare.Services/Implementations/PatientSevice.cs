using Microsoft.Extensions.Logging;
using PharmaCare.Data.Models;
using PharmaCare.Data.Repositories.Interfaces;
using PharmaCare.Services.Interfaces;

namespace PharmaCare.Services.Implementations
{
    public class PatientService : IPatientService
    {
        private readonly IPatientRepository _patientRepository;
        private readonly ILogger<PatientService> _logger;

        public PatientService(
            IPatientRepository patientRepository,
            ILogger<PatientService> logger)
        {
            _patientRepository = patientRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<Patient>> GetAllPatientsAsync()
        {
            try
            {
                return await _patientRepository.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all patients");
                throw;
            }
        }

        public async Task<Patient?> GetPatientByIdAsync(int id)
        {
            try
            {
                return await _patientRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving patient with ID {id}");
                throw;
            }
        }

        public async Task<Patient?> GetPatientByUserIdAsync(string userId)
        {
            try
            {
                // Use the powerful FirstOrDefaultAsync with predicate
                return await _patientRepository.GetPatientByUserIdAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving patient for user {userId}");
                throw;
            }
        }

        public async Task<Patient> CreatePatientAsync(Patient patient)
        {
            try
            {
                // Use AnyAsync to check existence
                if (await _patientRepository.AnyAsync(p => p.UserId == patient.UserId))
                {
                    throw new InvalidOperationException("A patient profile already exists for this user.");
                }

                // Validate date of birth
                if (patient.DateOfBirth > DateTime.UtcNow)
                {
                    throw new InvalidOperationException("Date of birth cannot be in the future.");
                }

                if (patient.DateOfBirth > DateTime.UtcNow.AddYears(-1))
                {
                    throw new InvalidOperationException("Patient must be at least 1 year old.");
                }

                // Set timestamps
                patient.CreatedAt = DateTime.UtcNow;
                patient.UpdatedAt = DateTime.UtcNow;

                await _patientRepository.AddAsync(patient);
                await _patientRepository.SaveChangesAsync();

                _logger.LogInformation($"Patient profile created: ID {patient.PatientId} for User {patient.UserId}");
                return patient;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating patient profile");
                throw new InvalidOperationException("An error occurred while creating the patient profile.", ex);
            }
        }

        public async Task<bool> UpdatePatientAsync(Patient patient)
{
    try
    {
        var existingPatient = await _patientRepository.GetByIdAsync(patient.PatientId);
        if (existingPatient == null)
        {
            _logger.LogWarning($"Attempted to update non-existent patient: ID {patient.PatientId}");
            return false;
        }

        // Cannot change UserId
        if (existingPatient.UserId != patient.UserId)
        {
            throw new InvalidOperationException("Cannot change the user associated with a patient profile.");
        }

        // Validate date of birth
        if (patient.DateOfBirth > DateTime.UtcNow)
        {
            throw new InvalidOperationException("Date of birth cannot be in the future.");
        }

        // Update properties manually
        existingPatient.DateOfBirth = patient.DateOfBirth;
        existingPatient.Gender = patient.Gender;
        existingPatient.Address = patient.Address;
        existingPatient.City = patient.City;
        existingPatient.EmergencyContact = patient.EmergencyContact;
        existingPatient.Height = patient.Height;
        existingPatient.Weight = patient.Weight;
        existingPatient.BloodType = patient.BloodType;
        existingPatient.SmokingStatus = patient.SmokingStatus;
        existingPatient.AlcoholConsumption = patient.AlcoholConsumption;
        existingPatient.ExerciseFrequency = patient.ExerciseFrequency;
        existingPatient.IsPregnant = patient.IsPregnant;
        existingPatient.IsBreastfeeding = patient.IsBreastfeeding;
        existingPatient.HasKidneyDisease = patient.HasKidneyDisease;
        existingPatient.HasLiverDisease = patient.HasLiverDisease;
        existingPatient.HasDrugAllergies = patient.HasDrugAllergies;
        existingPatient.DrugAllergyDetails = patient.DrugAllergyDetails;
        existingPatient.AdditionalNotes = patient.AdditionalNotes;
        existingPatient.UpdatedAt = DateTime.UtcNow;
        
        // CreatedAt and UserId stay the same

        _patientRepository.Update(existingPatient);
        await _patientRepository.SaveChangesAsync();

        _logger.LogInformation($"Patient profile updated: ID {patient.PatientId}");
        return true;
    }
    catch (InvalidOperationException)
    {
        throw;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, $"Error updating patient {patient.PatientId}");
        throw new InvalidOperationException("An error occurred while updating the patient profile.", ex);
    }
}

        public async Task<bool> DeletePatientAsync(int id)
        {
            try
            {
                var patient = await _patientRepository.GetByIdAsync(id);
                if (patient == null)
                {
                    _logger.LogWarning($"Attempted to delete non-existent patient: ID {id}");
                    return false;
                }

                _patientRepository.Remove(patient);
                await _patientRepository.SaveChangesAsync();

                _logger.LogInformation($"Patient profile deleted: ID {id}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting patient {id}");
                throw new InvalidOperationException("An error occurred while deleting the patient profile.", ex);
            }
        }

        public async Task<bool> PatientExistsForUserAsync(string userId)
        {
            try
            {
                // Use AnyAsync with predicate
                return await _patientRepository.AnyAsync(p => p.UserId == userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking patient existence for user {userId}");
                throw;
            }
        }

        public async Task<IEnumerable<Patient>> SearchPatientsAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return await GetAllPatientsAsync();
                }

                // Use FindAsync with complex predicate
                return await _patientRepository.FindAsync(p =>
                    (p.User != null && (
                        p.User.FirstName.Contains(searchTerm) ||
                        p.User.LastName.Contains(searchTerm) ||
                        p.User.Email.Contains(searchTerm) ||
                        (p.User.PhoneNumber != null && p.User.PhoneNumber.Contains(searchTerm))
                    )) ||
                    (p.Address != null && p.Address.Contains(searchTerm)) ||
                    (p.City != null && p.City.Contains(searchTerm))
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error searching patients with term: {searchTerm}");
                throw;
            }
        }
        public async Task<Patient?> GetPatientWithDetailsAsync(int patientId)
        {
            try
            {
                return await _patientRepository.GetPatientWithDetailsAsync(patientId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving patient details for ID {patientId}");
                throw;
            }
        }
    }
}