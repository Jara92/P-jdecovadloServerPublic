using System.ComponentModel.DataAnnotations;
using PujcovadloServer.Enums;

namespace PujcovadloServer.Models;

public class ItemDTO
{
    [Required]
    [StringLength(64, MinimumLength = 6)]
    public string Name { get; set; }
    public string Description { get; set; }
    public string Parameters { get; set; }
    [Required] public float PricePerDay { get; set; }
    public float? RefundableDeposit { get; set; }
    public float? PurchasePrice { get; set; }
    public float? SellingPrice { get; set; }
}