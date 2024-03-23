namespace PujcovadloServer.Responses;

public class ImageResponse
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string Path { get; set; } = default!;

    public string Url { get; set; } = default!;

    public IList<LinkResponse> _links { get; private set; } = new List<LinkResponse>();
}