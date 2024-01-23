namespace PujcovadloServer.Responses;

public class LinkResponse
{
    public string? Href { get; set; } = default!;
    public string Rel { get; set; } = default!;
    public string Method { get; set; } = default!;

    public LinkResponse(string? href, string rel, string method)
    {
        Href = href;
        Rel = rel;
        Method = method;
    }
}