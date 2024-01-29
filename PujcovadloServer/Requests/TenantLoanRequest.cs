using System.ComponentModel.DataAnnotations;
using PujcovadloServer.Business.Enums;

namespace PujcovadloServer.Requests;

public class TenantLoanRequest : EntityRequest
{
    public LoanStatus? Status { get; set; } = null!;


    // TODO: check larger than now
    [Required] public DateTime From { get; set; } = default!;


    // todo: Check that larger than from
    [Required] public DateTime To { get; set; } = default!;

    public string? TenantNote { get; set; }

    [Required] public ItemRequest Item { get; set; } = default!;
}