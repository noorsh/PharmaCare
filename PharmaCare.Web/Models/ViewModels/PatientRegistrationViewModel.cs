using System.ComponentModel.DataAnnotations;
using PharmaCare.Data.Models;

namespace PharmaCare.MVC.Models.ViewModels
{
    public class PatientRegistrationViewModel
    {
        // User Account Information
        [Required(ErrorMessage = "First name is required")]
        [Display(Name = "First Name")]
        [StringLength(100)]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [Display(Name = "Last Name")]
        [StringLength(100)]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Please confirm your password")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }

        // Patient Profile Information
        [Required(ErrorMessage = "Date of birth is required")]
        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        [Display(Name = "Gender")]
        public string Gender { get; set; }

        [Display(Name = "Address")]
        [StringLength(200)]
        public string? Address { get; set; }

        [Display(Name = "City")]
        [StringLength(100)]
        public string? City { get; set; }

        [Display(Name = "Emergency Contact")]
        [Phone]
        public string? EmergencyContact { get; set; }

        // Medical Profile
        [Display(Name = "Height (cm)")]
        [Range(0, 300, ErrorMessage = "Height must be between 0 and 300 cm")]
        public decimal? Height { get; set; }

        [Display(Name = "Weight (kg)")]
        [Range(0, 500, ErrorMessage = "Weight must be between 0 and 500 kg")]
        public decimal? Weight { get; set; }

        [Display(Name = "Blood Type")]
        public Enums.BloodType? BloodType { get; set; }

        // Lifestyle
        [Display(Name = "Smoking Status")]
        public Enums.SmokingStatus? SmokingStatus { get; set; }

        [Display(Name = "Alcohol Consumption")]
        public Enums.AlcoholConsumption? AlcoholConsumption { get; set; }

        [Display(Name = "Exercise Frequency")]
        public Enums.ExerciseFrequency? ExerciseFrequency { get; set; }

        [Display(Name = "Are you pregnant?")]
        public bool IsPregnant { get; set; }

        [Display(Name = "Are you breastfeeding?")]
        public bool IsBreastfeeding { get; set; }

        // NEW: Critical Safety Flags
        [Display(Name = "Do you have chronic kidney disease?")]
        public bool HasKidneyDisease { get; set; }

        [Display(Name = "Do you have chronic liver disease?")]
        public bool HasLiverDisease { get; set; }

        [Display(Name = "Do you have any known drug allergies?")]
        public bool HasDrugAllergies { get; set; }

        [Display(Name = "If yes, please specify drug allergies")]
        [StringLength(500)]
        public string? DrugAllergyDetails { get; set; }

        [Display(Name = "Additional Notes")]
        [StringLength(1000)]
        public string? AdditionalNotes { get; set; }
    }
}