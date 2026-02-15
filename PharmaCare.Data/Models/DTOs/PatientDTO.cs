namespace PharmaCare.Data.DTOs
{
    public class PatientDto
    {
        public int PatientId { get; set; }
        public string UserId { get; set; }
        
        // User information
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        
        // Personal Information
        public DateTime DateOfBirth { get; set; }
        public int Age { get; set; }
        public string Gender { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? EmergencyContact { get; set; }
        
        // Medical Profile
        public decimal? Height { get; set; }
        public decimal? Weight { get; set; }
        public decimal? BMI { get; set; }
        public string? BloodType { get; set; }
        
        // Lifestyle Factors
        public string? SmokingStatus { get; set; }
        public string? AlcoholConsumption { get; set; }
        public string? ExerciseFrequency { get; set; }
        public bool IsPregnant { get; set; }
        public bool IsBreastfeeding { get; set; }
        
        public string? AdditionalNotes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}