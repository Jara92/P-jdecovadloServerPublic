namespace PujcovadloServer.Responses;

public class ResponseList<T> where T : class
{
    public IEnumerable<T> Data { get; set; } = new List<T>();
    public IList<LinkResponse> Links { get; set; } = new List<LinkResponse>();
}