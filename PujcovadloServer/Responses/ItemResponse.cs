using System.ComponentModel.DataAnnotations;
using PujcovadloServer.Enums;
using PujcovadloServer.Models;

namespace PujcovadloServer.Responses;

public class ItemResponse
{
    public int Id { get; set; }
    
    public string Name { get; set; } = default!;
    
    public string Alias { get; set; } = default!;
    
    public string Description { get; set; } = default!;
    
    public ItemStatus Status { get; set; } = ItemStatus.Public;
    
    public string Parameters { get; set; } = "";
    
    public float PricePerDay { get; set; }
    
    public float? RefundableDeposit { get; set; }
    
    public float? SellingPrice { get; set; }

    // todo make DTOs
    public virtual ICollection<ItemCategoryResponse> Categories { get; } = new List<ItemCategoryResponse>();
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    public DateTime? UpdatedAt { get; set; }
    
    public DateTime? ApprovedAt { get; set; }
}