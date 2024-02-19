using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace PujcovadloServer.AuthorizationHandlers.Item;

public class LoanOperations
{
    public static OperationAuthorizationRequirement Create =
        new OperationAuthorizationRequirement { Name = Constants.CreateOperationName };

    public static OperationAuthorizationRequirement Read =
        new OperationAuthorizationRequirement { Name = Constants.ReadOperationName };

    public static OperationAuthorizationRequirement Update =
        new OperationAuthorizationRequirement { Name = Constants.UpdateOperationName };

    public static OperationAuthorizationRequirement Delete =
        new OperationAuthorizationRequirement { Name = Constants.DeleteOperationName };

    public static OperationAuthorizationRequirement CreatePickupProtocol =
        new OperationAuthorizationRequirement { Name = Constants.CreatePickupProtocolOperationName };

    public static OperationAuthorizationRequirement CreateReturnProtocol =
        new OperationAuthorizationRequirement { Name = Constants.CreateReturnProtocolOperationName };

    public class Constants
    {
        public static readonly string CreateOperationName = "Create";
        public static readonly string ReadOperationName = "Read";
        public static readonly string UpdateOperationName = "Update";
        public static readonly string DeleteOperationName = "Delete";
        public static readonly string CreatePickupProtocolOperationName = "CreatePickupProtocol";
        public static readonly string CreateReturnProtocolOperationName = "CreateReturnProtocol";
    }
}