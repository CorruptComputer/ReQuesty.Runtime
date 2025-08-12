using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using ReQuesty.Runtime.Abstractions;
using ReQuesty.Runtime.Abstractions.Serialization;
using ReQuesty.Runtime.Serialization.Json.Tests.Converters;
using ReQuesty.Runtime.Serialization.Json.Tests.Mocks;
using Xunit;

namespace ReQuesty.Runtime.Serialization.Json.Tests;

public class JsonParseNodeTests
{
    public JsonParseNodeTests()
    {
        Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("cs-CZ");
    }
    private const string TestUserJson = "{\r\n" +
                                        "    \"@odata.context\": \"https://graph.microsoft.com/v1.0/$metadata#users/$entity\",\r\n" +
                                        "    \"@odata.id\": \"https://graph.microsoft.com/v2/dcd219dd-bc68-4b9b-bf0b-4a33a796be35/directoryObjects/48d31887-5fad-4d73-a9f5-3c356e68a038/Microsoft.DirectoryServices.User\",\r\n" +
                                        "    \"businessPhones\": [\r\n" +
                                        "        \"+1 412 555 0109\"\r\n" +
                                        "    ],\r\n" +
                                        "    \"displayName\": \"Megan Bowen\",\r\n" +
                                        "    \"numbers\":\"one,two,thirtytwo\"," +
                                        "    \"testNamingEnum\":\"Item2:SubItem1\"," +
                                        "    \"givenName\": \"Megan\",\r\n" +
                                        "    \"accountEnabled\": true,\r\n" +
                                        "    \"createdDateTime\": \"2017-07-29T03:07:25Z\",\r\n" +
                                        "    \"jobTitle\": \"Auditor\",\r\n" +
                                        "    \"mail\": \"MeganB@M365x214355.onmicrosoft.com\",\r\n" +
                                        "    \"mobilePhone\": null,\r\n" +
                                        "    \"officeLocation\": null,\r\n" +
                                        "    \"preferredLanguage\": \"en-US\",\r\n" +
                                        "    \"surname\": \"Bowen\",\r\n" +
                                        "    \"workDuration\": \"PT1H\",\r\n" +
                                        "    \"startWorkTime\": \"08:00:00.0000000\",\r\n" +
                                        "    \"endWorkTime\": \"17:00:00.0000000\",\r\n" +
                                        "    \"userPrincipalName\": \"MeganB@M365x214355.onmicrosoft.com\",\r\n" +
                                        "    \"birthDay\": \"2017-09-04\",\r\n" +
                                        "    \"id\": \"48d31887-5fad-4d73-a9f5-3c356e68a038\"\r\n" +
                                        "}";
    private const string TestStudentJson = "{\r\n" +
                                        "    \"@odata.context\": \"https://graph.microsoft.com/v1.0/$metadata#users/$entity\",\r\n" +
                                        "    \"@odata.type\": \"microsoft.graph.student\",\r\n" +
                                        "    \"@odata.id\": \"https://graph.microsoft.com/v2/dcd219dd-bc68-4b9b-bf0b-4a33a796be35/directoryObjects/48d31887-5fad-4d73-a9f5-3c356e68a038/Microsoft.DirectoryServices.User\",\r\n" +
                                        "    \"businessPhones\": [\r\n" +
                                        "        \"+1 412 555 0109\"\r\n" +
                                        "    ],\r\n" +
                                        "    \"displayName\": \"Megan Bowen\",\r\n" +
                                        "    \"numbers\":\"one,two,thirtytwo\"," +
                                        "    \"testNamingEnum\":\"Item2:SubItem1\"," +
                                        "    \"givenName\": \"Megan\",\r\n" +
                                        "    \"accountEnabled\": true,\r\n" +
                                        "    \"createdDateTime\": \"2017-07-29T03:07:25Z\",\r\n" +
                                        "    \"jobTitle\": \"Auditor\",\r\n" +
                                        "    \"mail\": \"MeganB@M365x214355.onmicrosoft.com\",\r\n" +
                                        "    \"mobilePhone\": null,\r\n" +
                                        "    \"officeLocation\": null,\r\n" +
                                        "    \"preferredLanguage\": \"en-US\",\r\n" +
                                        "    \"surname\": \"Bowen\",\r\n" +
                                        "    \"workDuration\": \"PT1H\",\r\n" +
                                        "    \"startWorkTime\": \"08:00:00.0000000\",\r\n" +
                                        "    \"endWorkTime\": \"17:00:00.0000000\",\r\n" +
                                        "    \"userPrincipalName\": \"MeganB@M365x214355.onmicrosoft.com\",\r\n" +
                                        "    \"birthDay\": \"2017-09-04\",\r\n" +
                                        "    \"enrolmentDate\": \"2017-09-04\",\r\n" +
                                        "    \"id\": \"48d31887-5fad-4d73-a9f5-3c356e68a038\"\r\n" +
                                        "}";

    private const string TestUntypedJson = "{\r\n" +
                                        "    \"@odata.context\": \"https://graph.microsoft.com/v1.0/$metadata#sites('contoso.sharepoint.com')/lists('fa631c4d-ac9f-4884-a7f5-13c659d177e3')/items('1')/fields/$entity\",\r\n" +
                                        "    \"id\": \"5\",\r\n" +
                                        "    \"title\": \"Project 101\",\r\n" +
                                        "    \"location\": {\r\n" +
                                        "        \"address\": {\r\n" +
                                        "            \"city\": \"Redmond\",\r\n" +
                                        "            \"postalCode\": \"98052\",\r\n" +
                                        "            \"state\": \"Washington\",\r\n" +
                                        "            \"street\": \"NE 36th St\"\r\n" +
                                        "        },\r\n" +
                                        "        \"coordinates\": {\r\n" +
                                        "            \"latitude\": 47.641942,\r\n" +
                                        "            \"longitude\": -122.127222\r\n" +
                                        "        },\r\n" +
                                        "        \"displayName\": \"Microsoft Building 92\",\r\n" +
                                        "        \"floorCount\": 50,\r\n" +
                                        "        \"hasReception\": true,\r\n" +
                                        "        \"contact\": null\r\n" +
                                        "    },\r\n" +
                                        "    \"keywords\": [\r\n" +
                                        "        {\r\n" +
                                        "            \"created\": \"2023-07-26T10:41:26Z\",\r\n" +
                                        "            \"label\": \"Keyword1\",\r\n" +
                                        "            \"termGuid\": \"10e9cc83-b5a4-4c8d-8dab-4ada1252dd70\",\r\n" +
                                        "            \"wssId\": 6442450942\r\n" +
                                        "        },\r\n" +
                                        "        {\r\n" +
                                        "            \"created\": \"2023-07-26T10:51:26Z\",\r\n" +
                                        "            \"label\": \"Keyword2\",\r\n" +
                                        "            \"termGuid\": \"2cae6c6a-9bb8-4a78-afff-81b88e735fef\",\r\n" +
                                        "            \"wssId\": 6442450943\r\n" +
                                        "        }\r\n" +
                                        "    ],\r\n" +
                                        "    \"detail\": null,\r\n" +
                                        "    \"table\": [[1,2,3],[4,5,6],[7,8,9]],\r\n" +
                                        "    \"extra\": {\r\n" +
                                        "        \"createdDateTime\":\"2024-01-15T00:00:00\\u002B00:00\"\r\n" +
                                        "    }\r\n" +
                                        "}";

    private const string TestCollectionOfEnumsJson = "[\r\n" +
                                                        "  \"Item2:SubItem1\",\r\n" +
                                                        "  \"Item3:SubItem1\"\r\n" +
                                                        "]";

    private static readonly string TestUserCollectionString = $"[{TestUserJson}]";

    private static readonly ReQuestyJsonSerializationContext ReadNumbersAsStringsContext = new(
        new JsonSerializerOptions()
        {
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        }
    );

    [Fact]
    public void GetsEntityValueFromJson()
    {
        // Arrange
        using JsonDocument jsonDocument = JsonDocument.Parse(TestUserJson);
        JsonParseNode jsonParseNode = new(jsonDocument.RootElement);
        // Act
        TestEntity testEntity = jsonParseNode.GetObjectValue(TestEntity.CreateFromDiscriminator);
        // Assert
        Assert.NotNull(testEntity);
        Assert.Null(testEntity.OfficeLocation);
        Assert.NotEmpty(testEntity.AdditionalData);
        Assert.True(testEntity.AdditionalData.ContainsKey("jobTitle"));
        Assert.True(testEntity.AdditionalData.ContainsKey("mobilePhone"));
        Assert.Equal("Auditor", testEntity.AdditionalData["jobTitle"]);
        Assert.Equal("48d31887-5fad-4d73-a9f5-3c356e68a038", testEntity.Id);
        Assert.Equal(TestEnum.One | TestEnum.Two, testEntity.Numbers); // Unknown enum value is not included
        Assert.Equal(TestNamingEnum.Item2SubItem1, testEntity.TestNamingEnum); // correct value is chosen
        Assert.Equal(TimeSpan.FromHours(1), testEntity.WorkDuration); // Parses timespan values
        Assert.Equal(new Time(8, 0, 0).ToString(), testEntity.StartWorkTime.ToString());// Parses time values
        Assert.Equal(new Time(17, 0, 0).ToString(), testEntity.EndWorkTime.ToString());// Parses time values
        Assert.Equal(new Date(2017, 9, 4).ToString(), testEntity.BirthDay.ToString());// Parses date values
    }
    [Fact]
    public void GetsFieldFromDerivedType()
    {
        // Arrange
        using JsonDocument jsonDocument = JsonDocument.Parse(TestStudentJson);
        JsonParseNode jsonParseNode = new(jsonDocument.RootElement);
        // Act
        DerivedTestEntity? testEntity = jsonParseNode.GetObjectValue(TestEntity.CreateFromDiscriminator) as DerivedTestEntity;
        // Assert
        Assert.NotNull(testEntity);
        Assert.NotNull(testEntity.EnrolmentDate);
    }

    [Fact]
    public void GetCollectionOfObjectValuesFromJson()
    {
        // Arrange
        using JsonDocument jsonDocument = JsonDocument.Parse(TestUserCollectionString);
        JsonParseNode jsonParseNode = new(jsonDocument.RootElement);
        // Act
        TestEntity[] testEntityCollection = jsonParseNode.GetCollectionOfObjectValues<TestEntity>(x => new TestEntity()).ToArray();
        // Assert
        Assert.NotEmpty(testEntityCollection);
        Assert.Equal("48d31887-5fad-4d73-a9f5-3c356e68a038", testEntityCollection[0].Id);
    }

    [Fact]
    public void GetsChildNodeAndGetCollectionOfPrimitiveValuesFromJsonParseNode()
    {
        // Arrange
        using JsonDocument jsonDocument = JsonDocument.Parse(TestUserJson);
        JsonParseNode rootParseNode = new(jsonDocument.RootElement);
        // Act to get business phones list
        IParseNode? phonesListChildNode = rootParseNode.GetChildNode("businessPhones");
        Assert.NotNull(phonesListChildNode);
        string[] phonesList = phonesListChildNode.GetCollectionOfPrimitiveValues<string>().ToArray();
        // Assert
        Assert.NotEmpty(phonesList);
        Assert.Equal("+1 412 555 0109", phonesList[0]);
    }

    [Fact]
    public void ReturnsDefaultIfChildNodeDoesNotExist()
    {
        // Arrange
        using JsonDocument jsonDocument = JsonDocument.Parse(TestUserJson);
        JsonParseNode rootParseNode = new(jsonDocument.RootElement);
        // Try to get an imaginary node value
        IParseNode? imaginaryNode = rootParseNode.GetChildNode("imaginaryNode");
        // Assert
        Assert.Null(imaginaryNode);
    }

    [Fact]
    public void ParseGuidWithConverter()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        string json = $"{{\"id\": \"{id:N}\"}}";
        JsonSerializerOptions serializerOptions = new(JsonSerializerDefaults.General)
        {
            Converters = { new JsonGuidConverter() }
        };
        ReQuestyJsonSerializationContext serializationContext = new(serializerOptions);
        using JsonDocument jsonDocument = JsonDocument.Parse(json);
        JsonParseNode rootParseNode = new(jsonDocument.RootElement, serializationContext);

        // Act
        ConverterTestEntity entity = rootParseNode.GetObjectValue(_ => new ConverterTestEntity());

        // Assert
        Assert.Equal(id, entity.Id);
    }

    [Fact]
    public void ParseGuidWithoutConverter()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        string json = $"{{\"id\": \"{id:D}\"}}";
        JsonSerializerOptions serializerOptions = new(JsonSerializerDefaults.General);
        ReQuestyJsonSerializationContext serializationContext = new(serializerOptions);
        using JsonDocument jsonDocument = JsonDocument.Parse(json);
        JsonParseNode rootParseNode = new(jsonDocument.RootElement, serializationContext);

        // Act
        ConverterTestEntity entity = rootParseNode.GetObjectValue(_ => new ConverterTestEntity());

        // Assert
        Assert.Equal(id, entity.Id);
    }

    [Fact]
    public void ParseGuidEmptyString()
    {
        // Arrange
        string json = $"{{\"id\": \"\"}}";
        JsonSerializerOptions serializerOptions = new(JsonSerializerDefaults.General);
        ReQuestyJsonSerializationContext serializationContext = new(serializerOptions);
        using JsonDocument jsonDocument = JsonDocument.Parse(json);
        JsonParseNode rootParseNode = new(jsonDocument.RootElement, serializationContext);

        // Act
        ConverterTestEntity entity = rootParseNode.GetObjectValue(_ => new ConverterTestEntity());

        // Assert
        Assert.Null(entity.Id);
    }

    [Fact]
    public void GetEntityWithUntypedNodesFromJson()
    {
        // Arrange
        using JsonDocument jsonDocument = JsonDocument.Parse(TestUntypedJson);
        JsonParseNode rootParseNode = new(jsonDocument.RootElement);
        // Act
        UntypedTestEntity entity = rootParseNode.GetObjectValue(UntypedTestEntity.CreateFromDiscriminatorValue);
        // Assert
        Assert.NotNull(entity);
        Assert.Equal("5", entity.Id);
        Assert.Equal("Project 101", entity.Title);
        Assert.NotNull(entity.Location);
        Assert.IsType<UntypedObject>(entity.Location); // creates untyped object
        UntypedObject location = (UntypedObject)entity.Location;
        IDictionary<string, UntypedNode> locationProperties = location.GetValue();
        Assert.IsType<UntypedObject>(locationProperties["address"]);
        Assert.IsType<UntypedString>(locationProperties["displayName"]); // creates untyped string
        Assert.IsType<UntypedInteger>(locationProperties["floorCount"]); // creates untyped number
        Assert.IsType<UntypedBoolean>(locationProperties["hasReception"]); // creates untyped boolean
        Assert.IsType<UntypedNull>(locationProperties["contact"]); // creates untyped null
        Assert.IsType<UntypedObject>(locationProperties["coordinates"]); // creates untyped null
        UntypedObject coordinates = (UntypedObject)locationProperties["coordinates"];
        IDictionary<string, UntypedNode> coordinatesProperties = coordinates.GetValue();
        Assert.IsType<UntypedDecimal>(coordinatesProperties["latitude"]); // creates untyped decimal
        Assert.IsType<UntypedDecimal>(coordinatesProperties["longitude"]);
        Assert.Equal("Microsoft Building 92", ((UntypedString)locationProperties["displayName"]).GetValue());
        Assert.Equal(50, ((UntypedInteger)locationProperties["floorCount"]).GetValue());
        Assert.True(((UntypedBoolean)locationProperties["hasReception"]).GetValue());
        Assert.Null(((UntypedNull)locationProperties["contact"]).GetValue());
        Assert.NotNull(entity.Keywords);
        Assert.IsType<UntypedArray>(entity.Keywords); // creates untyped array
        Assert.Equal(2, ((UntypedArray)entity.Keywords).GetValue().Count());
        Assert.Null(entity.Detail);
        object extra = entity.AdditionalData["extra"];
        Assert.NotNull(extra);
        Assert.NotNull(entity.Table);
        UntypedArray table = (UntypedArray)entity.Table;// the table is a collection
        foreach (UntypedNode value in table.GetValue())
        {
            UntypedArray row = (UntypedArray)value;
            Assert.NotNull(row);// The values are a nested collection
            foreach (UntypedNode item in row.GetValue())
            {
                UntypedInteger rowItem = (UntypedInteger)item;
                Assert.NotNull(rowItem);// The values are a nested collection
            }
        }
    }

    [Fact]
    public void GetCollectionOfEnumValuesFromJson()
    {
        using JsonDocument jsonDocument = JsonDocument.Parse(TestCollectionOfEnumsJson);
        JsonParseNode rootParseNode = new(jsonDocument.RootElement);
        TestNamingEnum[] values = rootParseNode.GetCollectionOfPrimitiveValues<TestNamingEnum>().ToArray();
        // Assert
        Assert.NotEmpty(values);
        Assert.Equal(TestNamingEnum.Item2SubItem1, values[0]);
        Assert.Equal(TestNamingEnum.Item3SubItem1, values[1]);
    }

    [Fact]
    public void GetCollectionOfNullableEnumValuesFromJson()
    {
        using JsonDocument jsonDocument = JsonDocument.Parse(TestCollectionOfEnumsJson);
        JsonParseNode rootParseNode = new(jsonDocument.RootElement);
        TestNamingEnum?[] values = rootParseNode.GetCollectionOfPrimitiveValues<TestNamingEnum?>().ToArray();
        // Assert
        Assert.NotEmpty(values);
        Assert.Equal(TestNamingEnum.Item2SubItem1, values[0]);
        Assert.Equal(TestNamingEnum.Item3SubItem1, values[1]);
    }

    [Fact]
    public void GetDateValue_ReturnNullWhenValueKindIsNotString()
    {
        // Arrange
        using JsonDocument jsonDocument = JsonDocument.Parse("{\"startDate\":\"2024-07-31\"}");
        JsonParseNode parseNode = new(jsonDocument.RootElement);

        // Act
        Date? result = parseNode.GetDateValue();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetDateValue_ReturnNullWhenDateParseFails()
    {
        // Arrange
        using JsonDocument jsonDocument = JsonDocument.Parse("\"2024-13-32\"");
        JsonParseNode parseNode = new(jsonDocument.RootElement);

        // Act
        Date? result = parseNode.GetDateValue();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetDateValue_ReturnDateWhenCustomConverterIsUsed()
    {
        // Arrange
        JsonSerializerOptions serializerOptions = new(JsonSerializerDefaults.General)
        {
            Converters = { new JsonDateConverter() }
        };
        ReQuestyJsonSerializationContext serializationContext = new(serializerOptions);

        using JsonDocument jsonDocument = JsonDocument.Parse("\"31---07---2024\"");
        JsonParseNode parseNode = new(jsonDocument.RootElement, serializationContext);

        // Act
        Date? result = parseNode.GetDateValue();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(31, result.Value.Day);
        Assert.Equal(7, result.Value.Month);
        Assert.Equal(2024, result.Value.Year);
    }

    [Fact]
    public void GetDateValue_ReturnCorrectDate()
    {
        // Arrange
        using JsonDocument jsonDocument = JsonDocument.Parse("\"2024-07-31\"");
        JsonParseNode parseNode = new(jsonDocument.RootElement);

        // Act
        Date? result = parseNode.GetDateValue();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(31, result.Value.Day);
        Assert.Equal(7, result.Value.Month);
        Assert.Equal(2024, result.Value.Year);
    }

    [Fact]
    public void GetTimeValue_ReturnNullWhenValueKindIsNotString()
    {
        // Arrange
        using JsonDocument jsonDocument = JsonDocument.Parse("{\"startTime\":\"12:34:56\"}");
        JsonParseNode parseNode = new(jsonDocument.RootElement);

        // Act
        Time? result = parseNode.GetTimeValue();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetTimeValue_ReturnNullWhenTimeParseFails()
    {
        // Arrange
        using JsonDocument jsonDocument = JsonDocument.Parse("\"12:60:56\"");
        JsonParseNode parseNode = new(jsonDocument.RootElement);

        // Act
        Time? result = parseNode.GetTimeValue();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetTimeValue_ReturnTimeWhenCustomConverterIsUsed()
    {
        // Arrange
        JsonSerializerOptions serializerOptions = new(JsonSerializerDefaults.General)
        {
            Converters = { new JsonTimeConverter() }
        };
        ReQuestyJsonSerializationContext serializationContext = new(serializerOptions);

        using JsonDocument jsonDocument = JsonDocument.Parse("\"12__34__56\"");
        JsonParseNode parseNode = new(jsonDocument.RootElement, serializationContext);

        // Act
        Time? result = parseNode.GetTimeValue();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(12, result.Value.Hour);
        Assert.Equal(34, result.Value.Minute);
        Assert.Equal(56, result.Value.Second);
    }

    [Fact]
    public void GetTimeValue_ReturnTime()
    {
        // Arrange
        using JsonDocument jsonDocument = JsonDocument.Parse("\"12:34:56\"");
        JsonParseNode parseNode = new(jsonDocument.RootElement);

        // Act
        Time? result = parseNode.GetTimeValue();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(12, result.Value.Hour);
        Assert.Equal(34, result.Value.Minute);
        Assert.Equal(56, result.Value.Second);
    }

    [Fact]
    public void GetDateTimeOffsetValue_ReturnNullWhenValueKindIsNotString()
    {
        // Arrange
        using JsonDocument jsonDocument = JsonDocument.Parse("{\"startDateTime\":\"2024-07-31\"}");
        JsonParseNode parseNode = new(jsonDocument.RootElement);

        // Act
        DateTimeOffset? result = parseNode.GetDateTimeOffsetValue();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetDateTimeOffsetValue_ReturnNullWhenDateTimeOffsetParseFails()
    {
        // Arrange
        using JsonDocument jsonDocument = JsonDocument.Parse("\"2024-13-32T12:34:56Z\"");
        JsonParseNode parseNode = new(jsonDocument.RootElement);

        // Act
        DateTimeOffset? result = parseNode.GetDateTimeOffsetValue();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetDateTimeOffsetValue_ReturnDateTimeOffsetWhenCustomConverterIsUsed()
    {
        // Arrange
        JsonSerializerOptions serializerOptions = new(JsonSerializerDefaults.General)
        {
            Converters = { new JsonDateTimeOffsetConverter() }
        };
        ReQuestyJsonSerializationContext serializationContext = new(serializerOptions);

        using JsonDocument jsonDocument = JsonDocument.Parse("\"31__07__2024T12_34_56Z\"");
        JsonParseNode parseNode = new(jsonDocument.RootElement, serializationContext);

        // Act
        DateTimeOffset? result = parseNode.GetDateTimeOffsetValue();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(31, result.Value.Day);
        Assert.Equal(7, result.Value.Month);
        Assert.Equal(2024, result.Value.Year);
        Assert.Equal(12, result.Value.Hour);
        Assert.Equal(34, result.Value.Minute);
        Assert.Equal(56, result.Value.Second);
    }

    [Fact]
    public void GetDateTimeOffsetValue_ReturnCorrectDateTimeOffset()
    {
        // Arrange
        using JsonDocument jsonDocument = JsonDocument.Parse("\"2024-07-31T12:34:56Z\"");
        JsonParseNode parseNode = new(jsonDocument.RootElement);

        // Act
        DateTimeOffset? result = parseNode.GetDateTimeOffsetValue();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(31, result.Value.Day);
        Assert.Equal(7, result.Value.Month);
        Assert.Equal(2024, result.Value.Year);
        Assert.Equal(12, result.Value.Hour);
        Assert.Equal(34, result.Value.Minute);
        Assert.Equal(56, result.Value.Second);
    }

    [Theory]
    [InlineData("42", 42)]
    [InlineData("\"42\"", null)]
    [InlineData("\"not-a-number\"", null)]
    [InlineData("null", null)]
    public void GetIntValue_CanReadNumber(string input, int? expected)
    {
        // Arrange
        using JsonDocument jsonDocument = JsonDocument.Parse(input);
        JsonParseNode parseNode = new(jsonDocument.RootElement);

        // Act, Assert
        Assert_CanReadNumber(parseNode.GetIntValue, expected);
    }

    [Theory]
    [InlineData("42", 42)]
    [InlineData("null", null)]
    [InlineData("\"not-a-number\"", null, "The JSON value could not be converted to System.Int32.")]
    [InlineData("\"42\"", 42)]
    public void GetIntValue_CanReadNumber_AsString(string input, int? expectedValue, string? expectexpectedExceptionMessage = null)
    {
        // Arrange
        using JsonDocument jsonDocument = JsonDocument.Parse(input);
        JsonParseNode parseNode = new(jsonDocument.RootElement, ReadNumbersAsStringsContext);

        // Act, Assert
        Assert_CanReadNumber(parseNode.GetIntValue, expectedValue, expectexpectedExceptionMessage);
    }

    [Theory]
    [InlineData("42", 42L)]
    [InlineData("\"42\"", null)]
    [InlineData("\"not-a-number\"", null)]
    [InlineData("null", null)]
    public void GetLongValue_CanReadNumber(string input, long? expected)
    {
        // Arrange
        using JsonDocument jsonDocument = JsonDocument.Parse(input);
        JsonParseNode parseNode = new(jsonDocument.RootElement);

        // Act, Assert
        Assert_CanReadNumber(parseNode.GetLongValue, expected);
    }

    [Theory]
    [InlineData("42", 42L)]
    [InlineData("null", null)]
    [InlineData("\"not-a-number\"", null, "The JSON value could not be converted to System.Int64.")]
    [InlineData("\"42\"", 42L)]
    public void GetLongValue_CanReadNumber_AsString(string input, long? expectedValue, string? expectedExceptionMessage = null)
    {
        // Arrange
        using JsonDocument jsonDocument = JsonDocument.Parse(input);
        JsonParseNode parseNode = new(jsonDocument.RootElement, ReadNumbersAsStringsContext);

        // Act, Assert
        Assert_CanReadNumber(parseNode.GetLongValue, expectedValue, expectedExceptionMessage);
    }

    [Theory]
    [InlineData("13.37", 13.37F)]
    [InlineData("\"13.37\"", null)]
    [InlineData("\"not-a-number\"", null)]
    [InlineData("null", null)]
    public void GetFloatValue_CanReadNumber(string input, float? expected)
    {
        // Arrange
        using JsonDocument jsonDocument = JsonDocument.Parse(input);
        JsonParseNode parseNode = new(jsonDocument.RootElement);

        // Act, Assert
        Assert_CanReadNumber(parseNode.GetFloatValue, expected);
    }

    [Theory]
    [InlineData("13.37", 13.37F)]
    [InlineData("null", null)]
    [InlineData("\"not-a-number\"", null, "The JSON value could not be converted to System.Single.")]
    [InlineData("\"13.37\"", 13.37F)]
    public void GetFloatValue_CanReadNumber_AsString(string input, float? expectedValue, string? expectedExceptionMessage = null)
    {
        // Arrange
        using JsonDocument jsonDocument = JsonDocument.Parse(input);
        JsonParseNode parseNode = new(jsonDocument.RootElement, ReadNumbersAsStringsContext);

        // Act, Assert
        Assert_CanReadNumber(parseNode.GetFloatValue, expectedValue, expectedExceptionMessage);
    }

    [Theory]
    [InlineData("13.37", 13.37D)]
    [InlineData("\"13.37\"", null)]
    [InlineData("\"not-a-number\"", null)]
    [InlineData("null", null)]
    public void GetDoubleValue_CanReadNumber(string input, double? expected)
    {
        // Arrange
        using JsonDocument jsonDocument = JsonDocument.Parse(input);
        JsonParseNode parseNode = new(jsonDocument.RootElement);

        // Act, Assert
        Assert_CanReadNumber(parseNode.GetDoubleValue, expected);
    }

    [Theory]
    [InlineData("13.37", 13.37D)]
    [InlineData("null", null)]
    [InlineData("\"not-a-number\"", null, "The JSON value could not be converted to System.Double.")]
    [InlineData("\"13.37\"", 13.37D)]
    public void GetDoubleValue_CanReadNumber_AsString(string input, double? expectedValue, string? expectedExceptionMessage = null)
    {
        // Arrange
        using JsonDocument jsonDocument = JsonDocument.Parse(input);
        JsonParseNode parseNode = new(jsonDocument.RootElement, ReadNumbersAsStringsContext);

        // Act, Assert
        Assert_CanReadNumber(parseNode.GetDoubleValue, expectedValue, expectedExceptionMessage);
    }

    [Theory]
    [InlineData("13.37", 13.37)]
    [InlineData("\"13.37\"", null)]
    [InlineData("\"not-a-number\"", null)]
    [InlineData("null", null)]
    public void GetDecimalValue_CanReadNumber(string input, double? expectedDouble)
    {
        // Arrange
        decimal? expectedValue = expectedDouble.HasValue
            ? Convert.ToDecimal(expectedDouble)
            : default(decimal?)
        ; //13.37M is not supported as a constant expression in attributes

        using JsonDocument jsonDocument = JsonDocument.Parse(input);
        JsonParseNode parseNode = new(jsonDocument.RootElement);

        Assert_CanReadNumber(parseNode.GetDecimalValue, expectedValue);
    }

    [Theory]
    [InlineData("13.37", 13.37)]
    [InlineData("null", null)]
    [InlineData("\"not-a-number\"", null, "The JSON value could not be converted to System.Decimal.")]
    [InlineData("\"13.37\"", 13.37)]
    public void GetDecimalValue_CanReadNumber_AsString(string input, double? expectedDouble, string? expectedExceptionMessage = null)
    {
        // Arrange
        decimal? expected = expectedDouble.HasValue
            ? Convert.ToDecimal(expectedDouble)
            : default(decimal?)
        ; //13.37M is not supported as a constant expression in attributes

        using JsonDocument jsonDocument = JsonDocument.Parse(input);
        JsonParseNode parseNode = new(jsonDocument.RootElement, ReadNumbersAsStringsContext);

        // Act, Assert
        Assert_CanReadNumber(parseNode.GetDecimalValue, expected, expectedExceptionMessage);
    }

    private static void Assert_CanReadNumber<TNumeric>(Func<TNumeric?> act, TNumeric? expectedValue, string? expectedExceptionMessage = null) where TNumeric : struct
    {
        if (string.IsNullOrEmpty(expectedExceptionMessage))
        {
            // Act
            TNumeric? actual = act();

            // Assert
            Assert.Equal(expectedValue.HasValue, actual.HasValue);
            if (expectedValue.HasValue && actual.HasValue)
            {
                Assert.Equal(expectedValue.Value, actual.Value);
            }
        }
        else
        {
            // Act, Assert
            JsonException exception = Assert.Throws<JsonException>(() => act());
            Assert.StartsWith(expectedExceptionMessage, exception.Message);
        }
    }
}
