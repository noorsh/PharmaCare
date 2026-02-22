using Microsoft.Extensions.Logging;
using PharmaCare.Data.Models;
using PharmaCare.Data.Repositories.Interfaces;
using PharmaCare.Services.Interfaces;

namespace PharmaCare.Services.Implementations
{
    public class MedicationService : IMedicationService
    {
        private readonly IMedicationRepository _medicationRepository;
        private readonly ILogger<MedicationService> _logger;

        public MedicationService(
            IMedicationRepository medicationRepository,
            ILogger<MedicationService> logger)
        {
            _medicationRepository = medicationRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<Medication>> GetAllMedicationsAsync()
        {
            try
            {
                return await _medicationRepository.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving medications");
                throw;
            }
        }

        public async Task<Medication?> GetMedicationByIdAsync(int id)
        {
            try
            {
                return await _medicationRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving medication {id}");
                throw;
            }
        }
    }

  
}