namespace PujcovadloServer.Areas.Admin.Responses;

public class ReviewResponse
{
    public int? Id { get; set; }

    public string? Comment { get; set; }

    public float Rating { get; set; }

    public int LoanId { get; set; }

    public string AuthorId { get; set; }

    public UserResponse Author { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }
}