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
        Task AddAIAssessmentAsync(AIAssessment assessment);       // ADD
        Task AddRecommendationAsync(Recommendation recommendation); // ADD
        Task<AIAssessment?> GetAIAssessmentByConsultationIdAsync(int consultationId);
        
        Task AddMedicationOrderAsync(MedicationOrder order);
        Task<MedicationOrder?> GetOrderByConsultationIdAsync(int consultationId);
        Task UpdateMedicationOrderAsync(MedicationOrder order);
        Task<MedicationOrder?> GetOrderByIdAsync(int orderId);
        Task<IEnumerable<MedicationOrder>> GetPendingOrdersAsync();
        Task AddAttachmentAsync(ConsultationAttachment attachment);
        Task<IEnumerable<ConsultationAttachment>> GetAttachmentsByConsultationIdAsync(int consultationId);
        Task<ConsultationAttachment?> GetAttachmentByIdAsync(int attachmentId);
        Task DeleteAttachmentAsync(int attachmentId);
        Task AddAttachmentRequestAsync(ConsultationAttachment request);
        Task UpdateAttachmentAsync(ConsultationAttachment attachment);
        Task AddMessageAsync(ConsultationMessage message);
        Task<IEnumerable<ConsultationMessage>> GetMessagesByConsultationIdAsync(int consultationId);
        Task MarkMessagesAsReadAsync(int consultationId, string readerUserId);
     
    }
}