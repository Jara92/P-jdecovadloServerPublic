using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace PujcovadloServer.Areas.Api.Attributes;

/// <summary>
/// Validates that the length of the items is between the specified values.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public class ItemsLength : ValidationAttribute
{
    public int MinLength { get; }
    public int MaxLength { get; }

    public ItemsLength(int minLength, int maxLength)
    {
        MinLength = minLength;
        MaxLength = maxLength;
        ErrorMessage = $"The price must be between {MinLength} and {MaxLength}.";
    }

    public override string FormatErrorMessage(string name)
    {
        return $"The {name} field is not valid.";
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null) return ValidationResult.Success;

        if (value is IList)
        {
            foreach (var item in (IList)value)
            {
                // HERE...
                if (item is string str)
                {
                    if (str.Length < MinLength)
                        return new ValidationResult($"Item length must be greater than {MinLength}.");
                    if (str.Length > MaxLength)
                        return new ValidationResult($"Item length must be less than {MaxLength}.");
                }
            }
        }

        return ValidationResult.Success;
    }
}