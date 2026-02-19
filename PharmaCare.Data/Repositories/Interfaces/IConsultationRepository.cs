using PharmaCare.Data.Models;

namespace PharmaCare.Data.Repositories.Interfaces
{
    public interface IConsultationRepository : IRepository<Consultation>
    {
        Task<Consultation?> GetConsultationWithDetailsAsync(int id);
        Task<IEnumerable<Consultation>> GetConsultationsByPatientWithDetailsAsync(int patientId);
        Task<IEnumerable<Consultation>> GetPendingConsultationsWithDetailsAsync();
        Task<IEnumerable<Consultation>> GetConsultationsByPharmacistAsync(string pharmacistId);
        Task<IEnumerable<Consultation>> GetRecentWithDetailsAsync(int count);
    }
}