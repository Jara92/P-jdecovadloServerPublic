namespace PujcovadloServer.Responses;

public class ProfileResponse
{
    public int Id { get; set; }

    public string? Description { get; set; }

    public ImageResponse? ProfileImage { get; set; }

    public IList<LinkResponse> _links { get; private set; } = new List<LinkResponse>();

    public ProfileAggregationsResponse? _aggregations { get; set; }
}