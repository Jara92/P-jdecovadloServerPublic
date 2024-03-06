namespace PujcovadloServer.Business.Enums;

public static class UserRoles
{
    public const string Admin = "Admin";
    public const string User = "User";
    public const string Tenant = "Tenant";
    public const string Owner = "Owner";

    public static string[] AllRoles = { Admin, User, Tenant, Owner };
}