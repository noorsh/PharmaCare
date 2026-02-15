
using PharmaCare.Data.Models;
using PharmaCare.Data.Repositories.Interfaces;

namespace PharmaCare.Data.Repositories.Implementations
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        // Repository instances
        private IPatientRepository? _patients;
        private IRepository<ApplicationUser>? _users;
        private IRepository<Pharmacist>? _pharmacists;
        private IRepository<Medication>? _medications;
        private IRepository<Consultation>? _consultations;
        private IRepository<AIAssessment>? _aiAssessments;
        private IRepository<Recommendation>? _recommendations;
        private IRepository<Allergy>? _allergies;
        private IRepository<MedicalHistory>? _medicalHistories;
        private IRepository<CurrentMedication>? _currentMedications;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        // Lazy initialization of repositories
        public IPatientRepository Patients
        {
            get
            {
                return _patients ??= new PatientRepository(_context);
            }
        }

        public IRepository<ApplicationUser> Users
        {
            get
            {
                return _users ??= new Repository<ApplicationUser>(_context);
            }
        }

        public IRepository<Pharmacist> Pharmacists
        {
            get
            {
                return _pharmacists ??= new Repository<Pharmacist>(_context);
            }
        }

        public IRepository<Medication> Medications
        {
            get
            {
                return _medications ??= new Repository<Medication>(_context);
            }
        }

        public IRepository<Consultation> Consultations
        {
            get
            {
                return _consultations ??= new Repository<Consultation>(_context);
            }
        }

        public IRepository<AIAssessment> AIAssessments
        {
            get
            {
                return _aiAssessments ??= new Repository<AIAssessment>(_context);
            }
        }

        public IRepository<Recommendation> Recommendations
        {
            get
            {
                return _recommendations ??= new Repository<Recommendation>(_context);
            }
        }

        public IRepository<Allergy> Allergies
        {
            get
            {
                return _allergies ??= new Repository<Allergy>(_context);
            }
        }

        public IRepository<MedicalHistory> MedicalHistories
        {
            get
            {
                return _medicalHistories ??= new Repository<MedicalHistory>(_context);
            }
        }

        public IRepository<CurrentMedication> CurrentMedications
        {
            get
            {
                return _currentMedications ??= new Repository<CurrentMedication>(_context);
            }
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public int SaveChanges()
        {
            return _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}