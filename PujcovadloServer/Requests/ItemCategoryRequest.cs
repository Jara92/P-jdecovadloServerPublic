using System.ComponentModel.DataAnnotations;

namespace PujcovadloServer.Requests;

public class ItemCategoryRequest : EntityRequest
{
    
    public string Name { get; set; } = default!;
    
    public string? Alias { get; set; }
    
    public string Description { get; set; } = default!;
}