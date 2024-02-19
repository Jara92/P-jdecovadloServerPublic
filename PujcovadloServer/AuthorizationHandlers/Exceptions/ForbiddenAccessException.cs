namespace PujcovadloServer.AuthorizationHandlers.Exceptions;

public class ForbiddenAccessException : Exception
{
    public IList<string?> Reasons { get; private set; } = new List<string?>();

    public ForbiddenAccessException()
    {
    }

    public ForbiddenAccessException(string message) : base(message)
    {
    }

    public ForbiddenAccessException(string message, IList<string?> reasons) : base(message)
    {
        Reasons = reasons;
    }
}