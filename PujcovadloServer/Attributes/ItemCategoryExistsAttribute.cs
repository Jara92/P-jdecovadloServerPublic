using System.ComponentModel.DataAnnotations;
using PujcovadloServer.Business.Services;

namespace PujcovadloServer.Attributes;

[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
public class ItemCategoryExistsAttribute : ValidationAttribute
{
    private readonly bool _allowNull;

    public ItemCategoryExistsAttribute(bool allowNull = false)
    {
        _allowNull = allowNull;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var categoryService = validationContext.GetRequiredService<ItemCategoryService>();

        // Null allowed
        if (_allowNull && value == null)
        {
            return ValidationResult.Success;
        }

        // Convert value to list of category IDs
        var categoryIds = new List<int>();
        if (value is int) categoryIds.Add((int)value);
        else if (value is IEnumerable<int> enumerable) categoryIds.AddRange(enumerable);
        else
        {
            return new ValidationResult("Invalid category ID.");
        }

        // Check each category ID
        foreach (var categoryId in categoryIds)
        {
            var categoryExists = categoryService.Exists(categoryId).ConfigureAwait(false).GetAwaiter().GetResult();

            if (categoryExists == false)
            {
                return new ValidationResult("Category does not exist.");
            }
        }

        return ValidationResult.Success;
    }
}