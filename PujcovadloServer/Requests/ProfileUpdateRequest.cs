using System.ComponentModel.DataAnnotations;

namespace PujcovadloServer.Requests;

public class ProfileUpdateRequest : EntityRequest
{
    [StringLength(512)] public string? Description { get; set; }
}