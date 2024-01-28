using System.ComponentModel.DataAnnotations;

namespace PujcovadloServer.Responses;

public class ItemOwnerResponse : ItemDetailResponse
{
    public float? PurchasePrice { get; set; }
}