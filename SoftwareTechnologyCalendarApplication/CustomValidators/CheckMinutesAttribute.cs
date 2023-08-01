using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SoftwareTechnologyCalendarApplicationMVC.CustomValidators
{
    [AttributeUsage(AttributeTargets.Property)]

    public class CheckMinutesAttribute : ValidationAttribute, IClientModelValidator
    {
        public override bool IsValid(object value)
        {
            if (value == null || !(value is DateTime dateTime))
            {
                return false;
            }

            // Check if the minutes part of the DateTime is either 00 or 30
            return dateTime.Minute == 0 || dateTime.Minute == 30;
        }

        public override string FormatErrorMessage(string name)
        {
            return $"{name} must have minutes as either 00 or 30.";
        }

        public void AddValidation(ClientModelValidationContext context)
        {
            context.Attributes.Add("data-val", "true");
            context.Attributes.Add("data-val-checkminutes", FormatErrorMessage(context.ModelMetadata.GetDisplayName()));
        }
    }

}