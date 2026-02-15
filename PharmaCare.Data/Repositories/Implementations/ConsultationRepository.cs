using PharmaCare.Data;
using PharmaCare.Data.Models;
using PharmaCare.Data.Repositories.Interfaces;

namespace PharmaCare.Data.Repositories.Implementations
{
    public class ConsultationRepository : Repository<Consultation>,IConsultationRepository
    {
        public ConsultationRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
