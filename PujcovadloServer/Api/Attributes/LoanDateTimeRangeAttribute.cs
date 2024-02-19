using System.ComponentModel.DataAnnotations;
using PujcovadloServer.Requests;

namespace PujcovadloServer.Api.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class LoanDateTimeRangeAttribute : ValidationAttribute
{
    public override string FormatErrorMessage(string name)
    {
        return $"The {name} field is not valid.";
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value is DateTime toDate && validationContext.ObjectInstance is LoanRequest request)
        {
            DateTime fromDate = request.From ?? DateTime.MinValue;

            if (toDate.Date < fromDate.Date)
            {
                return new ValidationResult("'To' date must be after 'From' date.");
            }

            if (fromDate.Date < DateTime.Now.Date)
            {
                return new ValidationResult("'From' date must be greater than or equal to the current date.");
            }
        }

        return ValidationResult.Success;
    }
}