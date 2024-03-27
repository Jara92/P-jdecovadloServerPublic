using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PujcovadloServer.Authentication;
using PujcovadloServer.Business.EntityAggregations;

namespace PujcovadloServer.Business.Entities;

public class Profile : BaseEntity
{
    [StringLength(512)] public string? Description { get; set; }

    public string UserId { get; set; } = null!;
    public virtual ApplicationUser User { get; set; } = null!;

    public virtual Image? ProfileImage { get; set; }

    [NotMapped] public ProfileAggregations? Aggregations { get; set; }
}