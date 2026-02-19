using System.ComponentModel.DataAnnotations;

namespace PharmaCare.MVC.Models.ViewModels
{
    public class SubmitConsultationViewModel
    {
        [Required(ErrorMessage = "Please describe your symptoms")]
        [MaxLength(2000)]
        [Display(Name = "Describe your symptoms")]
        public string Symptoms { get; set; }

        [MaxLength(50)]
        [Display(Name = "How long have you had these symptoms?")]
        public string? SymptomDuration { get; set; }

        [Required(ErrorMessage = "Please select severity")]
        [Display(Name = "Symptom Severity")]
        public string SymptomSeverity { get; set; }

        [MaxLength(1000)]
        [Display(Name = "Additional information (optional)")]
        public string? AdditionalInformation { get; set; }
    }
}