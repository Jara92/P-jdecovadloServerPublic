using PujcovadloServer.Business.Enums;

namespace PujcovadloServer.Responses;

public class ItemResponse
{
    public int Id { get; set; }

    public string Name { get; set; } = default!;

    public string Alias { get; set; } = default!;

    public string Description { get; set; } = default!;

    public ItemStatus Status { get; set; } = ItemStatus.Public;

    public float PricePerDay { get; set; }

    public UserResponse Owner { get; set; } = default!;

    public ImageResponse? MainImage { get; set; }

    public IList<ImageResponse> Images { get; set; } = new List<ImageResponse>();

    public IList<LinkResponse> _links { get; set; } = new List<LinkResponse>();
}