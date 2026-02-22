using PharmaCare.Data.Models;

namespace PharmaCare.Services.Interfaces
{
    public interface IMedicationService
    {
        Task<IEnumerable<Medication>> GetAllMedicationsAsync();
        Task<Medication?> GetMedicationByIdAsync(int id);
    }
}