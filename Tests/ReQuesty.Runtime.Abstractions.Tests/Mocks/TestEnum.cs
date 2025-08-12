using System.Runtime.Serialization;

namespace ReQuesty.Runtime.Abstractions.Tests.Mocks;

public enum TestEnum
{
    [EnumMember(Value = "Value_1")]
    First,
    [EnumMember(Value = "Value_2")]
    Second,
}
