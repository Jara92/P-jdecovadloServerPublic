namespace PujcovadloServer.Responses;

public class ItemDetailResponse : ItemResponse
{
    public string Description { get; set; } = default!;

    public string Parameters { get; set; } = "";

    public virtual IList<ItemCategoryResponse> Categories { get; set; } = new List<ItemCategoryResponse>();

    public virtual IList<ItemTagResponse> Tags { get; set; } = new List<ItemTagResponse>();

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime? UpdatedAt { get; set; }

    public DateTime? ApprovedAt { get; set; }
}