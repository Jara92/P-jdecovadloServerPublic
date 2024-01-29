using System.ComponentModel.DataAnnotations;
using PujcovadloServer.Api.Attributes;
using PujcovadloServer.Business.Enums;

namespace PujcovadloServer.Requests;

public class TenantLoanRequest : EntityRequest
{
    public LoanStatus? Status { get; set; } = null!;
    
    [Required]
    [DataType(DataType.Date)] 
    public DateTime? From { get; set; } = default!;
    
    [Required]
    [DataType(DataType.Date)] 
    [LoanDateTimeRange]
    public DateTime? To { get; set; } = default!;

    public string? TenantNote { get; set; }

    public ItemRequest Item { get; set; } = default!;
}