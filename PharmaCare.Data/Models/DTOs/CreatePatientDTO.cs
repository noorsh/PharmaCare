using System.ComponentModel.DataAnnotations;

namespace PharmaCare.Data.DTOs
{
    public class CreatePatientDto
    {
        // User Account Information
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        [Required]
        [MinLength(6)]
        public string Password { get; set; }
        
        [Required]
        public string FirstName { get; set; }
        
        [Required]
        public string LastName { get; set; }
        
        [Required]
        [Phone]
        public string PhoneNumber { get; set; }
        
        // Personal Information
        [Required]
        public DateTime DateOfBirth { get; set; }
        
        [Required]
        public string Gender { get; set; }
        
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? EmergencyContact { get; set; }
        
        // Medical Profile
        public decimal? Height { get; set; }
        public decimal? Weight { get; set; }
        public string? BloodType { get; set; }
        
        // Lifestyle Factors
        public string? SmokingStatus { get; set; }
        public string? AlcoholConsumption { get; set; }
        public string? ExerciseFrequency { get; set; }
        public bool IsPregnant { get; set; }
        public bool IsBreastfeeding { get; set; }
        
        public string? AdditionalNotes { get; set; }
    }
}