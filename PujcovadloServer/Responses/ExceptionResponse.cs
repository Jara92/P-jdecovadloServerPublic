namespace PujcovadloServer.Responses;

public class ExceptionResponse
{
    public string? Title { get; set; }

    public int? Status { get; set; }

    public List<string> Errors { get; set; } = new();
}