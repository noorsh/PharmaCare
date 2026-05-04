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
                .Include(c => c.Patient).ThenInclude(p => p.User)
                .Include(c => c.AIAssessment)
                .Include(c => c.Recommendation).ThenInclude(r => r.Inventory)
                .Include(c => c.Recommendation).ThenInclude(r => r.Pharmacist)
                .Include(c => c.Pharmacist)
                .Include(c => c.MedicationOrder)
                .FirstOrDefaultAsync(c => c.ConsultationId == id);
        }

        public async Task<IEnumerable<Consultation>> GetConsultationsByPatientWithDetailsAsync(int patientId)
        {
            return await _context.Consultations
                .Include(c => c.AIAssessment)
                .Include(c => c.Recommendation)
                .ThenInclude(r => r.Inventory)        // for medication name in dispatched notif
                .Include(c => c.Pharmacist)               // ← needed for pharmacistName
                .Include(c => c.MedicationOrder)          // ← needed for dispatched check
                .ThenInclude(o => o.Inventory)        // ← needed for medicine name
                .Where(c => c.PatientId == patientId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<Consultation>> GetPendingConsultationsWithDetailsAsync()
        {
            return await _context.Consultations
                .Include(c => c.Patient).ThenInclude(p => p.User)
                .Include(c => c.AIAssessment)
                .Include(c => c.Pharmacist)
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
                .Include(c => c.Patient.User)
                .Include(c => c.Pharmacist)
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
        public async Task<AIAssessment?> GetAIAssessmentByConsultationIdAsync(int consultationId)
        {
            return await _context.AIAssessments
                .FirstOrDefaultAsync(a => a.ConsultationId == consultationId);
        }
        public async Task AddMedicationOrderAsync(MedicationOrder order)
        {
            await _context.MedicationOrders.AddAsync(order);
            await _context.SaveChangesAsync();
        }

        public async Task<MedicationOrder?> GetOrderByConsultationIdAsync(int consultationId)
        {
            return await _context.MedicationOrders
                .Include(o => o.Inventory)
                .Include(o => o.Patient)
                .FirstOrDefaultAsync(o => o.ConsultationId == consultationId);
        }

        public async Task UpdateMedicationOrderAsync(MedicationOrder order)
        {
            _context.MedicationOrders.Update(order);
            await _context.SaveChangesAsync();
        }
        public async Task<MedicationOrder?> GetOrderByIdAsync(int orderId)
        {
            return await _context.MedicationOrders
                .Include(o => o.Inventory)
                .Include(o => o.Patient)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
        }
        public async Task<IEnumerable<MedicationOrder>> GetPendingOrdersAsync()
        {
            return await _context.MedicationOrders
                .Include(o => o.Inventory)
                .Include(o => o.Patient)
                .Where(o => o.Status == "Pending")
                .OrderByDescending(o => o.OrderedAt)
                .ToListAsync();
        }
    }
}