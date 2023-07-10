using System.ComponentModel.DataAnnotations;
using System;

namespace SoftwareTechnologyCalendarApplication.SpareClasses
{
    public sealed class CheckMinutes : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            DateTime dt = (DateTime)value;

            if (dt.Minute==00 || dt.Minute == 30)
            {
                return ValidationResult.Success;
            }
            else
            {
                return new ValidationResult("The app supports only :00 and :30 for the minute of time of the event.");
            }
        }
    }
}
