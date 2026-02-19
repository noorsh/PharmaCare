using PharmaCare.Data.Models;

namespace PharmaCare.Services.Interfaces
{
    public interface IAIAssessmentService
    {
        Task<AIAssessment> GenerateAssessmentAsync(Consultation consultation);
        Task<AIAssessment?> GetAssessmentByConsultationIdAsync(int consultationId);
    }
}