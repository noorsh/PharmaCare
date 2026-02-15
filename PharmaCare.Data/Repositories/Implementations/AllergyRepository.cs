using PharmaCare.Data;
using PharmaCare.Data.Models;
using PharmaCare.Data.Repositories.Interfaces;

namespace PharmaCare.Data.Repositories.Implementations
{
    public class MedicalHistoryRepository : Repository<MedicalHistory>,IMedicalHistoryRepository
    {
        public MedicalHistoryRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
