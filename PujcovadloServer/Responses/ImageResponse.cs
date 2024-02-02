using PujcovadloServer.Authentication;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Enums;

namespace PujcovadloServer.Responses;

public class ImageResponse
{
    public int Id { get; set; }
    
    public IList<LinkResponse> Links { get; private set; } = new List<LinkResponse>();
}