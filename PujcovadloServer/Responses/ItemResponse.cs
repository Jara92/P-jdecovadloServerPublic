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
    
    public float PricePerDay { get; set; }
    
    //public string? Href { get; set; }
    public IList<LinkResponse> Links { get; private set; } = new List<LinkResponse>();
 
}