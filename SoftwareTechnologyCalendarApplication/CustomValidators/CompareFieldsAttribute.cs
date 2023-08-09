namespace SoftwareTechnologyCalendarApplicationMVC.CustomValidators
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class CompareFieldsAttribute : ValidationAttribute
    {
        private readonly string _otherProperty;

        public CompareFieldsAttribute(string otherProperty)
        {
            _otherProperty = otherProperty;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var otherProperty = validationContext.ObjectType.GetProperty(_otherProperty);

            if (otherProperty == null)
            {
                throw new ArgumentException($"Property {_otherProperty} not found.");
            }

            var otherValue = otherProperty.GetValue(validationContext.ObjectInstance);

            if (!Equals(value, otherValue))
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }
}
