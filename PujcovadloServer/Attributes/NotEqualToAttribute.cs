using System.ComponentModel.DataAnnotations;

namespace PujcovadloServer.Attributes;

/// <summary>
/// Attribute that checks if the value of the property is not equal to the value of another property.
/// Validation fails if both values are equal.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
public class NotEqualToAttribute : ValidationAttribute
{
    private readonly string _otherFieldName;
    private readonly bool _allowBothNull;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotEqualToAttribute"/> class.
    /// </summary>
    /// <param name="otherFieldName">Other fields name</param>
    /// <param name="allowBothNull">If true and both properties are null, validation won't fail</param>
    /// <exception cref="ArgumentNullException"></exception>
    public NotEqualToAttribute(string otherFieldName, bool allowBothNull = true)
    {
        _otherFieldName = otherFieldName ?? throw new ArgumentNullException(nameof(otherFieldName));
        _allowBothNull = allowBothNull;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var otherPropertyInfo = validationContext.ObjectType.GetProperty(_otherFieldName);

        if (otherPropertyInfo == null)
        {
            return new ValidationResult($"Property with name {_otherFieldName} not found.");
        }

        var otherValue = otherPropertyInfo.GetValue(validationContext.ObjectInstance, null);

        // if both values are null and we allow it, return success
        if (_allowBothNull && value == null && otherValue == null)
        {
            return ValidationResult.Success;
        }

        // if both values are equal, return error
        if (Equals(value, otherValue))
        {
            // if no custom error message is set, return default
            if (ErrorMessage == null)
            {
                return new ValidationResult(
                    $"{validationContext.DisplayName} cannot be the same as {_otherFieldName}.");
            }

            return new ValidationResult(ErrorMessage);
        }

        return ValidationResult.Success;
    }
}