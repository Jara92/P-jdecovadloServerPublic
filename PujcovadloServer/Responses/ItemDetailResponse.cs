using System.ComponentModel.DataAnnotations;
using PujcovadloServer.Business.Entities;

namespace PujcovadloServer.Responses;

public class ItemDetailResponse : ItemResponse
{
    public string Parameters { get; set; } = "";
    
    public float? RefundableDeposit { get; set; }
    
    public float? SellingPrice { get; set; }
    
    public virtual ICollection<ItemCategoryResponse> Categories { get; } = new List<ItemCategoryResponse>();
    
    public virtual ICollection<ItemTagResponse> Tags { get; } = new List<ItemTagResponse>();
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    public DateTime? UpdatedAt { get; set; }
    
    public DateTime? ApprovedAt { get; set; }
}