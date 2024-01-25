namespace PujcovadloServer.Authentication.Exceptions;

public class RegistrationFailedException : Exception
{
    public IList<string> Messages { get; protected set; } = new List<string>();
    
    public RegistrationFailedException()
    {
    }
    
    public RegistrationFailedException(string message) : base(message)
    {
        Messages.Add(message);
    }
    
    public RegistrationFailedException(IList<string> messages)
    {
        Messages = messages;
    }
}