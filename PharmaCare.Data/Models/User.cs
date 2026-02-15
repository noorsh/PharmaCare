
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;


namespace PharmaCare.Data.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; }

        [Required]
        public UserType UserType { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        // Navigation property - if user is a patient
        public Patient? Patient { get; set; }
    }
}