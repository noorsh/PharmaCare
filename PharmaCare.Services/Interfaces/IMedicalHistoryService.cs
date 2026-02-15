using PharmaCare.Data.Models;

namespace PharmaCare.Services.Interfaces
{
    public interface IMedicalHistoryService
    {
        Task<IEnumerable<MedicalHistory>> GetAllMedicalHistoriesAsync();
        Task<MedicalHistory?> GetMedicalHistoryByIdAsync(int id);
        Task<IEnumerable<MedicalHistory>> GetMedicalHistoriesByPatientIdAsync(int patientId);
        Task<MedicalHistory> CreateMedicalHistoryAsync(MedicalHistory medicalHistory);
        Task<bool> UpdateMedicalHistoryAsync(MedicalHistory medicalHistory);
        Task<bool> DeleteMedicalHistoryAsync(int id);
        Task<bool> PatientHasConditionAsync(int patientId, string condition);
    }
}