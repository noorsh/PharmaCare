using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PharmaCare.Data.Models;

namespace PharmaCare.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Patient> Patients { get; set; }
        public DbSet<MedicalHistory> MedicalHistories { get; set; }
        public DbSet<Allergy> Allergies { get; set; }
        public DbSet<CurrentMedication> CurrentMedications { get; set; }
        public DbSet<Consultation> Consultations { get; set; }
        public DbSet<AIAssessment> AIAssessments { get; set; }
        public DbSet<Recommendation> Recommendations { get; set; }
        public DbSet<Inventory> Inventories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Patient - User relationship (one-to-one)
            modelBuilder.Entity<Patient>()
                .HasOne(p => p.User)
                .WithOne(u => u.Patient)
                .HasForeignKey<Patient>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Patient - MedicalHistory relationship (one-to-many)
            modelBuilder.Entity<MedicalHistory>()
                .HasOne(mh => mh.Patient)
                .WithMany(p => p.MedicalHistories)
                .HasForeignKey(mh => mh.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            // Patient - Allergy relationship (one-to-many)
            modelBuilder.Entity<Allergy>()
                .HasOne(a => a.Patient)
                .WithMany(p => p.Allergies)
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            // Patient - CurrentMedication relationship (one-to-many)
            modelBuilder.Entity<CurrentMedication>()
                .HasOne(cm => cm.Patient)
                .WithMany(p => p.CurrentMedications)
                .HasForeignKey(cm => cm.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            // Patient - Consultation relationship (one-to-many)
            modelBuilder.Entity<Consultation>()
                .HasOne(c => c.Patient)
                .WithMany(p => p.Consultations)
                .HasForeignKey(c => c.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            // Consultation - AIAssessment relationship (one-to-one)
            modelBuilder.Entity<AIAssessment>()
                .HasOne(ai => ai.Consultation)
                .WithOne(c => c.AIAssessment)
                .HasForeignKey<AIAssessment>(ai => ai.ConsultationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Consultation - Recommendation relationship (one-to-one)
            modelBuilder.Entity<Recommendation>()
                .HasOne(r => r.Consultation)
                .WithOne(c => c.Recommendation)
                .HasForeignKey<Recommendation>(r => r.ConsultationId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        public override int SaveChanges()
        {
            ConvertDatesToUtc();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ConvertDatesToUtc();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void ConvertDatesToUtc()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                foreach (var property in entry.Properties)
                {
                    if (property.Metadata.ClrType == typeof(DateTime))
                    {
                        var value = (DateTime?)property.CurrentValue;
                        if (value.HasValue && value.Value.Kind == DateTimeKind.Unspecified)
                        {
                            property.CurrentValue = DateTime.SpecifyKind(value.Value, DateTimeKind.Utc);
                        }
                    }
                    
                    else if (property.Metadata.ClrType == typeof(DateTime?))
                    {
                        var value = (DateTime?)property.CurrentValue;
                        if (value.HasValue && value.Value.Kind == DateTimeKind.Unspecified)
                        {
                            property.CurrentValue = DateTime.SpecifyKind(value.Value, DateTimeKind.Utc);
                        }
                    }
                }
            }
        }
    }
}