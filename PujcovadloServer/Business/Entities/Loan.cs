using PujcovadloServer.Authentication;
using PujcovadloServer.Business.Enums;

namespace PujcovadloServer.Business.Entities;

public class Loan : BaseEntity
{
    public LoanStatus Status { get; set; } = LoanStatus.Inquired;
    
    public DateTime From { get; set; }
    
    public DateTime To { get; set; }
    
    public int Days { get; set; }
    
    public float PricePerUnit { get; set; }
    
    public float ExpectedPrice { get; set; }
    
    public float? RefundableDeposit { get; set; }
    
    public string? TenantNote { get; set; }
    
    public virtual ApplicationUser Tenant { get; set; } = default!;
    
    public virtual Item Item { get; set; } = default!;
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    public DateTime? UpdatedAt { get; set; }
}