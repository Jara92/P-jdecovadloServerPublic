using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace PujcovadloServer.AuthorizationHandlers.Image;

public class ImageOperations
{
    public static OperationAuthorizationRequirement Read =
        new OperationAuthorizationRequirement { Name = Constants.ReadOperationName };

    public static OperationAuthorizationRequirement Delete =
        new OperationAuthorizationRequirement { Name = Constants.DeleteOperationName };

    public class Constants
    {
        public static readonly string ReadOperationName = "Read";
        public static readonly string DeleteOperationName = "Delete";
    }
}