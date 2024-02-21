using System.ComponentModel.DataAnnotations;

namespace PujcovadloServer.Requests;

public class ReviewRequest
{
    [StringLength(256)] public string? Comment { get; set; }

    [Required] [Range(0, 5)] public float Rating { get; set; }
}