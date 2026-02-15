using PharmaCare.Data.Models;

namespace PharmaCare.Data.Repositories.Interfaces
{
    public interface IPatientRepository : IRepository<Patient>
    {
        Task<Patient?> GetPatientWithDetailsAsync(int patientId);
        Task<IEnumerable<Patient>> SearchPatientsAsync(string searchTerm);
        Task<Patient?> GetPatientByEmailAsync(string email);
        Task<Patient?> GetPatientByPhoneAsync(string phone);
        Task<Patient?> GetPatientByUserIdAsync(string userId); // Added this
        Task<bool> IsEmailExistsAsync(string email, int? excludePatientId = null);
        Task<bool> IsPhoneExistsAsync(string phone, int? excludePatientId = null);
    }
}