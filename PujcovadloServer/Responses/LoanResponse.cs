using PujcovadloServer.Authentication;
using PujcovadloServer.Business.Entities;
using PujcovadloServer.Business.Enums;

namespace PujcovadloServer.Responses;

public class LoanResponse
{
    public int Id { get; set; }
    
    public LoanStatus Status { get; set; } = LoanStatus.Inquired;
    
    public DateTime From { get; set; }
    
    public DateTime To { get; set; }
    
    public int Days { get; set; }
    
    public float PricePerUnit { get; set; }
    
    public float ExpectedPrice { get; set; }
    
    public float? RefundableDeposit { get; set; }
    
    public UserResponse Tenant { get; set; } = default!;
    
    public ItemResponse Item { get; set; } = default!;
    
    public IList<LinkResponse> Links { get; private set; } = new List<LinkResponse>();
}