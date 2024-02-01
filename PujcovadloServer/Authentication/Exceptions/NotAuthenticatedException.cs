namespace PujcovadloServer.Authentication.Exceptions;

public class NotAuthenticatedException : Exception
{
    public NotAuthenticatedException() : base()
    {
    }

    public NotAuthenticatedException(string message) : base(message)
    {
    }
}