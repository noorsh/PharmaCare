using PharmaCare.Data.Models;

namespace PharmaCare.Data.Repositories.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        // Repositories
        IPatientRepository Patients { get; }
        IRepository<ApplicationUser> Users { get; }
        IRepository<Pharmacist> Pharmacists { get; }
        IRepository<Medication> Medications { get; }
        IRepository<Consultation> Consultations { get; }
        IRepository<AIAssessment> AIAssessments { get; }
        IRepository<Recommendation> Recommendations { get; }
        IRepository<Allergy> Allergies { get; }
        IRepository<MedicalHistory> MedicalHistories { get; }
        IRepository<CurrentMedication> CurrentMedications { get; }
        // Removed UserTypes - it's an enum, not an entity

        // Save changes
        Task<int> SaveChangesAsync();
        int SaveChanges();
    }
}