using PharmaCare.Data.Models;

namespace PharmaCare.Services.Interfaces
{
    public interface IAllergyService
    {
        Task<IEnumerable<Allergy>> GetAllAllergiesAsync();
        Task<Allergy?> GetAllergyByIdAsync(int id);
        Task<IEnumerable<Allergy>> GetAllergiesByPatientIdAsync(int patientId);
        Task<Allergy> CreateAllergyAsync(Allergy allergy);
        Task<bool> UpdateAllergyAsync(Allergy allergy);
        Task<bool> DeleteAllergyAsync(int id);
        Task<bool> PatientHasAllergyToAsync(int patientId, string allergen);
    }
}