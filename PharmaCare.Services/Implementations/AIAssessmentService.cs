using Microsoft.Extensions.Logging;
using PharmaCare.Data.Models;
using PharmaCare.Data.Repositories.Interfaces;
using PharmaCare.Services.Interfaces;
using System.Text;
using System.Text.Json;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace PharmaCare.Services.Implementations
{
    public class AIAssessmentService : IAIAssessmentService
    {
        private readonly IConsultationRepository _consultationRepository;
        private readonly ILogger<AIAssessmentService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public AIAssessmentService(
            IConsultationRepository consultationRepository,
            ILogger<AIAssessmentService> logger,
            IHttpClientFactory httpClientFactory)
        {
            _consultationRepository = consultationRepository;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<AIAssessment> GenerateAssessmentAsync(Consultation consultation)
        {
            _logger.LogInformation($"Generating AI assessment for consultation {consultation.ConsultationId}");

            var assessment = new AIAssessment
            {
                ConsultationId = consultation.ConsultationId,
                GeneratedAt = DateTime.UtcNow,
            };

            try
            {
                // Extract symptom keywords from the free-text symptoms field
                var symptoms = ExtractSymptoms(consultation.Symptoms);

                // Call the Python Flask microservice
                var client = _httpClientFactory.CreateClient("AIService");

                var payload = new { symptoms };
                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("predict", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var result = System.Text.Json.JsonSerializer.Deserialize<AIPredictionResult>(responseJson,
                        new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }); 
                    if (result != null)
                    {
                        assessment.PossibleConditions = result.Disease;
                        assessment.ConfidenceScore = (decimal)(result.Confidence * 100);
                        assessment.AssessmentReport =
                            $"AI analysis identified: {result.Disease} " +
                            $"(confidence: {result.Confidence * 100:F0}%). " +
                            $"Matched symptoms: {string.Join(", ", result.MatchedSymptoms)}. " +
                            $"Pharmacist review required before any recommendation is made.";
                        assessment.SuggestedQuestions =
                            result.UnrecognizedSymptoms.Count > 0
                                ? $"Could not match these reported symptoms: {string.Join(", ", result.UnrecognizedSymptoms)}. Please clarify with patient."
                                : "All reported symptoms were recognized by the AI model.";
                        assessment.RedFlags =
                            "This is an AI-generated suggestion only. " +
                            "Pharmacist must review and make the final recommendation.";

                        _logger.LogInformation(
                            $"AI prediction: {result.Disease} with {result.Confidence * 100:F0}% confidence");
                    }
                }
                else
                {
                    _logger.LogWarning($"AI service returned {response.StatusCode} — using fallback");
                    ApplyFallback(assessment, consultation.Symptoms);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI service call failed — using fallback assessment");
                ApplyFallback(assessment, consultation.Symptoms);
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
            return await _consultationRepository.GetAIAssessmentByConsultationIdAsync(consultationId);
        }

        // ── Helpers ────────────────────────────────────────────────────────

        private static List<string> ExtractSymptoms(string symptomsText)
        {
            if (string.IsNullOrWhiteSpace(symptomsText))
                return new List<string>();

            // Normalize: lowercase, split on delimiters (NOT on '.' to preserve phrases)
            var delimiters = new[] { ',', ';', '\n', '\r' };

            return symptomsText
                .Split(delimiters, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim().ToLower())
                .Where(s => s.Length > 2)
                // Remove non-informative button answers
                .Where(s => !new[]
                {
                    "no medications", "took a painkiller", "taking prescribed medication",
                    "tried home remedies", "mild", "moderate", "severe",
                    "less than 24 hours", "1–2 days", "3–7 days", "1–2 weeks",
                    "more than 2 weeks", "no", "yes", "none"
                }.Contains(s))
                // Convert spaces to underscores to match Flask model format
                .Select(s => Regex.Replace(s.Replace(' ', '_'), @"_+", "_").Trim('_'))
                .Where(s => s.Length > 2)
                .Distinct()
                .ToList();
        }

        private static void ApplyFallback(AIAssessment assessment, string symptoms)
        {
            assessment.PossibleConditions = "Unable to determine — manual assessment required";
            assessment.ConfidenceScore = 0;
            assessment.AssessmentReport =
                "AI service was unavailable. Pharmacist should assess the patient's symptoms manually.";
            assessment.SuggestedQuestions =
                "Please ask the patient to describe symptoms in detail.";
            assessment.RedFlags =
                "AI assessment failed. Full pharmacist review is mandatory.";
        }

        // ── Response model ─────────────────────────────────────────────────

        private class AIPredictionResult
        {
            public string Disease { get; set; } = string.Empty;
            public double Confidence { get; set; }
            public List<string> MatchedSymptoms { get; set; } = new();
            public List<string> UnrecognizedSymptoms { get; set; } = new();
        }
    }
}