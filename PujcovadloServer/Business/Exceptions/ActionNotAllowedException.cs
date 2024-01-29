namespace PujcovadloServer.Business.Exceptions;

public class ActionNotAllowedException : Exception
{
    public ActionNotAllowedException()
    {
    }
    
    public ActionNotAllowedException(string message) : base(message)
    {
    }
}