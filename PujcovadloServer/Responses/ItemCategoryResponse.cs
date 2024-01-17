namespace PujcovadloServer.Responses;

public class ItemCategoryResponse
{
    public int Id { get; set; }
    
    public string Name { get; set; } = default!;
    
    public string? Alias { get; set; }
    
    //public string Description { get; set; } = default!;
    
    //public virtual ItemCategoryResponse? Parent { get; set; }
}