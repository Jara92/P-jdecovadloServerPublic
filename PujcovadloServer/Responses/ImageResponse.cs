using PujcovadloServer.Authentication;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Enums;

namespace PujcovadloServer.Responses;

public class ImageResponse
{
    public int Id { get; set; }
    
    public string? Name { get; set; }
    
    public string Path { get; set; } = default!;
    
    public IList<LinkResponse> Links { get; private set; } = new List<LinkResponse>();
}