using System.Globalization;
using System.Text;
using ReQuesty.Runtime.Abstractions;
using ReQuesty.Runtime.Serialization.Form.Tests.Mocks;

namespace ReQuesty.Runtime.Serialization.Form.Tests;
public class FormSerializationWriterTests
{
    public FormSerializationWriterTests()
    {
        Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("cs-CZ");
    }

    [Fact]
    public void WritesSampleObjectValue()
    {
        // Arrange
        TestEntity testEntity = new()
        {
            Id = "48d31887-5fad-4d73-a9f5-3c356e68a038",
            WorkDuration = TimeSpan.FromHours(1),
            StartWorkTime = new Time(8, 0, 0),
            BirthDay = new Date(2017, 9, 4),
            Numbers = TestEnum.One | TestEnum.Two,
            DeviceNames =
            [
                "device1", "device2"
            ],
            AdditionalData = new Dictionary<string, object>
            {
                {"mobilePhone", null!}, // write null value
                {"accountEnabled", false}, // write bool value
                {"jobTitle","Author"}, // write string value
                {"otherPhones", new List<string>{ "123456789", "987654321"} },
                {"createdDateTime", DateTimeOffset.MinValue}, // write date value
                { "decimalValue", 2m},
                { "floatValue", 1.2f},
                { "longValue", 2L},
                { "doubleValue", 2d},
                { "guidValue", Guid.Parse("48d31887-5fad-4d73-a9f5-3c356e68a038")},
                { "intValue", 1}
            }
        };
        using FormSerializationWriter formSerializerWriter = new();
        // Act
        formSerializerWriter.WriteObjectValue(string.Empty, testEntity);
        // Get the string from the stream.
        Stream serializedStream = formSerializerWriter.GetSerializedContent();
        using StreamReader reader = new(serializedStream, Encoding.UTF8);
        string serializedFormString = reader.ReadToEnd();

        // Assert
        string expectedString = "id=48d31887-5fad-4d73-a9f5-3c356e68a038&" +
                                "numbers=One%2CTwo&" +   // serializes enums
                                "workDuration=PT1H&" +    // Serializes timespans
                                "birthDay=2017-09-04&" + // Serializes dates
                                "startWorkTime=08%3A00%3A00&" + //Serializes times
                                "deviceNames=device1&deviceNames=device2&" + // Serializes collection of scalars using the same key
                                "mobilePhone=null&" + // Serializes null values
                                "accountEnabled=false&" +
                                "jobTitle=Author&" +
                                "otherPhones=123456789&otherPhones=987654321&" + // Serializes collection of scalars using the same key which we present in the AdditionalData
                                "createdDateTime=0001-01-01T00%3A00%3A00.0000000%2B00%3A00&" +
                                "decimalValue=2&" +
                                "floatValue=1.2&" +
                                "longValue=2&" +
                                "doubleValue=2&" +
                                "guidValue=48d31887-5fad-4d73-a9f5-3c356e68a038&" +
                                "intValue=1";
        Assert.Equal(expectedString, serializedFormString);
    }

    [Fact]
    public void DoesNotWritesSampleCollectionOfObjectValues()
    {
        // Arrange
        TestEntity testEntity = new()
        {
            Id = "48d31887-5fad-4d73-a9f5-3c356e68a038",
            Numbers = TestEnum.One | TestEnum.Two,
            AdditionalData = new Dictionary<string, object>
            {
                {"mobilePhone",null!}, // write null value
                {"accountEnabled",false}, // write bool value
                {"jobTitle","Author"}, // write string value
                {"createdDateTime", DateTimeOffset.MinValue}, // write date value
            }
        };
        List<TestEntity> entityList = [testEntity];
        using FormSerializationWriter formSerializerWriter = new();
        // Act
        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => formSerializerWriter.WriteCollectionOfObjectValues(string.Empty, entityList));
        Assert.Equal("Form serialization does not support collections.", exception.Message);
    }

    [Fact]
    public void DoesNotWriteNestedObjectValuesInAdditionalData()
    {
        // Arrange
        TestEntity testEntity = new()
        {
            Id = "48d31887-5fad-4d73-a9f5-3c356e68a038",
            Numbers = TestEnum.One | TestEnum.Two,
            AdditionalData = new Dictionary<string, object>
            {
                {"nestedEntity", new TestEntity()
                {
                    Id = new Guid().ToString(),
                }} // write nested entity
            }
        };
        using FormSerializationWriter formSerializerWriter = new();
        // Act
        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => formSerializerWriter.WriteObjectValue(string.Empty, testEntity));
        Assert.Equal("Form serialization does not support nested objects.", exception.Message);
    }

    [Fact]
    public void WriteBoolValue_IsWrittenCorrectly()
    {
        // Arrange
        bool value = true;

        using FormSerializationWriter formSerializationWriter = new();

        // Act
        formSerializationWriter.WriteBoolValue("prop1", value);
        Stream contentStream = formSerializationWriter.GetSerializedContent();
        using StreamReader reader = new(contentStream, Encoding.UTF8);
        string serializedString = reader.ReadToEnd();

        // Assert
        Assert.Equal("prop1=true", serializedString);
    }

    [Fact]
    public void WriteByteArrayValue_IsWrittenCorrectly()
    {
        // Arrange
        byte[] value = new byte[] { 2, 4, 6 };

        using FormSerializationWriter formSerializationWriter = new();

        // Act
        formSerializationWriter.WriteByteArrayValue("prop1", value);
        Stream contentStream = formSerializationWriter.GetSerializedContent();
        using StreamReader reader = new(contentStream, Encoding.UTF8);
        string serializedString = reader.ReadToEnd();

        // Assert
        Assert.Equal("prop1=AgQG", serializedString);
    }

    [Fact]
    public void WriteByteValue_IsWrittenCorrectly()
    {
        // Arrange
        byte value = 5;

        using FormSerializationWriter formSerializationWriter = new();

        // Act
        formSerializationWriter.WriteByteValue("prop1", value);
        Stream contentStream = formSerializationWriter.GetSerializedContent();
        using StreamReader reader = new(contentStream, Encoding.UTF8);
        string serializedString = reader.ReadToEnd();

        // Assert
        Assert.Equal("prop1=5", serializedString);
    }

    [Fact]
    public void WriteDateTimeOffsetValue_IsWrittenCorrectly()
    {
        // Arrange
        DateTimeOffset value = new(2024, 11, 30, 15, 35, 45, 987, TimeSpan.FromHours(3));

        using FormSerializationWriter formSerializationWriter = new();

        // Act
        formSerializationWriter.WriteDateTimeOffsetValue("prop1", value);
        Stream contentStream = formSerializationWriter.GetSerializedContent();
        using StreamReader reader = new(contentStream, Encoding.UTF8);
        string serializedString = reader.ReadToEnd();

        // Assert
        Assert.Equal("prop1=2024-11-30T15%3A35%3A45.9870000%2B03%3A00", serializedString);
    }

    [Fact]
    public void WriteDateValue_IsWrittenCorrectly()
    {
        // Arrange
        Date value = new(2024, 11, 30);

        using FormSerializationWriter formSerializationWriter = new();

        // Act
        formSerializationWriter.WriteDateValue("prop1", value);
        Stream contentStream = formSerializationWriter.GetSerializedContent();
        using StreamReader reader = new(contentStream, Encoding.UTF8);
        string serializedString = reader.ReadToEnd();

        // Assert
        Assert.Equal("prop1=2024-11-30", serializedString);
    }

    [Fact]
    public void WriteDecimalValue_IsWrittenCorrectly()
    {
        // Arrange
        decimal value = 36.8m;

        using FormSerializationWriter formSerializationWriter = new();

        // Act
        formSerializationWriter.WriteDecimalValue("prop1", value);
        Stream contentStream = formSerializationWriter.GetSerializedContent();
        using StreamReader reader = new(contentStream, Encoding.UTF8);
        string serializedString = reader.ReadToEnd();

        // Assert
        Assert.Equal("prop1=36.8", serializedString);
    }

    [Fact]
    public void WriteDoubleValue_IsWrittenCorrectly()
    {
        // Arrange
        double value = 36.8d;

        using FormSerializationWriter formSerializationWriter = new();

        // Act
        formSerializationWriter.WriteDoubleValue("prop1", value);
        Stream contentStream = formSerializationWriter.GetSerializedContent();
        using StreamReader reader = new(contentStream, Encoding.UTF8);
        string serializedString = reader.ReadToEnd();

        // Assert
        Assert.Equal("prop1=36.8", serializedString);
    }

    [Fact]
    public void WriteFloatValue_IsWrittenCorrectly()
    {
        // Arrange
        float value = 36.8f;

        using FormSerializationWriter formSerializationWriter = new();

        // Act
        formSerializationWriter.WriteFloatValue("prop1", value);
        Stream contentStream = formSerializationWriter.GetSerializedContent();
        using StreamReader reader = new(contentStream, Encoding.UTF8);
        string serializedString = reader.ReadToEnd();

        // Assert
        Assert.Equal("prop1=36.8", serializedString);
    }

    [Fact]
    public void WriteGuidValue_IsWrittenCorrectly()
    {
        // Arrange
        Guid value = new("3adeb301-58f1-45c5-b820-ae5f4af13c89");

        using FormSerializationWriter formSerializationWriter = new();

        // Act
        formSerializationWriter.WriteGuidValue("prop1", value);
        Stream contentStream = formSerializationWriter.GetSerializedContent();
        using StreamReader reader = new(contentStream, Encoding.UTF8);
        string serializedString = reader.ReadToEnd();

        // Assert
        Assert.Equal("prop1=3adeb301-58f1-45c5-b820-ae5f4af13c89", serializedString);
    }

    [Fact]
    public void WriteIntegerValue_IsWrittenCorrectly()
    {
        // Arrange
        int value = 25;

        using FormSerializationWriter formSerializationWriter = new();

        // Act
        formSerializationWriter.WriteIntValue("prop1", value);
        Stream contentStream = formSerializationWriter.GetSerializedContent();
        using StreamReader reader = new(contentStream, Encoding.UTF8);
        string serializedString = reader.ReadToEnd();

        // Assert
        Assert.Equal("prop1=25", serializedString);
    }

    [Fact]
    public void WriteLongValue_IsWrittenCorrectly()
    {
        // Arrange
        long value = long.MaxValue;

        using FormSerializationWriter formSerializationWriter = new();

        // Act
        formSerializationWriter.WriteLongValue("prop1", value);
        Stream contentStream = formSerializationWriter.GetSerializedContent();
        using StreamReader reader = new(contentStream, Encoding.UTF8);
        string serializedString = reader.ReadToEnd();

        // Assert
        Assert.Equal("prop1=9223372036854775807", serializedString);
    }

    [Fact]
    public void WriteNullValue_IsWrittenCorrectly()
    {
        // Arrange
        using FormSerializationWriter formSerializationWriter = new();

        // Act
        formSerializationWriter.WriteNullValue("prop1");
        Stream contentStream = formSerializationWriter.GetSerializedContent();
        using StreamReader reader = new(contentStream, Encoding.UTF8);
        string serializedString = reader.ReadToEnd();

        // Assert
        Assert.Equal("prop1=null", serializedString);
    }

    [Fact]
    public void WriteSByteValue_IsWrittenCorrectly()
    {
        // Arrange
        sbyte value = sbyte.MaxValue;

        using FormSerializationWriter formSerializationWriter = new();

        // Act
        formSerializationWriter.WriteSbyteValue("prop1", value);
        Stream contentStream = formSerializationWriter.GetSerializedContent();
        using StreamReader reader = new(contentStream, Encoding.UTF8);
        string serializedString = reader.ReadToEnd();

        // Assert
        Assert.Equal("prop1=127", serializedString);
    }

    [Fact]
    public void WriteTimeValue_IsWrittenCorrectly()
    {
        // Arrange
        Time value = new(23, 46, 59);

        using FormSerializationWriter formSerializationWriter = new();

        // Act
        formSerializationWriter.WriteTimeValue("prop1", value);
        Stream contentStream = formSerializationWriter.GetSerializedContent();
        using StreamReader reader = new(contentStream, Encoding.UTF8);
        string serializedString = reader.ReadToEnd();

        // Assert
        Assert.Equal("prop1=23%3A46%3A59", serializedString);
    }

    [Fact]
    public void WriteTimeSpanValue_IsWrittenCorrectly()
    {
        // Arrange
        TimeSpan value = new(756, 4, 6, 8, 10);

        using FormSerializationWriter formSerializationWriter = new();

        // Act
        formSerializationWriter.WriteTimeSpanValue("prop1", value);
        Stream contentStream = formSerializationWriter.GetSerializedContent();
        using StreamReader reader = new(contentStream, Encoding.UTF8);
        string serializedString = reader.ReadToEnd();

        // Assert
        Assert.Equal("prop1=P756DT4H6M8.01S", serializedString);
    }

    [Fact]
    public void WriteAdditionalData_AreWrittenCorrectly()
    {
        // Arrange
        Dictionary<string, object> additionalData = new()
        {
            { "prop1", "value1" },
            { "prop2", 2 },
            { "prop3", true },
            { "prop4", 2.25d },
            { "prop5", 3.14f },
            { "prop6", 4L },
            { "prop7", 5m },
            { "prop8", new DateTimeOffset(2024, 11, 30, 15, 35, 45, 987, TimeSpan.FromHours(3)) },
            { "prop9", new Date(2024, 11, 30) },
            { "prop10", new Time(23, 46, 59) },
            { "prop11", new TimeSpan(756, 4, 6, 8, 10) },
            { "prop12", new byte[] { 2, 4, 6 } },
            { "prop13", new Guid("3adeb301-58f1-45c5-b820-ae5f4af13c89") },
            { "prop14", sbyte.MaxValue }
        };

        using FormSerializationWriter formSerializationWriter = new();

        // Act and Assert
        formSerializationWriter.WriteAdditionalData(additionalData);
        Stream contentStream = formSerializationWriter.GetSerializedContent();
        using StreamReader reader = new(contentStream, Encoding.UTF8);
        string serializedString = reader.ReadToEnd();

        // Assert
        Assert.Equal("prop1=value1&prop2=2&prop3=true&prop4=2.25&prop5=3.14&prop6=4&prop7=5&prop8=2024-11-30T15%3A35%3A45.9870000%2B03%3A00&prop9=2024-11-30&prop10=23%3A46%3A59&prop11=P756DT4H6M8.01S&prop12=AgQG&prop13=3adeb301-58f1-45c5-b820-ae5f4af13c89&prop14=127", serializedString);
    }

    [Fact]
    public void WriteEnumValue_IsWrittenCorrectly()
    {
        // Arrange
        TestEnum value = TestEnum.Sixteen;

        using FormSerializationWriter formSerializationWriter = new();

        // Act
        formSerializationWriter.WriteEnumValue<TestEnum>("prop1", value);
        Stream contentStream = formSerializationWriter.GetSerializedContent();
        using StreamReader reader = new(contentStream, Encoding.UTF8);
        string serializedString = reader.ReadToEnd();

        // Assert
        Assert.Equal("prop1=Sixteen", serializedString);
    }

    [Fact]
    public void WriteEnumValueWithAttribute_IsWrittenCorrectly()
    {
        // Arrange
        TestNamingEnum value = TestNamingEnum.Item2SubItem1;

        using FormSerializationWriter formSerializationWriter = new();

        // Act
        formSerializationWriter.WriteEnumValue<TestNamingEnum>("prop1", value);
        Stream contentStream = formSerializationWriter.GetSerializedContent();
        using StreamReader reader = new(contentStream, Encoding.UTF8);
        string serializedString = reader.ReadToEnd();

        // Assert
        Assert.Equal("prop1=Item2%3ASubItem1", serializedString);
    }

    [Fact]
    public void WriteCollectionOfEnumValues_IsWrittenCorrectly()
    {
        // Arrange
        List<TestEnum?> value = [TestEnum.Sixteen, TestEnum.Two];

        using FormSerializationWriter formSerializationWriter = new();

        // Act
        formSerializationWriter.WriteCollectionOfEnumValues("prop1", value);
        Stream contentStream = formSerializationWriter.GetSerializedContent();
        using StreamReader reader = new(contentStream, Encoding.UTF8);
        string serializedString = reader.ReadToEnd();

        // Assert
        Assert.Equal("prop1=Sixteen%2CTwo", serializedString);
    }
}
