using System.ComponentModel.DataAnnotations;
using PujcovadloServer.Areas.Api.Attributes;

namespace PujcovadloServer.Areas.Admin.Requests;

public class ReturnProtocolRequest
{
    public int? Id { get; set; }

    [Required] [StringLength(1024)] public string? Description { get; set; }

    [Price(0, 10000000)] public float? ReturnedRefundableDeposit { get; set; }

    [DataType(DataType.DateTime)] public DateTime? ConfirmedAt { get; set; }

    [Required]
    [DataType(DataType.DateTime)]
    public DateTime CreatedAt { get; set; }

    [DataType(DataType.DateTime)] public DateTime? UpdatedAt { get; set; }
}