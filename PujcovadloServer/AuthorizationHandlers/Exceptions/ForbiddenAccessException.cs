namespace PujcovadloServer.AuthorizationHandlers.Exceptions;

public class ForbiddenAccessException : Exception
{
    public ForbiddenAccessException()
    {
    }
    
    public ForbiddenAccessException(string message) : base(message)
    {
    }
}