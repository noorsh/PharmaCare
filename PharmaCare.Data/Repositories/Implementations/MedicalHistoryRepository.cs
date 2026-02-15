using PharmaCare.Data.Models;
using PharmaCare.Data.Repositories.Interfaces;

namespace PharmaCare.Data.Repositories.Implementations
{
    public class AllergyRepository : Repository<Allergy>,IAllergyRepository
    {
        public AllergyRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
