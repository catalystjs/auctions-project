// Common Libraries
using System;
// Validation Annotations
using System.ComponentModel.DataAnnotations;

namespace beltexam.ModelValidations
{
    public class CheckDateRangeAttribute: ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            DateTime dt = (DateTime)value;
            if (dt >= DateTime.Now)
            {
                return ValidationResult.Success;
            }
            return new ValidationResult(ErrorMessage ?? "Make sure your date is greater than today");
        }
    }
}