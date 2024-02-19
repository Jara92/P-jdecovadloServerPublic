using System.ComponentModel.DataAnnotations;
using PujcovadloServer.Api.Attributes;

namespace PujcovadloServer.Requests;

public class PickupProtocolRequest
{
    [Required]
    [StringLength(512, MinimumLength = 4)]
    public string? Description { get; set; }

    [Price(0, 10000000)] public float? AcceptedRefundableDeposit { get; set; } = null!;

    // TODO: add common LoanRequest properties
}