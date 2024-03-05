namespace PujcovadloServer.Areas.Admin.Responses;

public class ItemCategoryResponse
{
    public int Id { get; set; }

    public string Name { get; set; } = default!;

    public string? Alias { get; set; }

    public int? ParentId { get; set; }

    public string ParentName { get; set; } = default!;
}