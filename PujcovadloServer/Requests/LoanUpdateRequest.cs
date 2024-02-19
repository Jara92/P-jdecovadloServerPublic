using System.ComponentModel.DataAnnotations;
using PujcovadloServer.Business.Enums;

namespace PujcovadloServer.Requests;

public class LoanUpdateRequest : EntityRequest
{
    public LoanStatus? Status { get; set; } = null!;

    [StringLength(256, MinimumLength = 0)] public string? TenantNote { get; set; }
}