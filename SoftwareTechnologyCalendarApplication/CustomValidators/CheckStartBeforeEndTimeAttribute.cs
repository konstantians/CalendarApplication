using SoftwareTechnologyCalendarApplication.Models;
using System.ComponentModel.DataAnnotations;

namespace SoftwareTechnologyCalendarApplicationMVC.CustomValidators
{
    public class CheckStartBeforeEndTimeAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var eventModel = (Event)validationContext.ObjectInstance;

            // Check if the EndingTime is smaller or equal to the StartingTime
            if (eventModel.StartingTime >= eventModel.EndingTime)
            {
                return new ValidationResult("Ending time must be greater than the starting time of the event.");
            }

            return ValidationResult.Success;
        }
    }
}
