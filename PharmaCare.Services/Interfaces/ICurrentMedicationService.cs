using PharmaCare.Data.Models;

namespace PharmaCare.Services.Interfaces
{
    public interface ICurrentMedicationService
    {
        Task<IEnumerable<CurrentMedication>> GetAllCurrentMedicationsAsync();
        Task<CurrentMedication?> GetCurrentMedicationByIdAsync(int id);
        Task<IEnumerable<CurrentMedication>> GetCurrentMedicationsByPatientIdAsync(int patientId);
        Task<CurrentMedication> CreateCurrentMedicationAsync(CurrentMedication currentMedication);
        Task<bool> UpdateCurrentMedicationAsync(CurrentMedication currentMedication);
        Task<bool> DeleteCurrentMedicationAsync(int id);
        Task<bool> PatientIsTakingMedicationAsync(int patientId, string medicationName);
        Task<IEnumerable<CurrentMedication>> GetActiveMedicationsByPatientIdAsync(int patientId);
    }
}