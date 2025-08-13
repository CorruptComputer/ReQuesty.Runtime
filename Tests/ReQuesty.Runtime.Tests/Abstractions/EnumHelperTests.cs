using ReQuesty.Runtime.Helpers;
using ReQuesty.Runtime.Tests.Abstractions.Mocks;

namespace ReQuesty.Runtime.Tests.Abstractions;

public class EnumHelperTests
{
    [Fact]
    public void EnumGenericIsParsedIfValueIsInteger()
    {
        TestEnum? result = EnumHelpers.GetEnumValue<TestEnum>("0");

        Assert.Equal(TestEnum.First, result);
    }

    [Fact]
    public void EnumWithFlagsGenericIsParsedIfValuesAreIntegers()
    {
        TestEnumWithFlags? result = EnumHelpers.GetEnumValue<TestEnumWithFlags>("1,2");

        Assert.Equal(TestEnumWithFlags.Value1 | TestEnumWithFlags.Value2, result);
    }

    [Fact]
    public void EnumGenericIsParsedIfValueIsString()
    {
        TestEnum? result = EnumHelpers.GetEnumValue<TestEnum>("First");

        Assert.Equal(TestEnum.First, result);
    }

    [Fact]
    public void EnumWithFlagsGenericIsParsedIfValuesAreStrings()
    {
        TestEnumWithFlags? result = EnumHelpers.GetEnumValue<TestEnumWithFlags>("Value1,Value3");

        Assert.Equal(TestEnumWithFlags.Value1 | TestEnumWithFlags.Value3, result);
    }

    [Fact]
    public void EnumGenericIsParsedIfValueIsFromEnumMember()
    {
        TestEnum? result = EnumHelpers.GetEnumValue<TestEnum>("Value_2");

        Assert.Equal(TestEnum.Second, result);
    }

    [Fact]
    public void EnumWithFlagsGenericIsParsedIfValuesAreFromEnumMember()
    {
        TestEnumWithFlags? result = EnumHelpers.GetEnumValue<TestEnumWithFlags>("Value__2,Value__3");

        Assert.Equal(TestEnumWithFlags.Value2 | TestEnumWithFlags.Value3, result);
    }

    [Fact]
    public void IfEnumGenericIsNotParsedThenNullIsReturned()
    {
        TestEnum? result = EnumHelpers.GetEnumValue<TestEnum>("Value_5");

        Assert.Null(result);
    }

    [Fact]
    public void EnumIsParsedIfValueIsInteger()
    {
        object? result = EnumHelpers.GetEnumValue(typeof(TestEnum), "0");

        Assert.Equal(TestEnum.First, result);
    }

    [Fact]
    public void NullableEnumIsParsedIfValueIsInteger()
    {
        object? result = EnumHelpers.GetEnumValue(typeof(TestEnum?), "0");

        Assert.Equal(TestEnum.First, result);
    }

    [Fact]
    public void EnumWithFlagsIsParsedIfValuesAreIntegers()
    {
        object? result = EnumHelpers.GetEnumValue(typeof(TestEnumWithFlags), "1,2");

        Assert.Equal(TestEnumWithFlags.Value1 | TestEnumWithFlags.Value2, result);
    }

    [Fact]
    public void NullableEnumWithFlagsIsParsedIfValuesAreIntegers()
    {
        object? result = EnumHelpers.GetEnumValue(typeof(TestEnumWithFlags?), "1,2");

        Assert.Equal(TestEnumWithFlags.Value1 | TestEnumWithFlags.Value2, result);
    }

    [Fact]
    public void EnumIsParsedIfValueIsString()
    {
        object? result = EnumHelpers.GetEnumValue(typeof(TestEnum), "First");

        Assert.Equal(TestEnum.First, result);
    }

    [Fact]
    public void NullableEnumIsParsedIfValueIsString()
    {
        object? result = EnumHelpers.GetEnumValue(typeof(TestEnum?), "First");

        Assert.Equal(TestEnum.First, result);
    }

    [Fact]
    public void EnumWithFlagsIsParsedIfValuesAreStrings()
    {
        object? result = EnumHelpers.GetEnumValue(typeof(TestEnumWithFlags), "Value1,Value3");

        Assert.Equal(TestEnumWithFlags.Value1 | TestEnumWithFlags.Value3, result);
    }

    [Fact]
    public void NullableEnumWithFlagsIsParsedIfValuesAreStrings()
    {
        object? result = EnumHelpers.GetEnumValue(typeof(TestEnumWithFlags?), "Value1,Value3");

        Assert.Equal(TestEnumWithFlags.Value1 | TestEnumWithFlags.Value3, result);
    }

    [Fact]
    public void EnumIsParsedIfValueIsFromEnumMember()
    {
        object? result = EnumHelpers.GetEnumValue(typeof(TestEnum), "Value_2");

        Assert.Equal(TestEnum.Second, result);
    }

    [Fact]
    public void NullableEnumIsParsedIfValueIsFromEnumMember()
    {
        object? result = EnumHelpers.GetEnumValue(typeof(TestEnum?), "Value_2");

        Assert.Equal(TestEnum.Second, result);
    }

    [Fact]
    public void EnumWithFlagsIsParsedIfValuesAreFromEnumMember()
    {
        object? result = EnumHelpers.GetEnumValue(typeof(TestEnumWithFlags), "Value__2,Value__3");

        Assert.Equal(TestEnumWithFlags.Value2 | TestEnumWithFlags.Value3, result);
    }

    [Fact]
    public void NullableEnumWithFlagsIsParsedIfValuesAreFromEnumMember()
    {
        object? result = EnumHelpers.GetEnumValue(typeof(TestEnumWithFlags?), "Value__2,Value__3");

        Assert.Equal(TestEnumWithFlags.Value2 | TestEnumWithFlags.Value3, result);
    }

    [Fact]
    public void IfEnumIsNotParsedThenNullIsReturned()
    {
        object? result = EnumHelpers.GetEnumValue(typeof(TestEnum), "Value_5");

        Assert.Null(result);
    }

    [Fact]
    public void IfNullableEnumIsNotParsedThenNullIsReturned()
    {
        object? result = EnumHelpers.GetEnumValue(typeof(TestEnum?), "Value_5");

        Assert.Null(result);
    }
}
