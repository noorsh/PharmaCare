using Microsoft.Extensions.Logging;
using PharmaCare.Data.Models;
using PharmaCare.Data.Repositories.Interfaces;
using PharmaCare.Services.Interfaces;

namespace PharmaCare.Services.Implementations
{
    public class ConsultationService : IConsultationService
    {
        private readonly IConsultationRepository _consultationRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly ILogger<ConsultationService> _logger;

        public ConsultationService(
            IConsultationRepository consultationRepository,
            IPatientRepository patientRepository,
            ILogger<ConsultationService> logger)
        {
            _consultationRepository = consultationRepository;
            _patientRepository = patientRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<Consultation>> GetAllConsultationsAsync()
        {
            try
            {
                return await _consultationRepository.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all consultations");
                throw;
            }
        }

        public async Task<Consultation?> GetConsultationByIdAsync(int id)
        {
            try
            {
                return await _consultationRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving consultation {id}");
                throw;
            }
        }

        public async Task<IEnumerable<Consultation>> GetConsultationsByPatientIdAsync(int patientId)
        {
            try
            {
                return await _consultationRepository.FindAsync(c => c.PatientId == patientId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving consultations for patient {patientId}");
                throw;
            }
        }

        public async Task<Consultation> CreateConsultationAsync(Consultation consultation)
        {
            try
            {
                // Validate patient exists
                var patientExists = await _patientRepository.AnyAsync(p => p.PatientId == consultation.PatientId);
                if (!patientExists)
                {
                    throw new InvalidOperationException($"Patient with ID {consultation.PatientId} does not exist.");
                }

                // Set timestamps and default status
                consultation.CreatedAt = DateTime.UtcNow;
                if (string.IsNullOrEmpty(consultation.Status))
                {
                    consultation.Status = "Pending";
                }

                await _consultationRepository.AddAsync(consultation);
                await _consultationRepository.SaveChangesAsync();

                _logger.LogInformation($"Consultation created: ID {consultation.ConsultationId} for Patient {consultation.PatientId}");
                return consultation;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating consultation");
                throw new InvalidOperationException("An error occurred while creating the consultation.", ex);
            }
        }

        public async Task<bool> UpdateConsultationAsync(Consultation consultation)
        {
            try
            {
                var existingConsultation = await _consultationRepository.GetByIdAsync(consultation.ConsultationId);
                if (existingConsultation == null)
                {
                    _logger.LogWarning($"Attempted to update non-existent consultation: ID {consultation.ConsultationId}");
                    return false;
                }

                // Preserve creation date
                consultation.CreatedAt = existingConsultation.CreatedAt;

                // Set completion timestamp if status is Completed
                if (consultation.Status == "Completed" && !consultation.CompletedAt.HasValue)
                {
                    consultation.CompletedAt = DateTime.UtcNow;
                }

                _consultationRepository.Update(consultation);
                await _consultationRepository.SaveChangesAsync();

                _logger.LogInformation($"Consultation updated: ID {consultation.ConsultationId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating consultation {consultation.ConsultationId}");
                throw new InvalidOperationException("An error occurred while updating the consultation.", ex);
            }
        }

        public async Task<bool> DeleteConsultationAsync(int id)
        {
            try
            {
                var consultation = await _consultationRepository.GetByIdAsync(id);
                if (consultation == null)
                {
                    _logger.LogWarning($"Attempted to delete non-existent consultation: ID {id}");
                    return false;
                }

                _consultationRepository.Remove(consultation);
                await _consultationRepository.SaveChangesAsync();

                _logger.LogInformation($"Consultation deleted: ID {id}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting consultation {id}");
                throw new InvalidOperationException("An error occurred while deleting the consultation.", ex);
            }
        }

        public async Task<IEnumerable<Consultation>> GetPendingConsultationsAsync()
        {
            try
            {
                return await _consultationRepository.FindAsync(c => c.Status == "Pending");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending consultations");
                throw;
            }
        }

        public async Task<IEnumerable<Consultation>> GetRecentConsultationsAsync(int count = 10)
        {
            try
            {
                var allConsultations = await _consultationRepository.GetAllAsync();
                return allConsultations
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(count)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recent consultations");
                throw;
            }
            
        }
        public async Task<Consultation?> GetConsultationWithDetailsAsync(int id)
{
    try
    {
        return await _consultationRepository.GetConsultationWithDetailsAsync(id);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, $"Error retrieving consultation details for ID {id}");
        throw;
    }
}

public async Task<IEnumerable<Consultation>> GetConsultationsByPatientWithDetailsAsync(int patientId)
{
    try
    {
        return await _consultationRepository.GetConsultationsByPatientWithDetailsAsync(patientId);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, $"Error retrieving consultation details for patient {patientId}");
        throw;
    }
}

public async Task<IEnumerable<Consultation>> GetPendingConsultationsWithDetailsAsync()
{
    try
    {
        return await _consultationRepository.GetPendingConsultationsWithDetailsAsync();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error retrieving pending consultations with details");
        throw;
    }
}

public async Task<bool> AssignPharmacistAsync(int consultationId, string pharmacistId)
{
    try
    {
        var consultation = await _consultationRepository.GetByIdAsync(consultationId);
        if (consultation == null)
        {
            _logger.LogWarning($"Consultation {consultationId} not found for pharmacist assignment");
            return false;
        }

        if (consultation.Status != "Pending")
        {
            _logger.LogWarning($"Consultation {consultationId} is not in Pending status, cannot assign pharmacist");
            return false;
        }

        consultation.PharmacistId = pharmacistId;
        consultation.Status = "UnderReview";
        consultation.ReviewedAt = DateTime.UtcNow;

        _consultationRepository.Update(consultation);
        await _consultationRepository.SaveChangesAsync();

        _logger.LogInformation($"Pharmacist {pharmacistId} assigned to consultation {consultationId}");
        return true;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, $"Error assigning pharmacist to consultation {consultationId}");
        throw new InvalidOperationException("An error occurred while assigning the pharmacist.", ex);
    }
}

public async Task<bool> CompleteConsultationAsync(int consultationId, string pharmacistId)
{
    try
    {
        var consultation = await _consultationRepository.GetByIdAsync(consultationId);
        if (consultation == null)
        {
            _logger.LogWarning($"Consultation {consultationId} not found for completion");
            return false;
        }

        if (consultation.PharmacistId != pharmacistId)
        {
            _logger.LogWarning($"Pharmacist {pharmacistId} is not assigned to consultation {consultationId}");
            return false;
        }

        consultation.Status = "Completed";
        consultation.CompletedAt = DateTime.UtcNow;

        _consultationRepository.Update(consultation);
        await _consultationRepository.SaveChangesAsync();

        _logger.LogInformation($"Consultation {consultationId} completed by pharmacist {pharmacistId}");
        return true;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, $"Error completing consultation {consultationId}");
        throw new InvalidOperationException("An error occurred while completing the consultation.", ex);
    }
}

public async Task<bool> CancelConsultationAsync(int consultationId)
{
    try
    {
        var consultation = await _consultationRepository.GetByIdAsync(consultationId);
        if (consultation == null)
        {
            _logger.LogWarning($"Consultation {consultationId} not found for cancellation");
            return false;
        }

        if (consultation.Status == "Completed")
        {
            _logger.LogWarning($"Cannot cancel completed consultation {consultationId}");
            return false;
        }

        consultation.Status = "Cancelled";
        _consultationRepository.Update(consultation);
        await _consultationRepository.SaveChangesAsync();

        _logger.LogInformation($"Consultation {consultationId} cancelled");
        return true;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, $"Error cancelling consultation {consultationId}");
        throw new InvalidOperationException("An error occurred while cancelling the consultation.", ex);
    }
}

public async Task<IEnumerable<Consultation>> GetConsultationsByPharmacistAsync(string pharmacistId)
{
    try
    {
        return await _consultationRepository.GetConsultationsByPharmacistAsync(pharmacistId);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, $"Error retrieving consultations for pharmacist {pharmacistId}");
        throw;
    }
}
    }
}