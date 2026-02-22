using PharmaCare.Data.Models;
using PharmaCare.Data.Repositories.Interfaces;

namespace PharmaCare.Data.Repositories.Implementations
{
    public class MedicationRepository : Repository<Medication>,IMedicationRepository
    {
        public MedicationRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
