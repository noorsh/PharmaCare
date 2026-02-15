
using PharmaCare.Data.Models;
using PharmaCare.Data.Repositories.Interfaces;

namespace PharmaCare.Data.Repositories.Implementations
{
    public class CurrentMedicationRepository : Repository<CurrentMedication>,ICurrentMedicationRepository
    {
        public CurrentMedicationRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
