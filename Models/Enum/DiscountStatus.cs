using System.Runtime.Serialization;

namespace WebApi.Models.Enum
{
    public enum DiscountStatus
    {
        [EnumMember(Value = "Active")]
        Active,
        [EnumMember(Value = "Expried")]
        Expried,
    }
}
