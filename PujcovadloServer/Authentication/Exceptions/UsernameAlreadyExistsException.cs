namespace PujcovadloServer.Authentication.Exceptions;

public class UsernameAlreadyExistsException : Exception
{
    public UsernameAlreadyExistsException()
    {
    }

    public UsernameAlreadyExistsException(string message) : base(message)
    {
    }
}