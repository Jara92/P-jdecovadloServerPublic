namespace PujcovadloServer.Responses;

public class ResponseList
{
    public IEnumerable<ItemResponse> Data { get; set; } = new List<ItemResponse>();
    public IEnumerable<LinkResponse> Links { get; set; } = new List<LinkResponse>();
}