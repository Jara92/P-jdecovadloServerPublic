using System.ComponentModel.DataAnnotations;
using PujcovadloServer.Areas.Api.Attributes;
using PujcovadloServer.Business.Enums;

namespace PujcovadloServer.Areas.Admin.Requests;

public class LoanRequest
{
    public int? Id { get; set; }

    public LoanStatus Status { get; set; }

    [Required] [DataType(DataType.Date)] public DateTime From { get; set; }

    [Required] [DataType(DataType.Date)] public DateTime To { get; set; }

    [Required] public int Days { get; set; }

    [StringLength(256, MinimumLength = 0)] public string? TenantNote { get; set; }

    [Required] [Price(0, 9999900)] public float PricePerDay { get; set; }

    [Price(0, 9999900)] public float? RefundableDeposit { get; set; }

    [Required] [Price(0, 9999900)] public float ExpectedPrice { get; set; }

    [Required] public int ItemId { get; set; }

    [Required] public string TenantId { get; set; } = default!;
}