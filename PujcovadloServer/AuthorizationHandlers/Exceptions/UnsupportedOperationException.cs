namespace PujcovadloServer.AuthorizationHandlers.Exceptions;

public class UnsupportedOperationException: Exception
{
    public UnsupportedOperationException()
    {
    }
    
    public UnsupportedOperationException(string message) : base(message)
    {
    }
}