using Microsoft.Extensions.Logging;
using PharmaCare.Data.Models;
using PharmaCare.Data.Repositories.Interfaces;
using PharmaCare.Services.Interfaces;

namespace PharmaCare.Services.Implementations
{
    /// <summary>
    /// Placeholder AI assessment service.
    /// This will be replaced by calls to the Python microservice in a later phase.
    /// </summary>
    public class AIAssessmentService : IAIAssessmentService
    {
        private readonly IConsultationRepository _consultationRepository;
        private readonly ILogger<AIAssessmentService> _logger;

        public AIAssessmentService(
            IConsultationRepository consultationRepository,
            ILogger<AIAssessmentService> logger)
        {
            _consultationRepository = consultationRepository;
            _logger = logger;
        }

        public async Task<AIAssessment> GenerateAssessmentAsync(Consultation consultation)
        {
            _logger.LogInformation($"Generating AI assessment for consultation {consultation.ConsultationId}");

            // --- PLACEHOLDER LOGIC ---
            // In Phase 2, this method will call the Python microservice via HTTP.
            // For now, it returns a stub response based on keywords in the symptoms.

            await Task.Delay(500); // Simulate async processing

            var symptoms = consultation.Symptoms.ToLower();
            var assessment = new AIAssessment
            {
                ConsultationId = consultation.ConsultationId,
                GeneratedAt = DateTime.UtcNow,
                ConfidenceScore = 65.0m, // Default placeholder confidence
            };

            // Simple keyword matching for demo purposes
            if (symptoms.Contains("headache") || symptoms.Contains("head"))
            {
                assessment.PossibleConditions = "Tension headache, Migraine, Dehydration";
                assessment.SuggestedQuestions = "Is the pain throbbing or constant? Any sensitivity to light or sound? When did it start?";
                assessment.RedFlags = "Sudden severe headache, headache with fever and stiff neck, headache after head injury";
                assessment.AssessmentReport = "Patient reports headache symptoms. Common causes include tension headache or migraine. Pharmacist should assess pain severity, duration, and any associated neurological symptoms.";
                assessment.ConfidenceScore = 70.0m;
            }
            else if (symptoms.Contains("fever") || symptoms.Contains("temperature"))
            {
                assessment.PossibleConditions = "Viral infection, Bacterial infection, Flu";
                assessment.SuggestedQuestions = "What is the temperature reading? Any chills or sweating? Any other symptoms like cough or sore throat?";
                assessment.RedFlags = "Temperature above 39.5°C, fever lasting more than 3 days, fever with rash or difficulty breathing";
                assessment.AssessmentReport = "Patient reports fever. Pharmacist should assess temperature, duration, and accompanying symptoms to determine if OTC treatment is appropriate or physician referral is needed.";
                assessment.ConfidenceScore = 75.0m;
            }
            else if (symptoms.Contains("cough") || symptoms.Contains("throat"))
            {
                assessment.PossibleConditions = "Common cold, Pharyngitis, Bronchitis";
                assessment.SuggestedQuestions = "Is the cough dry or productive? Any fever? How long have symptoms been present?";
                assessment.RedFlags = "Coughing up blood, difficulty breathing, chest pain with cough";
                assessment.AssessmentReport = "Patient reports respiratory symptoms. Likely viral upper respiratory tract infection. Pharmacist should assess duration and severity before recommending OTC remedies.";
                assessment.ConfidenceScore = 72.0m;
            }
            else if (symptoms.Contains("stomach") || symptoms.Contains("nausea") || symptoms.Contains("vomit"))
            {
                assessment.PossibleConditions = "Gastroenteritis, Food poisoning, Indigestion";
                assessment.SuggestedQuestions = "Any vomiting or diarrhea? Did symptoms start after eating? Any blood in stool?";
                assessment.RedFlags = "Signs of severe dehydration, blood in vomit or stool, severe abdominal pain";
                assessment.AssessmentReport = "Patient reports gastrointestinal symptoms. Pharmacist should assess hydration status and rule out red flag symptoms requiring urgent care.";
                assessment.ConfidenceScore = 68.0m;
            }
            else
            {
                // Generic fallback
                assessment.PossibleConditions = "Requires further assessment";
                assessment.SuggestedQuestions = "How long have you had these symptoms? Are they getting better or worse? Any relevant medical history?";
                assessment.RedFlags = "Any sudden worsening of symptoms, difficulty breathing, chest pain, or loss of consciousness";
                assessment.AssessmentReport = $"Patient reports: {consultation.Symptoms}. Insufficient symptom data for confident AI classification. Pharmacist should conduct a thorough assessment.";
                assessment.ConfidenceScore = 40.0m;
            }

            return assessment;
        }

        public async Task<AIAssessment> SaveAssessmentAsync(AIAssessment assessment)
        {
            await _consultationRepository.AddAIAssessmentAsync(assessment);
            await _consultationRepository.SaveChangesAsync();
            _logger.LogInformation($"AIAssessment saved for consultation {assessment.ConsultationId}");
            return assessment;
        }
        public async Task<AIAssessment?> GetAssessmentByConsultationIdAsync(int consultationId)
        {
            var consultation = await _consultationRepository.GetConsultationWithDetailsAsync(consultationId);
            return consultation?.AIAssessment;
        }
        
    }
}