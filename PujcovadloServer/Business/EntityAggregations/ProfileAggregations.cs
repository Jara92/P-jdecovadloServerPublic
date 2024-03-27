namespace PujcovadloServer.Business.EntityAggregations;

public class ProfileAggregations
{
    public int CountOfPublicItems { get; set; }

    public int CountOfBorrowedItems { get; set; }

    public int CountOfLentItems { get; set; }

    public float? AverageRating { get; set; }

    public int TotalReviews { get; set; }
}