using PharmaCare.Data.Models;

namespace PharmaCare.Services.Interfaces
{
    public interface IConsultationService
    {
        Task<IEnumerable<Consultation>> GetAllConsultationsAsync();
        Task<Consultation?> GetConsultationByIdAsync(int id);
        Task<IEnumerable<Consultation>> GetConsultationsByPatientIdAsync(int patientId);
        Task<Consultation> CreateConsultationAsync(Consultation consultation);
        Task<bool> UpdateConsultationAsync(Consultation consultation);
        Task<bool> DeleteConsultationAsync(int id);
        Task<IEnumerable<Consultation>> GetPendingConsultationsAsync();
        Task<IEnumerable<Consultation>> GetRecentConsultationsAsync(int count = 10);
    }
}