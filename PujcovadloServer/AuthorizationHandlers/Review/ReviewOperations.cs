using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace PujcovadloServer.AuthorizationHandlers.Review;

public class ReviewOperations
{
    public static OperationAuthorizationRequirement Read =
        new OperationAuthorizationRequirement { Name = Constants.ReadOperationName };

    public static OperationAuthorizationRequirement Update =
        new OperationAuthorizationRequirement { Name = Constants.UpdateOperationName };

    public static OperationAuthorizationRequirement Delete =
        new OperationAuthorizationRequirement { Name = Constants.DeleteOperationName };

    public class Constants
    {
        public static readonly string ReadOperationName = "Read";
        public static readonly string UpdateOperationName = "Update";
        public static readonly string DeleteOperationName = "Delete";
    }
}