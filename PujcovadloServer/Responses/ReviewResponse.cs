namespace PujcovadloServer.Responses;

public class ReviewResponse
{
    public int Id { get; set; }

    public string? Comment { get; set; }

    public float Rating { get; set; }

    public int LoanId { get; set; }

    public string AuthorId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public IList<LinkResponse> _links { get; private set; } = new List<LinkResponse>();
}