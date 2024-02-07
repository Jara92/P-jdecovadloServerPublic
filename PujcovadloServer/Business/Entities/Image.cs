using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PujcovadloServer.Authentication;

namespace PujcovadloServer.Business.Entities;

public class Image : BaseEntity
{
    [Required]
    [Column(TypeName = "VARCHAR")]
    [StringLength(64, MinimumLength = 1)]
    [RegularExpression(@"^[A-Z]+[a-zA-Z]+[0-9]*$")]
    public string Name { get; set; } = default!;

    [Column(TypeName = "VARCHAR")]
    [StringLength(256, MinimumLength = 1)]
    public string Path { get; set; } = default!;
    
    public string Extension { get; set; } = default!;
    
    public string MimeType { get; set; } = default!;

    public virtual ApplicationUser Owner { get; set; } = default!;

    public virtual Item? Item { get; set; }

    [ReadOnly(true)] public DateTime CreatedAt { get; set; } = DateTime.Now;
}