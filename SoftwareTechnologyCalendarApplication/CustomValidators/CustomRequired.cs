using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace SoftwareTechnologyCalendarApplicationMVC.CustomValidators
{
    public class CustomRequiredAttribute : ValidationAttribute, IClientModelValidator
    {
        public override bool IsValid(object value)
        {
            return value != null && !string.IsNullOrWhiteSpace(value.ToString());
        }

        public override string FormatErrorMessage(string name)
        {
            return $"You need to provide a {name} for the event before submiting.";
        }

        public void AddValidation(ClientModelValidationContext context)
        {
            context.Attributes.Add("data-val", "true");
            context.Attributes.Add("data-val-customrequired", FormatErrorMessage(context.ModelMetadata.GetDisplayName()));
        }
    }
}