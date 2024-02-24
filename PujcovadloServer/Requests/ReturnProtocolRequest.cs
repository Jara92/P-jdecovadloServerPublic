using System.ComponentModel.DataAnnotations;
using PujcovadloServer.Areas.Api.Attributes;

namespace PujcovadloServer.Requests;

public class ReturnProtocolRequest
{
    [Required]
    [StringLength(512, MinimumLength = 4)]
    public string? Description { get; set; }

    [Price(0, 10000000)] public float? ReturnedRefundableDeposit { get; set; } = null!;

    // TODO: add common LoanRequest properties
}