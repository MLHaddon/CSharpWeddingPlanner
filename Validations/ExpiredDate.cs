using System;
using System.ComponentModel.DataAnnotations;
namespace WeddingPlanner.Validations
{
    public class ExpiredDate : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            DateTime compare = (DateTime)value;
            if(compare <= DateTime.Now)
            {
                return new ValidationResult("Date must be in the future.");
            }
            else
            {
                return ValidationResult.Success;
            }
        }
    }
}