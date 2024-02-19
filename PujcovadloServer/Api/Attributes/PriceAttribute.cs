using System.ComponentModel.DataAnnotations;

namespace PujcovadloServer.Api.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public class PriceAttribute : ValidationAttribute
{
    public float MinValue { get; }
    public float MaxValue { get; }

    public PriceAttribute(float minValue, float maxValue)
    {
        MinValue = minValue;
        MaxValue = maxValue;
        ErrorMessage = $"The price must be between {MinValue} and {MaxValue}.";
    }

    public override string FormatErrorMessage(string name)
    {
        return $"The {name} field is not valid.";
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null) return ValidationResult.Success;

        if (value is float price)
        {
            if (price < MinValue) return new ValidationResult($"Price must be greater than {MinValue}.");
            if (price > MaxValue) return new ValidationResult($"Price must be less than {MaxValue}.");
        }

        return ValidationResult.Success;
    }
}