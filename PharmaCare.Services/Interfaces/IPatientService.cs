using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using PharmaCare.Data.Models;

namespace PharmaCare.Services.Interfaces
{
    public interface IPatientService
    {
        Task<IEnumerable<Patient>> GetAllPatientsAsync();
        Task<Patient?> GetPatientByIdAsync(int id);
        Task<Patient?> GetPatientByUserIdAsync(string userId);
        Task<Patient> CreatePatientAsync(Patient patient);
        Task<bool> UpdatePatientAsync(Patient patient);
        Task<bool> DeletePatientAsync(int id);
        Task<bool> PatientExistsForUserAsync(string userId);
        Task<IEnumerable<Patient>> SearchPatientsAsync(string searchTerm);
        Task<Patient?> GetPatientWithDetailsAsync(int patientId);
        Task<IEnumerable<Patient>> GetAllPatientsWithDetailsAsync();
        Task<string?> SaveProfilePhotoAsync(IFormFile file, string webRootPath);
        void DeleteProfilePhoto(string? photoUrl, string webRootPath);
    }
}