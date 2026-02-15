using System.ComponentModel.DataAnnotations;

namespace PharmaCare.Data.Models;

public class Enums
{
    public enum SmokingStatus
    {
        Never = 1,
        Former,
        Current
    }

    public enum AlcoholConsumption
    {
        None = 1,
        Occasional,
        Regular
    }

    public enum ExerciseFrequency
    {
        Sedentary = 1,
        Light,
        Moderate,
        Active
    }
    public enum BloodType
    {
        [Display(Name = "A+")] A_Positive = 1,
        [Display(Name = "A-")] A_Negative,
        [Display(Name = "B+")] B_Positive,
        [Display(Name = "B-")] B_Negative,
        [Display(Name = "AB+")] AB_Positive,
        [Display(Name = "AB-")] AB_Negative,
        [Display(Name = "O+")] O_Positive,
        [Display(Name = "O-")] O_Negative
    }
    public enum AllergyType
    {
        Medication = 1,
        Food,
        Environmental,
        Other
    }

    public enum AllergySeverity
    {
        Mild = 1,
        Moderate,
        Severe, 
        [Display(Name = "Life Threatening")] LifeThreatening
    }

}