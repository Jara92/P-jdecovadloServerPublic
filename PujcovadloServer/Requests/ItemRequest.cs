using System.ComponentModel.DataAnnotations;

namespace PujcovadloServer.Requests;

public class ItemRequest: EntityRequest
{
    
    public string Name { get; set; } = default!;
    
    public string Description { get; set; } = default!;
    
    public string Parameters { get; set; } = "";
    
    public float PricePerDay { get; set; }
    
    public float? RefundableDeposit { get; set; }
    
    public float? SellingPrice { get; set; }
    
    public float? PurchasePrice { get; set; }
    
    public virtual ICollection<ItemCategoryRequest> Categories { get; set; } = new List<ItemCategoryRequest>();
}