using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using ReQuesty.Runtime.Abstractions;
using ReQuesty.Runtime.Abstractions.Serialization;
using ReQuesty.Runtime.Serialization.Json.Tests.Converters;
using ReQuesty.Runtime.Serialization.Json.Tests.Mocks;
using Xunit;

namespace ReQuesty.Runtime.Serialization.Json.Tests;

public class JsonSerializationWriterTests
{
    public JsonSerializationWriterTests()
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
            HeightInMetres = 1.80m,
            AdditionalData = new Dictionary<string, object>
            {
                {"mobilePhone", new UntypedNull()}, // write null value
                {"accountEnabled",false}, // write bool value
                {"jobTitle","Author"}, // write string value
                {"createdDateTime", DateTimeOffset.MinValue}, // write date value
                {"weightInKgs", 51.80m}, // write weigth
                {"businessPhones", new List<string>() {"+1 412 555 0109"}}, // write collection of primitives value
                {"dates",new List<DateTimeOffset> { DateTimeOffset.MaxValue , DateTimeOffset.MinValue }},
                {"endDateTime", new DateTime(2023,03,14,0,0,0,DateTimeKind.Utc) }, // ensure the DateTime doesn't crash
                {"manager", new TestEntity{Id = "48d31887-5fad-4d73-a9f5-3c356e68a038"}}, // write nested object value
                {"anonymousObject", new {Value1 = true, Value2 = "", Value3 = new List<string>{ "Value3.1", "Value3.2"}}}, // write nested object value
                {"dictionaryString", new Dictionary<string, string>{{"91bbe8e2-09b2-482b-a90e-00f8d7e81636", "b7992f48-a51b-41a1-ace5-4cebb7f111d0"}, { "ed64c116-2776-4012-94d1-a348b9d241bd", "55e1b4d0-2959-4c71-89b5-385ba5338a1c" }, }}, // write a Dictionary
                {"dictionaryTestEntity", new Dictionary<string, TestEntity>{{ "dd476fc9-7e97-4a4e-8d40-6c3de7432eb3", new TestEntity { Id = "dd476fc9-7e97-4a4e-8d40-6c3de7432eb3" } }, { "ffa5c351-7cf5-43df-9b55-e12455cf6eb2", new TestEntity { Id = "ffa5c351-7cf5-43df-9b55-e12455cf6eb2" } }, }}, // write a Dictionary
            }
        };

        using JsonSerializationWriter jsonSerializerWriter = new();
        // Act
        jsonSerializerWriter.WriteObjectValue(string.Empty, testEntity);
        // Get the json string from the stream.
        Stream serializedStream = jsonSerializerWriter.GetSerializedContent();
        using StreamReader reader = new(serializedStream, Encoding.UTF8);
        string serializedJsonString = reader.ReadToEnd();

        // Assert
        string expectedString = "{" +
                                "\"id\":\"48d31887-5fad-4d73-a9f5-3c356e68a038\"," +
                                "\"workDuration\":\"PT1H\"," +    // Serializes timespans
                                "\"birthDay\":\"2017-09-04\"," + // Serializes dates
                                "\"heightInMetres\":1.80," +
                                "\"startWorkTime\":\"08:00:00\"," + //Serializes times
                                "\"mobilePhone\":null," +
                                "\"accountEnabled\":false," +
                                "\"jobTitle\":\"Author\"," +
                                "\"createdDateTime\":\"0001-01-01T00:00:00+00:00\"," +
                                "\"weightInKgs\":51.80," +
                                "\"businessPhones\":[\"\\u002B1 412 555 0109\"]," +
                                "\"dates\":[\"9999-12-31T23:59:59.9999999+00:00\",\"0001-01-01T00:00:00+00:00\"]," +
                                "\"endDateTime\":\"2023-03-14T00:00:00+00:00\"," +
                                "\"manager\":{\"id\":\"48d31887-5fad-4d73-a9f5-3c356e68a038\"}," +
                                "\"anonymousObject\":{\"Value1\":true,\"Value2\":\"\",\"Value3\":[\"Value3.1\",\"Value3.2\"]}," +
                                "\"dictionaryString\":{\"91bbe8e2-09b2-482b-a90e-00f8d7e81636\":\"b7992f48-a51b-41a1-ace5-4cebb7f111d0\",\"ed64c116-2776-4012-94d1-a348b9d241bd\":\"55e1b4d0-2959-4c71-89b5-385ba5338a1c\"}," +
                                "\"dictionaryTestEntity\":{\"dd476fc9-7e97-4a4e-8d40-6c3de7432eb3\":{\"id\":\"dd476fc9-7e97-4a4e-8d40-6c3de7432eb3\"},\"ffa5c351-7cf5-43df-9b55-e12455cf6eb2\":{\"id\":\"ffa5c351-7cf5-43df-9b55-e12455cf6eb2\"}}" +
                                "}";
        Assert.Equal(expectedString, serializedJsonString);
    }

    [Fact]
    public void WritesSampleObjectValueWithJsonElementAdditionalData()
    {
        JsonElement arrayJsonElement = JsonDocument.Parse("[\"+1 412 555 0109\"]").RootElement;
        JsonElement objectJsonElement = JsonDocument.Parse("{\"id\":\"48d31887-5fad-4d73-a9f5-3c356e68a038\"}").RootElement;

        // Arrange
        TestEntity testEntity = new()
        {
            Id = "48d31887-5fad-4d73-a9f5-3c356e68a038",
            WorkDuration = TimeSpan.FromHours(1),
            StartWorkTime = new Time(8, 0, 0),
            BirthDay = new Date(2017, 9, 4),
            AdditionalData = new Dictionary<string, object>
            {
                {"mobilePhone", new UntypedNull()}, // write null value
                {"accountEnabled",false}, // write bool value
                {"jobTitle","Author"}, // write string value
                {"createdDateTime", DateTimeOffset.MinValue}, // write date value
                {"businessPhones", arrayJsonElement }, // write collection of primitives value
                {"manager", objectJsonElement }, // write nested object value
            }
        };
        using JsonSerializationWriter jsonSerializerWriter = new();
        // Act
        jsonSerializerWriter.WriteObjectValue(string.Empty, testEntity);
        // Get the json string from the stream.
        Stream serializedStream = jsonSerializerWriter.GetSerializedContent();
        using StreamReader reader = new(serializedStream, Encoding.UTF8);
        string serializedJsonString = reader.ReadToEnd();

        // Assert
        string expectedString = "{" +
                                "\"id\":\"48d31887-5fad-4d73-a9f5-3c356e68a038\"," +
                                "\"workDuration\":\"PT1H\"," +    // Serializes timespans
                                "\"birthDay\":\"2017-09-04\"," + // Serializes dates
                                "\"startWorkTime\":\"08:00:00\"," + //Serializes times
                                "\"mobilePhone\":null," +
                                "\"accountEnabled\":false," +
                                "\"jobTitle\":\"Author\"," +
                                "\"createdDateTime\":\"0001-01-01T00:00:00+00:00\"," +
                                "\"businessPhones\":[\"\\u002B1 412 555 0109\"]," +
                                "\"manager\":{\"id\":\"48d31887-5fad-4d73-a9f5-3c356e68a038\"}" +
                                "}";
        Assert.Equal(expectedString, serializedJsonString);
    }

    [Fact]
    public void WritesSampleCollectionOfObjectValues()
    {
        // Arrange
        TestEntity testEntity = new()
        {
            Id = "48d31887-5fad-4d73-a9f5-3c356e68a038",
            Numbers = TestEnum.One | TestEnum.Two,
            TestNamingEnum = TestNamingEnum.Item2SubItem1,
            AdditionalData = new Dictionary<string, object>
            {
                {"mobilePhone", new UntypedNull()}, // write null value
                {"accountEnabled",false}, // write bool value
                {"jobTitle","Author"}, // write string value
                {"createdDateTime", DateTimeOffset.MinValue}, // write date value
                {"businessPhones", new List<string>() {"+1 412 555 0109"}}, // write collection of primitives value
                {"manager", new TestEntity{Id = "48d31887-5fad-4d73-a9f5-3c356e68a038"}}, // write nested object value
            }
        };
        List<TestEntity> entityList = [testEntity];
        using JsonSerializationWriter jsonSerializerWriter = new();
        // Act
        jsonSerializerWriter.WriteCollectionOfObjectValues(string.Empty, entityList);
        // Get the json string from the stream.
        Stream serializedStream = jsonSerializerWriter.GetSerializedContent();
        using StreamReader reader = new(serializedStream, Encoding.UTF8);
        string serializedJsonString = reader.ReadToEnd();

        // Assert
        string expectedString = "[{" +
                                "\"id\":\"48d31887-5fad-4d73-a9f5-3c356e68a038\"," +
                                "\"numbers\":\"One,Two\"," +
                                "\"testNamingEnum\":\"Item2:SubItem1\"," +
                                "\"mobilePhone\":null," +
                                "\"accountEnabled\":false," +
                                "\"jobTitle\":\"Author\"," +
                                "\"createdDateTime\":\"0001-01-01T00:00:00+00:00\"," +
                                "\"businessPhones\":[\"\\u002B1 412 555 0109\"]," +
                                "\"manager\":{\"id\":\"48d31887-5fad-4d73-a9f5-3c356e68a038\"}" +
                                "}]";
        Assert.Equal(expectedString, serializedJsonString);
    }

    [Fact]
    public void DoesntWriteUnsupportedTypes_NonStringKeyedDictionary()
    {
        // Arrange
        TestEntity testEntity = new()
        {
            AdditionalData = new Dictionary<string, object>
            {
                {"nonStringKeyedDictionary", new Dictionary<int, string>{{ 1, "one" }, { 2, "two" }}}
            }
        };

        using JsonSerializationWriter jsonSerializerWriter = new();
        // Act & Assert
        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => jsonSerializerWriter.WriteObjectValue(string.Empty, testEntity));
        Assert.Equal("Error serializing dictionary value with key nonStringKeyedDictionary, only string keyed dictionaries are supported.", exception.Message);
    }

    [Fact]
    public void WritesEnumValuesAsCamelCasedIfNotEscaped()
    {
        // Arrange
        TestEntity testEntity = new()
        {
            TestNamingEnum = TestNamingEnum.Item1,
        };
        List<TestEntity> entityList = [testEntity];
        using JsonSerializationWriter jsonSerializerWriter = new();
        // Act
        jsonSerializerWriter.WriteCollectionOfObjectValues(string.Empty, entityList);
        // Get the json string from the stream.
        Stream serializedStream = jsonSerializerWriter.GetSerializedContent();
        using StreamReader reader = new(serializedStream, Encoding.UTF8);
        string serializedJsonString = reader.ReadToEnd();

        // Assert
        string expectedString = "[{" +
                                "\"testNamingEnum\":\"Item1\"" + // Camel Cased
                                "}]";
        Assert.Equal(expectedString, serializedJsonString);
    }

    [Fact]
    public void WritesEnumValuesAsDescribedIfEscaped()
    {
        // Arrange
        TestEntity testEntity = new()
        {
            TestNamingEnum = TestNamingEnum.Item2SubItem1,
        };
        List<TestEntity> entityList = [testEntity];
        using JsonSerializationWriter jsonSerializerWriter = new();
        // Act
        jsonSerializerWriter.WriteCollectionOfObjectValues(string.Empty, entityList);
        // Get the json string from the stream.
        Stream serializedStream = jsonSerializerWriter.GetSerializedContent();
        using StreamReader reader = new(serializedStream, Encoding.UTF8);
        string serializedJsonString = reader.ReadToEnd();

        // Assert
        string expectedString = "[{" +
                                "\"testNamingEnum\":\"Item2:SubItem1\"" + // Appears same as attribute
                                "}]";
        Assert.Equal(expectedString, serializedJsonString);
    }

    [Fact]
    public void WriteGuidUsingConverter()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        ConverterTestEntity testEntity = new() { Id = id };
        JsonSerializerOptions serializerOptions = new(JsonSerializerDefaults.General)
        {
            Converters = { new JsonGuidConverter() }
        };
        ReQuestyJsonSerializationContext serializationContext = new(serializerOptions);
        using JsonSerializationWriter jsonSerializerWriter = new(serializationContext);

        // Act
        jsonSerializerWriter.WriteObjectValue(string.Empty, testEntity);
        Stream serializedStream = jsonSerializerWriter.GetSerializedContent();
        using StreamReader reader = new(serializedStream, Encoding.UTF8);
        string serializedJsonString = reader.ReadToEnd();

        // Assert
        string expectedString = $"{{\"id\":\"{id:N}\"}}";
        Assert.Equal(expectedString, serializedJsonString);
    }

    [Fact]
    public void ForwardsOptionsToWriterFromSerializationContext()
    {
        // Arrange
        TestEntity testEntity = new()
        {
            Id = "testId",
            AdditionalData = new Dictionary<string, object>()
            {
                {"href", "https://graph.microsoft.com/users/{user-id}"},
                {"unicodeName", "你好"}
            }
        };
        JsonSerializerOptions serializerOptions = new(JsonSerializerDefaults.General)
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        ReQuestyJsonSerializationContext serializationContext = new(serializerOptions);
        using JsonSerializationWriter jsonSerializerWriter = new(serializationContext);

        // Act
        jsonSerializerWriter.WriteObjectValue(string.Empty, testEntity);
        Stream serializedStream = jsonSerializerWriter.GetSerializedContent();
        using StreamReader reader = new(serializedStream, Encoding.UTF8);
        string serializedJsonString = reader.ReadToEnd();

        // Assert
        const string expectedString = "{\n  \"id\": \"testId\",\n  \"href\": \"https://graph.microsoft.com/users/{user-id}\",\n  \"unicodeName\": \"你好\"\n}";
        Assert.Contains("\n", serializedJsonString); // string is indented and not escaped
        Assert.Contains("你好", serializedJsonString); // string is indented and not escaped
        Assert.Equal(expectedString, serializedJsonString.Replace("\r", string.Empty)); // string is indented and not escaped
    }

    [Fact]
    public void WritesPrimitiveCollectionsInAdditionalData()
    {
        // Arrange
        List<DateTimeOffset> dates =
        [
            DateTimeOffset.MaxValue, DateTimeOffset.MinValue
        ];
        TestEntity testEntity = new()
        {
            Id = "testId",
            AdditionalData = new Dictionary<string, object>()
            {
                {"dates", dates}
            }
        };
        using JsonSerializationWriter jsonSerializerWriter = new();
        // Act
        jsonSerializerWriter.WriteObjectValue(string.Empty, testEntity);
        Stream serializedStream = jsonSerializerWriter.GetSerializedContent();
        using StreamReader reader = new(serializedStream, Encoding.UTF8);
        string serializedJsonString = reader.ReadToEnd();

        // Assert
        Assert.Contains("\"id\":\"testId\"", serializedJsonString);
        Assert.Contains("\"dates\":[\"", serializedJsonString);
        Assert.Contains(JsonSerializer.Serialize(DateTimeOffset.MinValue), serializedJsonString);
        Assert.Contains(JsonSerializer.Serialize(DateTimeOffset.MaxValue), serializedJsonString);
    }

    [Fact]
    public void UsesDefaultOptionsToWriterFromSerializationContext()
    {
        // Arrange
        TestEntity testEntity = new()
        {
            Id = "testId",
            AdditionalData = new Dictionary<string, object>()
            {
                {"href", "https://graph.microsoft.com/users/{user-id}"},
                {"unicodeName", "你好"}
            }
        };
        using JsonSerializationWriter jsonSerializerWriter = new(new ReQuestyJsonSerializationContext());

        // Act
        jsonSerializerWriter.WriteObjectValue(string.Empty, testEntity);
        Stream serializedStream = jsonSerializerWriter.GetSerializedContent();
        using StreamReader reader = new(serializedStream, Encoding.UTF8);
        string serializedJsonString = reader.ReadToEnd();

        // Assert
        string expectedString = $"{{\"id\":\"testId\",\"href\":\"https://graph.microsoft.com/users/{{user-id}}\",\"unicodeName\":\"\\u4F60\\u597D\"}}";
        Assert.DoesNotContain("\n", serializedJsonString); // string is not indented and not escaped
        Assert.DoesNotContain("你好", serializedJsonString); // string is not indented and not escaped
        Assert.Contains("\\u4F60\\u597D", serializedJsonString); // string is not indented and not escaped
        Assert.Equal(expectedString, serializedJsonString); // string is indented and not escaped
    }
    [Fact]
    public void WriteGuidUsingNoConverter()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        ConverterTestEntity testEntity = new() { Id = id };
        JsonSerializerOptions serializerOptions = new(JsonSerializerDefaults.General);
        ReQuestyJsonSerializationContext serializationContext = new(serializerOptions);
        using JsonSerializationWriter jsonSerializerWriter = new(serializationContext);

        // Act
        jsonSerializerWriter.WriteObjectValue(string.Empty, testEntity);
        Stream serializedStream = jsonSerializerWriter.GetSerializedContent();
        using StreamReader reader = new(serializedStream, Encoding.UTF8);
        string serializedJsonString = reader.ReadToEnd();

        // Assert
        string expectedString = $"{{\"id\":\"{id:D}\"}}";
        Assert.Equal(expectedString, serializedJsonString);
    }
    [Fact]
    public void WritesSampleObjectValueWithUntypedProperties()
    {
        // Arrange
        UntypedTestEntity untypedTestEntity = new()
        {
            Id = "1",
            Title = "Title",
            Location = new UntypedObject(new Dictionary<string, UntypedNode>
                {
                    {"address", new UntypedObject(new Dictionary<string, UntypedNode>
                    {
                        {"city", new UntypedString("Redmond") },
                        {"postalCode", new UntypedString("98052") },
                        {"state", new UntypedString("Washington") },
                        {"street", new UntypedString("NE 36th St") }
                    })},
                    {"coordinates", new UntypedObject(new Dictionary<string, UntypedNode>
                    {
                        {"latitude", new UntypedDouble(47.641942d) },
                        {"longitude", new UntypedDouble(-122.127222d) }
                    })},
                    {"displayName", new UntypedString("Microsoft Building 92") },
                    {"floorCount", new UntypedInteger(50) },
                    {"hasReception", new UntypedBoolean(true) },
                    {"contact", new UntypedNull() }
                }),
            Keywords = new UntypedArray(new List<UntypedNode>
            {
                new UntypedObject(new Dictionary<string, UntypedNode>
                {
                    {"created", new UntypedString("2023-07-26T10:41:26Z") },
                    {"label", new UntypedString("Keyword1") },
                    {"termGuid", new UntypedString("10e9cc83-b5a4-4c8d-8dab-4ada1252dd70") },
                    {"wssId", new UntypedLong(6442450941) }
                }),
                new UntypedObject(new Dictionary<string, UntypedNode>
                {
                    {"created", new UntypedString("2023-07-26T10:51:26Z") },
                    {"label", new UntypedString("Keyword2") },
                    {"termGuid", new UntypedString("2cae6c6a-9bb8-4a78-afff-81b88e735fef") },
                    {"wssId", new UntypedLong(6442450942) }
                })
            }),
            AdditionalData = new Dictionary<string, object>
            {
                { "extra", new UntypedObject(new Dictionary<string, UntypedNode>
                {
                    {"createdDateTime", new UntypedString("2024-01-15T00:00:00+00:00") }
                }) }
            }
        };
        using JsonSerializationWriter jsonSerializerWriter = new();
        // Act
        jsonSerializerWriter.WriteObjectValue(string.Empty, untypedTestEntity);
        // Get the json string from the stream.
        Stream serializedStream = jsonSerializerWriter.GetSerializedContent();
        using StreamReader reader = new(serializedStream, Encoding.UTF8);
        string serializedJsonString = reader.ReadToEnd();

        // Assert
        string expectedString = "{" +
            "\"id\":\"1\"," +
            "\"title\":\"Title\"," +
            "\"location\":{" +
            "\"address\":{\"city\":\"Redmond\",\"postalCode\":\"98052\",\"state\":\"Washington\",\"street\":\"NE 36th St\"}," +
            "\"coordinates\":{\"latitude\":47.641942,\"longitude\":-122.127222}," +
            "\"displayName\":\"Microsoft Building 92\"," +
            "\"floorCount\":50," +
            "\"hasReception\":true," +
            "\"contact\":null}," +
            "\"keywords\":[" +
            "{\"created\":\"2023-07-26T10:41:26Z\",\"label\":\"Keyword1\",\"termGuid\":\"10e9cc83-b5a4-4c8d-8dab-4ada1252dd70\",\"wssId\":6442450941}," +
            "{\"created\":\"2023-07-26T10:51:26Z\",\"label\":\"Keyword2\",\"termGuid\":\"2cae6c6a-9bb8-4a78-afff-81b88e735fef\",\"wssId\":6442450942}]," +
            "\"extra\":{\"createdDateTime\":\"2024-01-15T00:00:00\\u002B00:00\"}}";
        Assert.Equal(expectedString, serializedJsonString);
    }

    [Fact]
    public void WritesStringValue()
    {
        // Arrange
        string value = "This is a string value";

        using JsonSerializationWriter jsonSerializationWriter = new();

        // Act
        jsonSerializationWriter.WriteStringValue(null, value);
        Stream contentStream = jsonSerializationWriter.GetSerializedContent();
        using StreamReader reader = new(contentStream, Encoding.UTF8);
        string serializedString = reader.ReadToEnd();

        // Assert
        Assert.Equal("\"This is a string value\"", serializedString);
    }

    [Fact]
    public void StreamIsReadableAfterDispose()
    {
        // Arrange
        string value = "This is a string value";

        using JsonSerializationWriter jsonSerializationWriter = new();

        // Act
        jsonSerializationWriter.WriteStringValue(null, value);
        Stream contentStream = jsonSerializationWriter.GetSerializedContent();
        // Dispose the writer
        jsonSerializationWriter.Dispose();
        using StreamReader reader = new(contentStream, Encoding.UTF8);
        string serializedString = reader.ReadToEnd();

        // Assert
        Assert.Equal("\"This is a string value\"", serializedString);
    }

    [Fact]
    public void WriteBoolValue_IsWrittenCorrectly()
    {
        // Arrange
        bool value = true;

        using JsonSerializationWriter jsonSerializationWriter = new();

        // Act
        jsonSerializationWriter.WriteBoolValue(null, value);
        Stream contentStream = jsonSerializationWriter.GetSerializedContent();
        using StreamReader reader = new(contentStream, Encoding.UTF8);
        string serializedString = reader.ReadToEnd();

        // Assert
        Assert.Equal("true", serializedString);
    }

    [Fact]
    public void WriteByteArrayValue_IsWrittenCorrectly()
    {
        // Arrange
        byte[] value = new byte[] { 2, 4, 6 };

        using JsonSerializationWriter jsonSerializationWriter = new();

        // Act
        jsonSerializationWriter.WriteByteArrayValue(null, value);
        Stream contentStream = jsonSerializationWriter.GetSerializedContent();
        using StreamReader reader = new(contentStream, Encoding.UTF8);
        string serializedString = reader.ReadToEnd();

        // Assert
        Assert.Equal("\"AgQG\"", serializedString);
    }

    [Fact]
    public void WriteByteValue_IsWrittenCorrectly()
    {
        // Arrange
        byte value = 5;

        using JsonSerializationWriter jsonSerializationWriter = new();

        // Act
        jsonSerializationWriter.WriteByteValue(null, value);
        Stream contentStream = jsonSerializationWriter.GetSerializedContent();
        using StreamReader reader = new(contentStream, Encoding.UTF8);
        string serializedString = reader.ReadToEnd();

        // Assert
        Assert.Equal("5", serializedString);
    }

    [Fact]
    public void WriteDateTimeOffsetValue_IsWrittenCorrectly()
    {
        // Arrange
        DateTimeOffset value = new(2024, 11, 30, 15, 35, 45, 987, TimeSpan.FromHours(3));

        using JsonSerializationWriter jsonSerializationWriter = new();

        // Act
        jsonSerializationWriter.WriteDateTimeOffsetValue(null, value);
        Stream contentStream = jsonSerializationWriter.GetSerializedContent();
        using StreamReader reader = new(contentStream, Encoding.UTF8);
        string serializedString = reader.ReadToEnd();

        // Assert
        Assert.Equal("\"2024-11-30T15:35:45.987+03:00\"", serializedString);
    }

    [Fact]
    public void WriteDateValue_IsWrittenCorrectly()
    {
        // Arrange
        Date value = new(2024, 11, 30);

        using JsonSerializationWriter jsonSerializationWriter = new();

        // Act
        jsonSerializationWriter.WriteDateValue(null, value);
        Stream contentStream = jsonSerializationWriter.GetSerializedContent();
        using StreamReader reader = new(contentStream, Encoding.UTF8);
        string serializedString = reader.ReadToEnd();

        // Assert
        Assert.Equal("\"2024-11-30\"", serializedString);
    }

    [Fact]
    public void WriteDecimalValue_IsWrittenCorrectly()
    {
        // Arrange
        decimal value = 36.8m;

        using JsonSerializationWriter jsonSerializationWriter = new();

        // Act
        jsonSerializationWriter.WriteDecimalValue(null, value);
        Stream contentStream = jsonSerializationWriter.GetSerializedContent();
        using StreamReader reader = new(contentStream, Encoding.UTF8);
        string serializedString = reader.ReadToEnd();

        // Assert
        Assert.Equal("36.8", serializedString);
    }

    [Fact]
    public void WriteDoubleValue_IsWrittenCorrectly()
    {
        // Arrange
        double value = 36.8d;

        using JsonSerializationWriter jsonSerializationWriter = new();

        // Act
        jsonSerializationWriter.WriteDoubleValue(null, value);
        Stream contentStream = jsonSerializationWriter.GetSerializedContent();
        using StreamReader reader = new(contentStream, Encoding.UTF8);
        string serializedString = reader.ReadToEnd();

        // Assert
        Assert.Equal("36.8", serializedString);
    }

    [Fact]
    public void WriteFloatValue_IsWrittenCorrectly()
    {
        // Arrange
        float value = 36.8f;

        using JsonSerializationWriter jsonSerializationWriter = new();

        // Act
        jsonSerializationWriter.WriteFloatValue(null, value);
        Stream contentStream = jsonSerializationWriter.GetSerializedContent();
        using StreamReader reader = new(contentStream, Encoding.UTF8);
        string serializedString = reader.ReadToEnd();

        // Assert
        Assert.Equal("36.8", serializedString);
    }

    [Fact]
    public void WriteGuidValue_IsWrittenCorrectly()
    {
        // Arrange
        Guid value = new("3adeb301-58f1-45c5-b820-ae5f4af13c89");

        using JsonSerializationWriter jsonSerializationWriter = new();

        // Act
        jsonSerializationWriter.WriteGuidValue(null, value);
        Stream contentStream = jsonSerializationWriter.GetSerializedContent();
        using StreamReader reader = new(contentStream, Encoding.UTF8);
        string serializedString = reader.ReadToEnd();

        // Assert
        Assert.Equal("\"3adeb301-58f1-45c5-b820-ae5f4af13c89\"", serializedString);
    }

    [Fact]
    public void WriteIntegerValue_IsWrittenCorrectly()
    {
        // Arrange
        int value = 25;

        using JsonSerializationWriter jsonSerializationWriter = new();

        // Act
        jsonSerializationWriter.WriteIntValue(null, value);
        Stream contentStream = jsonSerializationWriter.GetSerializedContent();
        using StreamReader reader = new(contentStream, Encoding.UTF8);
        string serializedString = reader.ReadToEnd();

        // Assert
        Assert.Equal("25", serializedString);
    }

    [Fact]
    public void WriteLongValue_IsWrittenCorrectly()
    {
        // Arrange
        long value = long.MaxValue;

        using JsonSerializationWriter jsonSerializationWriter = new();

        // Act
        jsonSerializationWriter.WriteLongValue(null, value);
        Stream contentStream = jsonSerializationWriter.GetSerializedContent();
        using StreamReader reader = new(contentStream, Encoding.UTF8);
        string serializedString = reader.ReadToEnd();

        // Assert
        Assert.Equal("9223372036854775807", serializedString);
    }

    [Fact]
    public void WriteNullValue_IsWrittenCorrectly()
    {
        // Arrange
        using JsonSerializationWriter jsonSerializationWriter = new();

        // Act
        jsonSerializationWriter.WriteNullValue(null);
        Stream contentStream = jsonSerializationWriter.GetSerializedContent();
        using StreamReader reader = new(contentStream, Encoding.UTF8);
        string serializedString = reader.ReadToEnd();

        // Assert
        Assert.Equal("null", serializedString);
    }

    [Fact]
    public void WriteSByteValue_IsWrittenCorrectly()
    {
        // Arrange
        sbyte value = sbyte.MaxValue;

        using JsonSerializationWriter jsonSerializationWriter = new();

        // Act
        jsonSerializationWriter.WriteSbyteValue(null, value);
        Stream contentStream = jsonSerializationWriter.GetSerializedContent();
        using StreamReader reader = new(contentStream, Encoding.UTF8);
        string serializedString = reader.ReadToEnd();

        // Assert
        Assert.Equal("127", serializedString);
    }

    [Fact]
    public void WriteTimeValue_IsWrittenCorrectly()
    {
        // Arrange
        Time value = new(23, 46, 59);

        using JsonSerializationWriter jsonSerializationWriter = new();

        // Act
        jsonSerializationWriter.WriteTimeValue(null, value);
        Stream contentStream = jsonSerializationWriter.GetSerializedContent();
        using StreamReader reader = new(contentStream, Encoding.UTF8);
        string serializedString = reader.ReadToEnd();

        // Assert
        Assert.Equal("\"23:46:59\"", serializedString);
    }

    [Fact]
    public void WriteTimeSpanValue_IsWrittenCorrectly()
    {
        // Arrange
        TimeSpan value = new(756, 4, 6, 8, 10);

        using JsonSerializationWriter jsonSerializationWriter = new();

        // Act
        jsonSerializationWriter.WriteTimeSpanValue(null, value);
        Stream contentStream = jsonSerializationWriter.GetSerializedContent();
        using StreamReader reader = new(contentStream, Encoding.UTF8);
        string serializedString = reader.ReadToEnd();

        // Assert
        Assert.Equal("\"P756DT4H6M8.01S\"", serializedString);
    }
    [Fact]
    public async Task SerializesNullPropertiesForBackingStoreOnce()
    {
        BackedTestEntity value = new()
        {
            Name = null,
            Id = "testId",
        };
        SerializationWriterFactoryRegistry registry = new();
        JsonSerializationWriterFactory serializationJsonWriterFactory = new();
        registry.ContentTypeAssociatedFactories.TryAdd(serializationJsonWriterFactory.ValidContentType, serializationJsonWriterFactory);
        ISerializationWriterFactory backedWriterFactory = ApiClientBuilder.EnableBackingStoreForSerializationWriterFactory(registry);
        ISerializationWriter writer = backedWriterFactory.GetSerializationWriter(serializationJsonWriterFactory.ValidContentType);
        writer.WriteObjectValue(null, value);
        Stream contentStream = writer.GetSerializedContent();
        using StreamReader reader = new(contentStream, Encoding.UTF8);
        string serializedString = await reader.ReadToEndAsync();
        string expected =
        """
        {
            "name": null,
            "id": "testId"
        }
        """;
        JsonNode? expectedJsonNode = JsonNode.Parse(expected);
        JsonNode? actualJsonNode = JsonNode.Parse(serializedString);
        Assert.True(JsonNode.DeepEquals(expectedJsonNode, actualJsonNode));
    }
}
