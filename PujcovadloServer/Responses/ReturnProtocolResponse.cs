namespace PujcovadloServer.Responses;

public class ReturnProtocolResponse
{
    public int Id { get; set; }

    public string? Description { get; set; }

    public DateTime? ConfirmedAt { get; set; } = null!;

    public float? ReturnedRefundableDeposit { get; set; } = null!;

    public virtual LoanResponse Loan { get; set; } = default!;

    public IList<ImageResponse> Images { get; set; } = new List<ImageResponse>();

    public DateTime? CreatedAt { get; set; } = DateTime.Now;

    public DateTime? UpdatedAt { get; set; }

    public IList<LinkResponse> _links { get; private set; } = new List<LinkResponse>();
}