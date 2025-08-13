using System.Runtime.Serialization;

namespace ReQuesty.Runtime.Tests.Abstractions.Mocks;

public enum TestEnum
{
    [EnumMember(Value = "Value_1")]
    First,
    [EnumMember(Value = "Value_2")]
    Second,
}
