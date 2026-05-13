using Microsoft.AspNetCore.Http;
using PharmaCare.Data.Models;

namespace PharmaCare.Services.Interfaces
{
    public interface IConsultationService
    {
        // Existing
        Task<IEnumerable<Consultation>> GetAllConsultationsAsync();
        Task<Consultation?> GetConsultationByIdAsync(int id);
        Task<IEnumerable<Consultation>> GetConsultationsByPatientIdAsync(int patientId);
        Task<Consultation> CreateConsultationAsync(Consultation consultation);
        Task<bool> UpdateConsultationAsync(Consultation consultation);
        Task<bool> DeleteConsultationAsync(int id);
        Task<IEnumerable<Consultation>> GetPendingConsultationsAsync();
        Task<IEnumerable<Consultation>> GetRecentConsultationsAsync(int count = 10);

        // New - with navigation properties loaded
        Task<Consultation?> GetConsultationWithDetailsAsync(int id);
        Task<IEnumerable<Consultation>> GetConsultationsByPatientWithDetailsAsync(int patientId);
        Task<IEnumerable<Consultation>> GetPendingConsultationsWithDetailsAsync();

        // Pharmacist workflow
        Task<bool> AssignPharmacistAsync(int consultationId, string pharmacistId);
        Task<bool> CompleteConsultationAsync(int consultationId, string pharmacistId);
        Task<bool> CancelConsultationAsync(int consultationId);
        Task<IEnumerable<Consultation>> GetConsultationsByPharmacistAsync(string pharmacistId);
        
        Task<IEnumerable<Consultation>> GetRecentConsultationsWithDetailsAsync(int count = 10);
        Task<bool> SubmitRecommendationAsync(int consultationId, string pharmacistId, Recommendation recommendation);
        
        
        Task<bool> PlaceOrderAsync(int consultationId, string patientUserId, string deliveryAddress);
        Task<bool> DispatchOrderAsync(int orderId, string pharmacistNotes);
        Task<MedicationOrder?> GetOrderByConsultationIdAsync(int consultationId);
        Task<IEnumerable<MedicationOrder>> GetPendingOrdersAsync();
        Task<ConsultationAttachment?> SaveAttachmentAsync(
            int consultationId,
            string uploadedByUserId,
            IFormFile file,
            string fileType,
            string appDataPath);

        Task<IEnumerable<ConsultationAttachment>> GetAttachmentsAsync(int consultationId);
        Task<ConsultationAttachment?> GetAttachmentByIdAsync(int attachmentId);
        Task<bool> DeleteAttachmentAsync(int attachmentId, string requestingUserId, string appDataPath);
        Task RequestAttachmentAsync(int consultationId, string pharmacistUserId, string requestNote);
        Task<bool> FulfillAttachmentRequestAsync(int attachmentId);
        Task SendMessageAsync(int consultationId, string senderUserId, string senderRole, string message);
        Task<IEnumerable<ConsultationMessage>> GetMessagesAsync(int consultationId);
        Task MarkMessagesAsReadAsync(int consultationId, string readerUserId);
    }
}