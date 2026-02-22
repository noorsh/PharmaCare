using Microsoft.EntityFrameworkCore;
using PharmaCare.Data.Models;
using PharmaCare.Data.Repositories.Interfaces;

namespace PharmaCare.Data.Repositories.Implementations
{
    public class ConsultationRepository : Repository<Consultation>, IConsultationRepository
    {
        private readonly ApplicationDbContext _context;

        public ConsultationRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Consultation?> GetConsultationWithDetailsAsync(int id)
        {
            return await _context.Consultations
                .Include(c => c.Patient)
                .Include(c => c.AIAssessment)
                .Include(c => c.Recommendation)
                .FirstOrDefaultAsync(c => c.ConsultationId == id);
        }

        public async Task<IEnumerable<Consultation>> GetConsultationsByPatientWithDetailsAsync(int patientId)
        {
            return await _context.Consultations
                .Include(c => c.AIAssessment)
                .Include(c => c.Recommendation)
                .Where(c => c.PatientId == patientId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Consultation>> GetPendingConsultationsWithDetailsAsync()
        {
            return await _context.Consultations
                .Include(c => c.Patient)
                .Include(c => c.AIAssessment)
                .Where(c => c.Status == "Pending" || c.Status == "UnderReview")
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Consultation>> GetConsultationsByPharmacistAsync(string pharmacistId)
        {
            return await _context.Consultations
                .Include(c => c.Patient)
                .Include(c => c.AIAssessment)
                .Include(c => c.Recommendation)
                .Where(c => c.PharmacistId == pharmacistId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Consultation>> GetRecentWithDetailsAsync(int count)
        {
            return await _context.Consultations
                .Include(c => c.Patient)
                .Include(c => c.AIAssessment)
                .OrderByDescending(c => c.CreatedAt)
                .Take(count)
                .ToListAsync();
        }
        public async Task AddAIAssessmentAsync(AIAssessment assessment)
        {
            await _context.AIAssessments.AddAsync(assessment);
        }

        public async Task AddRecommendationAsync(Recommendation recommendation)
        {
            await _context.Recommendations.AddAsync(recommendation);
        }
    }
}