namespace PharmaCare.Data.DTOs
{
    public class UpdatePatientDto
    {
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? EmergencyContact { get; set; }
        public decimal? Height { get; set; }
        public decimal? Weight { get; set; }
        public string? BloodType { get; set; }
        public string? SmokingStatus { get; set; }
        public string? AlcoholConsumption { get; set; }
        public string? ExerciseFrequency { get; set; }
        public bool IsPregnant { get; set; }
        public bool IsBreastfeeding { get; set; }
        public string? AdditionalNotes { get; set; }
    }
}