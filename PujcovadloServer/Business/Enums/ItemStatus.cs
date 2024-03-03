using System.Runtime.Serialization;

namespace PujcovadloServer.Business.Enums;

public enum ItemStatus
{
    [EnumMember(Value = "Public")] Public = 1,
    [EnumMember(Value = "Denied")] Denied = 2,
    [EnumMember(Value = "Approving")] Approving = 3,
    [EnumMember(Value = "Deleted")] Deleted = 4
}