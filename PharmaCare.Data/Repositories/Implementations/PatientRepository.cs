using Microsoft.EntityFrameworkCore;
using PharmaCare.Data.Models;
using PharmaCare.Data.Repositories.Interfaces;

namespace PharmaCare.Data.Repositories.Implementations
{
    public class PatientRepository : Repository<Patient>, IPatientRepository
    {
        public PatientRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Patient?> GetPatientWithDetailsAsync(int patientId)
        {
            return await _dbSet
                .Include(p => p.User)
                .Include(p => p.Allergies)
                .Include(p => p.MedicalHistories)
                .Include(p => p.CurrentMedications)
                .Include(p => p.Consultations)
                .FirstOrDefaultAsync(p => p.PatientId == patientId);
        }

        public async Task<IEnumerable<Patient>> SearchPatientsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllAsync();

            searchTerm = searchTerm.ToLower();

            return await _dbSet
                .Include(p => p.User)
                .Where(p => 
                    p.User.FirstName.ToLower().Contains(searchTerm) ||
                    p.User.LastName.ToLower().Contains(searchTerm) ||
                    p.User.Email.ToLower().Contains(searchTerm) ||
                    p.User.PhoneNumber.Contains(searchTerm) ||
                    p.City.ToLower().Contains(searchTerm))
                .ToListAsync();
        }

        public async Task<Patient?> GetPatientByEmailAsync(string email)
        {
            return await _dbSet
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.User.Email.ToLower() == email.ToLower());
        }

        public async Task<Patient?> GetPatientByPhoneAsync(string phone)
        {
            return await _dbSet
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.User.PhoneNumber == phone);
        }

        public async Task<bool> IsEmailExistsAsync(string email, int? excludePatientId = null)
        {
            var query = _dbSet
                .Include(p => p.User)
                .Where(p => p.User.Email.ToLower() == email.ToLower());
            
            if (excludePatientId.HasValue)
                query = query.Where(p => p.PatientId != excludePatientId.Value);
            
            return await query.AnyAsync();
        }

        public async Task<bool> IsPhoneExistsAsync(string phone, int? excludePatientId = null)
        {
            var query = _dbSet
                .Include(p => p.User)
                .Where(p => p.User.PhoneNumber == phone);
            
            if (excludePatientId.HasValue)
                query = query.Where(p => p.PatientId != excludePatientId.Value);
            
            return await query.AnyAsync();
        }

        public async Task<Patient?> GetPatientByUserIdAsync(string userId)
        {
            return await _dbSet
                .Include(p => p.User)
                .Include(p => p.Allergies)
                .Include(p => p.MedicalHistories)
                .Include(p => p.CurrentMedications)
                .FirstOrDefaultAsync(p => p.UserId == userId);
        }
    }
}