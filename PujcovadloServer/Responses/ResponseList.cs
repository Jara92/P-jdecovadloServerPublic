namespace PujcovadloServer.Responses;

public class ResponseList<T> where T : class
{
    public IEnumerable<T> _data { get; set; } = new List<T>();
    public IList<LinkResponse> _links { get; set; } = new List<LinkResponse>();
}