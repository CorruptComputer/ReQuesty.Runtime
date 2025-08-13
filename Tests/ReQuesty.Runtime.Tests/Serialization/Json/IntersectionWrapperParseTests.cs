using System.Text;
using ReQuesty.Runtime.Abstractions.Serialization;
using ReQuesty.Runtime.Serialization.Json;
using ReQuesty.Runtime.Tests.Serialization.Json.Mocks;

namespace ReQuesty.Runtime.Tests.Serialization.Json;

public class IntersectionWrapperParseTests
{
    private readonly JsonParseNodeFactory _parseNodeFactory = new();
    private readonly JsonSerializationWriterFactory _serializationWriterFactory = new();
    private const string contentType = "application/json";
    [Fact]
    public async Task ParsesIntersectionTypeComplexProperty1()
    {
        // Given
        using MemoryStream payload = new(Encoding.UTF8.GetBytes("{\"displayName\":\"McGill\",\"officeLocation\":\"Montreal\", \"id\": \"opaque\"}"));
        IParseNode parseNode = await _parseNodeFactory.GetRootParseNodeAsync(contentType, payload);

        // When
        IntersectionTypeMock result = parseNode.GetObjectValue<IntersectionTypeMock>(IntersectionTypeMock.CreateFromDiscriminator);

        // Then
        Assert.NotNull(result);
        Assert.NotNull(result.ComposedType1);
        Assert.NotNull(result.ComposedType2);
        Assert.Null(result.ComposedType3);
        Assert.Null(result.StringValue);
        Assert.Equal("opaque", result.ComposedType1.Id);
        Assert.Equal("McGill", result.ComposedType2.DisplayName);
    }
    [Fact]
    public async Task ParsesIntersectionTypeComplexProperty2()
    {
        // Given
        using MemoryStream payload = new(Encoding.UTF8.GetBytes("{\"displayName\":\"McGill\",\"officeLocation\":\"Montreal\", \"id\": 10}"));
        IParseNode parseNode = await _parseNodeFactory.GetRootParseNodeAsync(contentType, payload);

        // When
        IntersectionTypeMock result = parseNode.GetObjectValue<IntersectionTypeMock>(IntersectionTypeMock.CreateFromDiscriminator);

        // Then
        Assert.NotNull(result);
        Assert.NotNull(result.ComposedType1);
        Assert.NotNull(result.ComposedType2);
        Assert.Null(result.ComposedType3);
        Assert.Null(result.StringValue);
        Assert.Null(result.ComposedType1.Id);
        Assert.Null(result.ComposedType2.Id); // it's expected to be null since we have conflicting properties here and the parser will only try one to avoid having to brute its way through
        Assert.Equal("McGill", result.ComposedType2.DisplayName);
    }
    [Fact]
    public async Task ParsesIntersectionTypeComplexProperty3()
    {
        // Given
        using MemoryStream payload = new(Encoding.UTF8.GetBytes("[{\"@odata.type\":\"#microsoft.graph.TestEntity\",\"officeLocation\":\"Ottawa\", \"id\": \"11\"}, {\"@odata.type\":\"#microsoft.graph.TestEntity\",\"officeLocation\":\"Montreal\", \"id\": \"10\"}]"));
        IParseNode parseNode = await _parseNodeFactory.GetRootParseNodeAsync(contentType, payload);

        // When
        IntersectionTypeMock result = parseNode.GetObjectValue<IntersectionTypeMock>(IntersectionTypeMock.CreateFromDiscriminator);

        // Then
        Assert.NotNull(result);
        Assert.Null(result.ComposedType1);
        Assert.Null(result.ComposedType2);
        Assert.NotNull(result.ComposedType3);
        Assert.Null(result.StringValue);
        Assert.Equal(2, result.ComposedType3.Count);
        Assert.Equal("Ottawa", result.ComposedType3.First().OfficeLocation);
    }
    [Fact]
    public async Task ParsesIntersectionTypeStringValue()
    {
        // Given
        using MemoryStream payload = new(Encoding.UTF8.GetBytes("\"officeLocation\""));
        IParseNode parseNode = await _parseNodeFactory.GetRootParseNodeAsync(contentType, payload);

        // When
        IntersectionTypeMock result = parseNode.GetObjectValue<IntersectionTypeMock>(IntersectionTypeMock.CreateFromDiscriminator);

        // Then
        Assert.NotNull(result);
        Assert.Null(result.ComposedType2);
        Assert.Null(result.ComposedType1);
        Assert.Null(result.ComposedType3);
        Assert.Equal("officeLocation", result.StringValue);
    }
    [Fact]
    public void SerializesIntersectionTypeStringValue()
    {
        // Given
        using ISerializationWriter writer = _serializationWriterFactory.GetSerializationWriter(contentType);
        IntersectionTypeMock model = new()
        {
            StringValue = "officeLocation"
        };

        // When
        model.Serialize(writer);
        using Stream resultStream = writer.GetSerializedContent();
        using StreamReader streamReader = new(resultStream);
        string result = streamReader.ReadToEnd();

        // Then
        Assert.Equal("\"officeLocation\"", result);
    }
    [Fact]
    public void SerializesIntersectionTypeComplexProperty1()
    {
        // Given
        using ISerializationWriter writer = _serializationWriterFactory.GetSerializationWriter(contentType);
        IntersectionTypeMock model = new()
        {
            ComposedType1 = new()
            {
                Id = "opaque",
                OfficeLocation = "Montreal",
            },
            ComposedType2 = new()
            {
                DisplayName = "McGill",
            },
        };

        // When
        model.Serialize(writer);
        using Stream resultStream = writer.GetSerializedContent();
        using StreamReader streamReader = new(resultStream);
        string result = streamReader.ReadToEnd();

        // Then
        Assert.Equal("{\"id\":\"opaque\",\"officeLocation\":\"Montreal\",\"displayName\":\"McGill\"}", result);
    }
    [Fact]
    public void SerializesIntersectionTypeComplexProperty2()
    {
        // Given
        using ISerializationWriter writer = _serializationWriterFactory.GetSerializationWriter(contentType);
        IntersectionTypeMock model = new()
        {
            ComposedType2 = new()
            {
                DisplayName = "McGill",
                Id = 10,
            },
        };

        // When
        model.Serialize(writer);
        using Stream resultStream = writer.GetSerializedContent();
        using StreamReader streamReader = new(resultStream);
        string result = streamReader.ReadToEnd();

        // Then
        Assert.Equal("{\"displayName\":\"McGill\",\"id\":10}", result);
    }

    [Fact]
    public void SerializesIntersectionTypeComplexProperty3()
    {
        // Given
        using ISerializationWriter writer = _serializationWriterFactory.GetSerializationWriter(contentType);
        IntersectionTypeMock model = new()
        {
            ComposedType3 = [
                new() {
                    OfficeLocation = "Montreal",
                    Id = "10",
                },
                new() {
                    OfficeLocation = "Ottawa",
                    Id = "11",
                }
            ],
        };

        // When
        model.Serialize(writer);
        using Stream resultStream = writer.GetSerializedContent();
        using StreamReader streamReader = new(resultStream);
        string result = streamReader.ReadToEnd();

        // Then
        Assert.Equal("[{\"id\":\"10\",\"officeLocation\":\"Montreal\"},{\"id\":\"11\",\"officeLocation\":\"Ottawa\"}]", result);
    }
}