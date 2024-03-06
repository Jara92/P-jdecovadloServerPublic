using System.ComponentModel.DataAnnotations;
using PujcovadloServer.Areas.Api.Attributes;

namespace PujcovadloServer.Areas.Admin.Requests;

public class PickupProtocolRequest
{
    public int? Id { get; set; }

    [Required] [StringLength(1024)] public string? Description { get; set; }

    [Price(0, 10000000)] public float? AcceptedRefundableDeposit { get; set; }

    [DataType(DataType.DateTime)] public DateTime? ConfirmedAt { get; set; }

    [Required]
    [DataType(DataType.DateTime)]
    public DateTime CreatedAt { get; set; }

    [DataType(DataType.DateTime)] public DateTime? UpdatedAt { get; set; }
}