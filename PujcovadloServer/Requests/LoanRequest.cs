using System.ComponentModel.DataAnnotations;
using PujcovadloServer.Api.Attributes;
using PujcovadloServer.Business.Enums;

namespace PujcovadloServer.Requests;

public class LoanRequest : EntityRequest
{
    public LoanStatus? Status { get; set; } = null!;

    [Required] [DataType(DataType.Date)] public DateTime? From { get; set; } = default!;

    [Required]
    [DataType(DataType.Date)]
    [LoanDateTimeRange]
    public DateTime? To { get; set; } = default!;

    [StringLength(256, MinimumLength = 0)] public string? TenantNote { get; set; }

    [Required] public int ItemId { get; set; }
}