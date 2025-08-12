using System.Globalization;
using ReQuesty.Runtime.Abstractions;
using ReQuesty.Runtime.Serialization.Form.Tests.Mocks;

namespace ReQuesty.Runtime.Serialization.Form.Tests;
public class FormParseNodeTests
{
    public FormParseNodeTests()
    {
        Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("cs-CZ");
    }

    private const string TestUserForm = "displayName=Megan+Bowen&" +
                                        "numbers=one,two,thirtytwo&" +
                                        "givenName=Megan&" +
                                        "accountEnabled=true&" +
                                        "createdDateTime=2017-07-29T03:07:25Z&" +
                                        "jobTitle=Auditor&" +
                                        "mail=MeganB@M365x214355.onmicrosoft.com&" +
                                        "mobilePhone=null&" +
                                        "officeLocation=null&" +
                                        "preferredLanguage=en-US&" +
                                        "surname=Bowen&" +
                                        "workDuration=PT1H&" +
                                        "startWorkTime=08:00:00.0000000&" +
                                        "endWorkTime=17:00:00.0000000&" +
                                        "userPrincipalName=MeganB@M365x214355.onmicrosoft.com&" +
                                        "birthDay=2017-09-04&" +
                                        "deviceNames=device1&deviceNames=device2&" + //collection property
                                        "otherPhones=123456789&otherPhones=987654321&" + //collection property for additionalData
                                        "id=48d31887-5fad-4d73-a9f5-3c356e68a038";

    [Fact]
    public void GetsEntityValueFromForm()
    {
        // Arrange
        FormParseNode formParseNode = new(TestUserForm);
        // Act
        TestEntity testEntity = formParseNode.GetObjectValue<TestEntity>(x => new TestEntity());
        // Assert
        Assert.NotNull(testEntity);
        Assert.Null(testEntity.OfficeLocation);
        Assert.NotEmpty(testEntity.AdditionalData);
        Assert.Equal(2, testEntity.DeviceNames?.Count);// collection is deserialized
        Assert.True(testEntity.AdditionalData.ContainsKey("jobTitle"));
        Assert.True(testEntity.AdditionalData.ContainsKey("mobilePhone"));
        Assert.True(testEntity.AdditionalData.ContainsKey("otherPhones"));
        Assert.Equal("true", testEntity.AdditionalData["accountEnabled"]);
        Assert.Equal("Auditor", testEntity.AdditionalData["jobTitle"]);
        Assert.Equal("123456789,987654321", testEntity.AdditionalData["otherPhones"]);
        Assert.Equal("48d31887-5fad-4d73-a9f5-3c356e68a038", testEntity.Id);
        Assert.Equal(TestEnum.One | TestEnum.Two, testEntity.Numbers); // Unknown enum value is not included
        Assert.Equal(TimeSpan.FromHours(1), testEntity.WorkDuration); // Parses timespan values
        Assert.Equal(new Time(8, 0, 0).ToString(), testEntity.StartWorkTime.ToString());// Parses time values
        Assert.Equal(new Time(17, 0, 0).ToString(), testEntity.EndWorkTime.ToString());// Parses time values
        Assert.Equal(new Date(2017, 9, 4).ToString(), testEntity.BirthDay.ToString());// Parses date values
    }

    [Fact]
    public void GetCollectionOfNumberPrimitiveValuesFromForm()
    {
        string TestFormData = "numbers=1&" +
                              "numbers=2&" +
                              "numbers=3&";
        FormParseNode formParseNode = new(TestFormData);
        Abstractions.Serialization.IParseNode? numberNode = formParseNode.GetChildNode("numbers");
        IEnumerable<int?>? numberCollection = numberNode?.GetCollectionOfPrimitiveValues<int?>();
        Assert.NotNull(numberCollection);
        Assert.Equal(3, numberCollection.Count());
        Assert.Equal(1, numberCollection.First());
        IEnumerable<string?>? numberCollectionAsStrings = numberNode?.GetCollectionOfPrimitiveValues<string?>();
        Assert.NotNull(numberCollectionAsStrings);
        Assert.Equal(3, numberCollectionAsStrings.Count());
        Assert.Equal("1", numberCollectionAsStrings.First());
        IEnumerable<sbyte?>? numberCollectionAsShort = numberNode?.GetCollectionOfPrimitiveValues<sbyte?>();
        Assert.NotNull(numberCollectionAsShort);
        Assert.Equal(3, numberCollectionAsShort.Count());
        Assert.Equal((sbyte)1, numberCollectionAsShort.First());
        IEnumerable<double?>? numberCollectionAsDouble = numberNode?.GetCollectionOfPrimitiveValues<double?>();
        Assert.NotNull(numberCollectionAsDouble);
        Assert.Equal(3, numberCollectionAsDouble.Count());
        Assert.Equal((double)1, numberCollectionAsDouble.First());
        IEnumerable<float?>? numberCollectionAsFloat = numberNode?.GetCollectionOfPrimitiveValues<float?>();
        Assert.NotNull(numberCollectionAsFloat);
        Assert.Equal(3, numberCollectionAsFloat.Count());
        Assert.Equal((float)1, numberCollectionAsFloat.First());
        IEnumerable<decimal?>? numberCollectionAsDecimal = numberNode?.GetCollectionOfPrimitiveValues<decimal?>();
        Assert.NotNull(numberCollectionAsDecimal);
        Assert.Equal(3, numberCollectionAsDecimal.Count());
        Assert.Equal((decimal)1, numberCollectionAsDecimal.First());
    }

    [Fact]
    public void GetCollectionOfBooleanPrimitiveValuesFromForm()
    {
        string TestFormData = "bools=true&" +
                              "bools=false";
        FormParseNode formParseNode = new(TestFormData);
        Abstractions.Serialization.IParseNode? numberNode = formParseNode.GetChildNode("bools");
        IEnumerable<bool?>? numberCollection = numberNode?.GetCollectionOfPrimitiveValues<bool?>();
        Assert.NotNull(numberCollection);
        Assert.Equal(2, numberCollection.Count());
        Assert.Equal(true, numberCollection.First());
    }

    [Fact]
    public void GetCollectionOfGuidPrimitiveValuesFromForm()
    {
        string TestFormData = "ids=48d31887-5fad-4d73-a9f5-3c356e68a038&" +
                              "ids=48d31887-5fad-4d73-a9f5-3c356e68a038";
        FormParseNode formParseNode = new(TestFormData);
        Abstractions.Serialization.IParseNode? numberNode = formParseNode.GetChildNode("ids");
        IEnumerable<Guid?>? numberCollection = numberNode?.GetCollectionOfPrimitiveValues<Guid?>();
        Assert.NotNull(numberCollection);
        Assert.Equal(2, numberCollection.Count());
        Assert.Equal(Guid.Parse("48d31887-5fad-4d73-a9f5-3c356e68a038"), numberCollection.First());
    }

    [Fact]
    public void GetCollectionOfObjectValuesFromForm()
    {
        FormParseNode formParseNode = new(TestUserForm);
        Assert.Throws<InvalidOperationException>(() => formParseNode.GetCollectionOfObjectValues<TestEntity>(static x => new TestEntity()));
    }

    [Fact]
    public void ReturnsDefaultIfChildNodeDoesNotExist()
    {
        // Arrange
        FormParseNode rootParseNode = new(TestUserForm);
        // Try to get an imaginary node value
        Abstractions.Serialization.IParseNode? imaginaryNode = rootParseNode.GetChildNode("imaginaryNode");
        // Assert
        Assert.Null(imaginaryNode);
    }

    [Fact]
    public void FormParseNode_GetBoolValue()
    {
        // Arrange
        string text = "true";
        FormParseNode parseNode = new(text);

        // Act
        bool? result = parseNode.GetBoolValue();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void FormParseNode_GetByteArrayValue()
    {
        // Arrange
        string text = "dGV4dA==";
        FormParseNode parseNode = new(text);

        // Act
        byte[]? result = parseNode.GetByteArrayValue();
        // Assert
        Assert.Equal([0x74, 0x65, 0x78, 0x74], result);
    }

    [Fact]
    public void FormParseNode_GetByteValue()
    {
        // Arrange
        string text = "1";
        FormParseNode parseNode = new(text);

        // Act
        byte? result = parseNode.GetByteValue();

        // Assert
        Assert.Equal((byte)1, result);
    }

    [Fact]
    public void FormParseNode_GetDateTimeOffsetValue()
    {
        // Arrange
        string text = "2021-11-30T12:24:36+03:00";
        FormParseNode parseNode = new(text);

        // Act
        DateTimeOffset? result = parseNode.GetDateTimeOffsetValue();

        // Assert
        Assert.Equal(new DateTimeOffset(2021, 11, 30, 12, 24, 36, TimeSpan.FromHours(3)), result);
    }

    [Fact]
    public void FormParseNode_GetDateValue()
    {
        // Arrange
        string text = "2021-11-30";
        FormParseNode parseNode = new(text);

        // Act
        Date? result = parseNode.GetDateValue();

        // Assert
        Assert.Equal(new Date(2021, 11, 30), result);
    }

    [Fact]
    public void FormParseNode_GetTimeSpanValue()
    {
        // Arrange
        string text = "P756DT4H6M8.01S";
        FormParseNode parseNode = new(text);

        // Act
        TimeSpan? result = parseNode.GetTimeSpanValue();

        // Assert
        Assert.Equal(new TimeSpan(756, 4, 6, 8, 10), result);
    }

    [Fact]
    public void FormParseNode_GetTimeValue()
    {
        // Arrange
        string text = "12:24:36";
        FormParseNode parseNode = new(text);

        // Act
        Time? result = parseNode.GetTimeValue();

        // Assert
        Assert.Equal(new Time(12, 24, 36), result);
    }

    [Fact]
    public void FormParseNode_GetGuidValue()
    {
        // Arrange
        string text = "f4b3b8f4-6f4d-4f1f-8f4d-8f4b3b8f4d1f";
        FormParseNode parseNode = new(text);

        // Act
        Guid? result = parseNode.GetGuidValue();

        // Assert
        Assert.Equal(new Guid("f4b3b8f4-6f4d-4f1f-8f4d-8f4b3b8f4d1f"), result);
    }

    [Fact]
    public void FormParseNode_GetIntValue()
    {
        // Arrange
        string text = "1";
        FormParseNode parseNode = new(text);

        // Act
        int? result = parseNode.GetIntValue();

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void FormParseNode_GetLongValue()
    {
        // Arrange
        string text = "1";
        FormParseNode parseNode = new(text);

        // Act
        long? result = parseNode.GetLongValue();

        // Assert
        Assert.Equal(1L, result);
    }

    [Fact]
    public void FormParseNode_GetSbyteValue()
    {
        // Arrange
        string text = "1";
        FormParseNode parseNode = new(text);

        // Act
        sbyte? result = parseNode.GetSbyteValue();

        // Assert
        Assert.Equal((sbyte)1, result);
    }

    [Fact]
    public void FormParseNode_GetStringValue()
    {
        // Arrange
        string text = "text";
        FormParseNode parseNode = new(text);

        // Act
        string result = parseNode.GetStringValue();

        // Assert
        Assert.Equal("text", result);
    }

    [Fact]
    public void FormParseNode_GetDecimalValue()
    {
        // Arrange
        string text = "1.1";
        FormParseNode parseNode = new(text);

        // Act
        decimal? result = parseNode.GetDecimalValue();

        // Assert
        Assert.Equal(1.1m, result);
    }

    [Fact]
    public void FormParseNode_GetDoubleValue()
    {
        // Arrange
        string text = "1.1";
        FormParseNode parseNode = new(text);

        // Act
        double? result = parseNode.GetDoubleValue();

        // Assert
        Assert.Equal(1.1, result);
    }

    [Fact]
    public void FormParseNode_GetFloatValue()
    {
        // Arrange
        string text = "1.1";
        FormParseNode parseNode = new(text);

        // Act
        float? result = parseNode.GetFloatValue();

        // Assert
        Assert.Equal(1.1f, result);
    }

    [Fact]
    public void FormParseNode_GetEnumFromInteger()
    {
        // Arange
        string text = "8";
        FormParseNode parseNode = new(text);

        // Act
        TestEnum? result = parseNode.GetEnumValue<TestEnum>();

        // Assert
        Assert.Equal(TestEnum.Eight, result);
    }

    [Fact]
    public void FormParseNode_GetEnumFromString()
    {
        // Arange
        string text = "Sixteen";
        FormParseNode parseNode = new(text);

        // Act
        TestEnum? result = parseNode.GetEnumValue<TestEnum>();

        // Assert
        Assert.Equal(TestEnum.Sixteen, result);
    }

    [Fact]
    public void FormParseNode_GetEnumsFromInteger()
    {
        // Arange
        string text = "8,16";
        FormParseNode parseNode = new(text);

        // Act
        IEnumerable<TestEnum?> result = parseNode.GetCollectionOfEnumValues<TestEnum>();

        // Assert
        Assert.Equal([TestEnum.Eight, TestEnum.Sixteen], result);
    }

    [Fact]
    public void FormParseNode_GetEnumsFromString()
    {
        // Arange
        string text = "Sixteen,Eight";
        FormParseNode parseNode = new(text);

        // Act
        IEnumerable<TestEnum?> result = parseNode.GetCollectionOfEnumValues<TestEnum>();

        // Assert
        Assert.Equal([TestEnum.Sixteen, TestEnum.Eight], result);
    }
}
