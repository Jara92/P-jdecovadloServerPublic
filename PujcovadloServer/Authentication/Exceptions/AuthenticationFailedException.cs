namespace PujcovadloServer.Authentication.Exceptions;

public class AuthenticationFailedException : Exception
{
    public AuthenticationFailedException() : base()
    {
    }
    
    public AuthenticationFailedException(string message) : base(message)
    {
    }
}