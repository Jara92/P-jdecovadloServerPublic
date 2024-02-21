using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PujcovadloServer.Authentication;

namespace PujcovadloServer.Business.Entities;

public class Review : BaseEntity
{
    [Column(TypeName = "VARCHAR")]
    [StringLength(256)]
    public string? Comment { get; set; }

    [Required] [Range(0, 5)] public float Rating { get; set; }

    // TODO: Add item rating?

    public virtual Loan Loan { get; set; } = default!;

    public virtual ApplicationUser Author { get; set; } = default!;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }
}