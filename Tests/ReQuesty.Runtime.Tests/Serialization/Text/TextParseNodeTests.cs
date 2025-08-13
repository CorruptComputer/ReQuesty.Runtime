using System.Globalization;
using ReQuesty.Runtime.Abstractions;
using ReQuesty.Runtime.Abstractions.Serialization;
using Moq;
using ReQuesty.Runtime.Serialization.Text;
using ReQuesty.Runtime.Tests.Serialization.Text.Mocks;

namespace ReQuesty.Runtime.Tests.Serialization.Text;

public class TextParseNodeTests
{
    public TextParseNodeTests()
    {
        Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("cs-CZ");
    }

    [Fact]
    public void TextParseNode_GetEnumFromInteger()
    {
        string text = "1";
        TextParseNode parseNode = new(text);

        TestEnum? result = parseNode.GetEnumValue<TestEnum>();

        Assert.Equal(TestEnum.SecondItem, result);
    }

    [Fact]
    public void TextParseNode_GetEnumFromString()
    {
        string text = "FirstItem";
        TextParseNode parseNode = new(text);

        TestEnum? result = parseNode.GetEnumValue<TestEnum>();

        Assert.Equal(TestEnum.FirstItem, result);
    }

    [Fact]
    public void TextParseNode_GetEnumFromEnumMember()
    {
        string text = "Item2:SubItem1";
        TextParseNode parseNode = new(text);

        TestNamingEnum? result = parseNode.GetEnumValue<TestNamingEnum>();

        Assert.Equal(TestNamingEnum.Item2SubItem1, result);
    }

    [Fact]
    public void TextParseNode_GetBoolValue()
    {
        string text = "true";
        TextParseNode parseNode = new(text);

        bool? result = parseNode.GetBoolValue();

        Assert.True(result);
    }

    [Fact]
    public void TextParseNode_GetByteArrayValue()
    {
        string text = "dGV4dA==";
        TextParseNode parseNode = new(text);

        byte[]? result = parseNode.GetByteArrayValue();

        Assert.Equal(new byte[] { 0x74, 0x65, 0x78, 0x74 }, result);
    }

    [Fact]
    public void TextParseNode_GetByteValue()
    {
        string text = "1";
        TextParseNode parseNode = new(text);

        byte? result = parseNode.GetByteValue();

        Assert.Equal((byte)1, result);
    }

    [Fact]
    public void TextParseNode_GetDateTimeOffsetValue()
    {
        string text = "2021-11-30T12:24:36+03:00";
        TextParseNode parseNode = new(text);

        DateTimeOffset? result = parseNode.GetDateTimeOffsetValue();

        Assert.Equal(new DateTimeOffset(2021, 11, 30, 12, 24, 36, TimeSpan.FromHours(3)), result);
    }

    [Fact]
    public void TextParseNode_GetDateValue()
    {
        string text = "2021-11-30";
        TextParseNode parseNode = new(text);

        Date? result = parseNode.GetDateValue();

        Assert.Equal(new Date(2021, 11, 30), result);
    }

    [Fact]
    public void TextParseNode_GetTimeSpanValue()
    {
        string text = "P756DT4H6M8.01S";
        TextParseNode parseNode = new(text);

        TimeSpan? result = parseNode.GetTimeSpanValue();

        Assert.Equal(new TimeSpan(756, 4, 6, 8, 10), result);
    }

    [Fact]
    public void TextParseNode_GetTimeValue()
    {
        string text = "12:24:36";
        TextParseNode parseNode = new(text);

        Time? result = parseNode.GetTimeValue();

        Assert.Equal(new Time(12, 24, 36), result);
    }

    [Fact]
    public void TextParseNode_GetGuidValue()
    {
        string text = "f4b3b8f4-6f4d-4f1f-8f4d-8f4b3b8f4d1f";
        TextParseNode parseNode = new(text);

        Guid? result = parseNode.GetGuidValue();

        Assert.Equal(new Guid("f4b3b8f4-6f4d-4f1f-8f4d-8f4b3b8f4d1f"), result);
    }

    [Fact]
    public void TextParseNode_GetIntValue()
    {
        string text = "1";
        TextParseNode parseNode = new(text);

        int? result = parseNode.GetIntValue();

        Assert.Equal(1, result);
    }

    [Fact]
    public void TextParseNode_GetLongValue()
    {
        string text = "1";
        TextParseNode parseNode = new(text);

        long? result = parseNode.GetLongValue();

        Assert.Equal(1L, result);
    }

    [Fact]
    public void TextParseNode_GetSbyteValue()
    {
        string text = "1";
        TextParseNode parseNode = new(text);

        sbyte? result = parseNode.GetSbyteValue();

        Assert.Equal((sbyte)1, result);
    }

    [Fact]
    public void TextParseNode_GetStringValue()
    {
        string text = "text";
        TextParseNode parseNode = new(text);

        string? result = parseNode.GetStringValue();

        Assert.Equal("text", result);
    }

    [Fact]
    public void TextParseNode_GetDecimalValue()
    {
        string text = "1.1";
        TextParseNode parseNode = new(text);

        decimal? result = parseNode.GetDecimalValue();

        Assert.Equal(1.1m, result);
    }

    [Fact]
    public void TextParseNode_GetDoubleValue()
    {
        string text = "1.1";
        TextParseNode parseNode = new(text);

        double? result = parseNode.GetDoubleValue();

        Assert.Equal(1.1, result);
    }

    [Fact]
    public void TextParseNode_GetFloatValue()
    {
        string text = "1.1";
        TextParseNode parseNode = new(text);

        float? result = parseNode.GetFloatValue();

        Assert.Equal(1.1f, result);
    }

    [Fact]
    public void TextParseNode_GetCollectionOfPrimitiveValues()
    {
        string text = "1,2,3";
        TextParseNode parseNode = new(text);

        Assert.Throws<InvalidOperationException>(parseNode.GetCollectionOfPrimitiveValues<int>);
    }

    [Fact]
    public void TextParseNode_GetCollectionOfObjectValues()
    {
        string text = "xxx";
        TextParseNode parseNode = new(text);

        Assert.Throws<InvalidOperationException>(() => parseNode.GetCollectionOfObjectValues(It.IsAny<ParsableFactory<TestEntity>>()));
    }

    [Fact]
    public void TextParseNode_GetCollectionOfEnumValues()
    {
        string text = "xxx";
        TextParseNode parseNode = new(text);

        Assert.Throws<InvalidOperationException>(parseNode.GetCollectionOfEnumValues<TestEnum>);
    }
}
