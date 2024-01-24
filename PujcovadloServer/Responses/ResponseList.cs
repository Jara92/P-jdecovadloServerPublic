namespace PujcovadloServer.Responses;

public class ResponseList<T> where T : class
{
    public IEnumerable<T> Data { get; set; } = new List<T>();
    public IEnumerable<LinkResponse> Links { get; set; } = new List<LinkResponse>();
}