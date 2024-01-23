using System.ComponentModel.DataAnnotations;
using PujcovadloServer.Enums;
using PujcovadloServer.Models;

namespace PujcovadloServer.Requests;

public class ItemCategoryRequest
{
    public int? Id { get; set; }
    
    public string Name { get; set; } = default!;
    
    public string? Alias { get; set; }
    
    public string Description { get; set; } = default!;
}