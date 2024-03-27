namespace PujcovadloServer.Responses;

public class ProfileAggregationsResponse
{
    public int CountOfPublicItems { get; set; }

    public int CountOfBorrowedItems { get; set; }

    public int CountOfLentItems { get; set; }

    public float? AverageRating { get; set; }

    public int TotalReviews { get; set; }
}