using System.ComponentModel.DataAnnotations;
using PujcovadloServer.Business.Enums;

namespace PujcovadloServer.Attributes;

/// <summary>
/// A validation attribute that checks if the array items belongs to the <see cref="UserRoles"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public class UserRolesAttribute : ValidationAttribute
{
    private readonly string[] _allowedRoles;

    /// <summary>
    /// By default, all roles <see cref="UserRoles"/> are allowed.
    /// </summary>
    public UserRolesAttribute()
    {
        _allowedRoles = UserRoles.AllRoles;
    }

    /// <summary>
    /// Only the specified roles are allowed.
    /// </summary>
    public UserRolesAttribute(params string[] roles)
    {
        _allowedRoles = roles;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is List<string> roles)
        {
            // Check if all roles are in the allowedRoles array
            if (roles.All(r => _allowedRoles.Contains(r)))
            {
                return ValidationResult.Success;
            }
        }

        // Validation failed
        return new ValidationResult($"Invalid role. Allowed roles are: {string.Join(", ", _allowedRoles)}");
    }
}