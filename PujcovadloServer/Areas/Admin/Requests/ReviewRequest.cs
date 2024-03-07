using System.ComponentModel.DataAnnotations;

namespace PujcovadloServer.Areas.Admin.Requests;

public class ReviewRequest
{
    public int? Id { get; set; }

    [StringLength(256)] public string? Comment { get; set; }

    [Required] [Range(0, 5)] public float Rating { get; set; }

    [Required] public int LoanId { get; set; }

    [Required] public string AuthorId { get; set; }

    [Required] public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }
}