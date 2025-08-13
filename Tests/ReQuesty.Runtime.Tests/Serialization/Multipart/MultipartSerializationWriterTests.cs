using ReQuesty.Runtime.Abstractions;
using ReQuesty.Runtime.Abstractions.Serialization;
using ReQuesty.Runtime.Serialization.Json;
using Moq;
using ReQuesty.Runtime.Tests.Serialization.Multipart.Mocks;
using ReQuesty.Runtime.Serialization.Multipart;

namespace ReQuesty.Runtime.Tests.Serialization.Multipart;

public class MultipartSerializationWriterTests
{
    [Fact]
    public void ThrowsOnParsable()
    {
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
                {"businessPhones", new List<string>() {"+1 412 555 0109"}}, // write collection of primitives value
                {"endDateTime", new DateTime(2023,03,14,0,0,0,DateTimeKind.Utc) }, // ensure the DateTime doesn't crash
                {"manager", new TestEntity{Id = "48d31887-5fad-4d73-a9f5-3c356e68a038"}}, // write nested object value
            }
        };
        using MultipartSerializationWriter mpSerializerWriter = new();
        // Act
        Assert.Throws<InvalidOperationException>(() => mpSerializerWriter.WriteObjectValue(string.Empty, testEntity));
    }
    [Fact]
    public void WritesStringValue()
    {
        using MultipartSerializationWriter mpSerializerWriter = new();
        mpSerializerWriter.WriteStringValue("key", "value");
        using Stream stream = mpSerializerWriter.GetSerializedContent();
        using StreamReader reader = new(stream);
        string content = reader.ReadToEnd();
        Assert.Equal("key: value\r\n", content);
    }
    [Fact]
    public void WriteByteArrayValue()
    {
        using MultipartSerializationWriter mpSerializerWriter = new();
        mpSerializerWriter.WriteByteArrayValue("key", new byte[] { 1, 2, 3 });
        using Stream stream = mpSerializerWriter.GetSerializedContent();
        using StreamReader reader = new(stream);
        string content = reader.ReadToEnd();
        Assert.Equal("\u0001\u0002\u0003", content);
    }
    [Fact]
    public void WritesAStructuredObject()
    {
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
                {"businessPhones", new List<string>() {"+1 412 555 0109"}}, // write collection of primitives value
                {"endDateTime", new DateTime(2023,03,14,0,0,0,DateTimeKind.Utc) }, // ensure the DateTime doesn't crash
                {"manager", new TestEntity{Id = "48d31887-5fad-4d73-a9f5-3c356e68a038"}}, // write nested object value
            }
        };
        Mock<IRequestAdapter> mockRequestAdapter = new();
        mockRequestAdapter.SetupGet(x => x.SerializationWriterFactory).Returns(new JsonSerializationWriterFactory());
        MultipartBody mpBody = new()
        {
            RequestAdapter = mockRequestAdapter.Object
        };
        mpBody.AddOrReplacePart("testEntity", "application/json", testEntity);
        mpBody.AddOrReplacePart("image", "application/octet-stream", new byte[] { 1, 2, 3 });

        using MultipartSerializationWriter mpSerializerWriter = new();
        // Act
        mpSerializerWriter.WriteObjectValue(string.Empty, mpBody);
        using Stream stream = mpSerializerWriter.GetSerializedContent();
        using StreamReader reader = new(stream);
        string content = reader.ReadToEnd();
        Assert.Equal("--" + mpBody.Boundary + "\r\nContent-Type: application/json\r\nContent-Disposition: form-data; name=\"testEntity\"\r\n\r\n{\"id\":\"48d31887-5fad-4d73-a9f5-3c356e68a038\",\"workDuration\":\"PT1H\",\"birthDay\":\"2017-09-04\",\"startWorkTime\":\"08:00:00\",\"mobilePhone\":null,\"accountEnabled\":false,\"jobTitle\":\"Author\",\"createdDateTime\":\"0001-01-01T00:00:00+00:00\",\"businessPhones\":[\"\\u002B1 412 555 0109\"],\"endDateTime\":\"2023-03-14T00:00:00+00:00\",\"manager\":{\"id\":\"48d31887-5fad-4d73-a9f5-3c356e68a038\"}}\r\n--" + mpBody.Boundary + "\r\nContent-Type: application/octet-stream\r\nContent-Disposition: form-data; name=\"image\"\r\n\r\n\u0001\u0002\u0003\r\n--" + mpBody.Boundary + "--\r\n", content);
    }
    [Fact]
    public void WritesAStructuredObjectInverted()
    {
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
                {"businessPhones", new List<string>() {"+1 412 555 0109"}}, // write collection of primitives value
                {"endDateTime", new DateTime(2023,03,14,0,0,0,DateTimeKind.Utc) }, // ensure the DateTime doesn't crash
                {"manager", new TestEntity{Id = "48d31887-5fad-4d73-a9f5-3c356e68a038"}}, // write nested object value
            }
        };
        Mock<IRequestAdapter> mockRequestAdapter = new();
        mockRequestAdapter.SetupGet(x => x.SerializationWriterFactory).Returns(new JsonSerializationWriterFactory());
        MultipartBody mpBody = new()
        {
            RequestAdapter = mockRequestAdapter.Object
        };
        mpBody.AddOrReplacePart("image", "application/octet-stream", new byte[] { 1, 2, 3 });
        mpBody.AddOrReplacePart("testEntity", "application/json", testEntity);

        using MultipartSerializationWriter mpSerializerWriter = new();
        // Act
        mpSerializerWriter.WriteObjectValue(string.Empty, mpBody);
        using Stream stream = mpSerializerWriter.GetSerializedContent();
        using StreamReader reader = new(stream);
        string content = reader.ReadToEnd();
        Assert.Equal("--" + mpBody.Boundary + "\r\nContent-Type: application/octet-stream\r\nContent-Disposition: form-data; name=\"image\"\r\n\r\n\u0001\u0002\u0003\r\n--" + mpBody.Boundary + "\r\nContent-Type: application/json\r\nContent-Disposition: form-data; name=\"testEntity\"\r\n\r\n{\"id\":\"48d31887-5fad-4d73-a9f5-3c356e68a038\",\"workDuration\":\"PT1H\",\"birthDay\":\"2017-09-04\",\"startWorkTime\":\"08:00:00\",\"mobilePhone\":null,\"accountEnabled\":false,\"jobTitle\":\"Author\",\"createdDateTime\":\"0001-01-01T00:00:00+00:00\",\"businessPhones\":[\"\\u002B1 412 555 0109\"],\"endDateTime\":\"2023-03-14T00:00:00+00:00\",\"manager\":{\"id\":\"48d31887-5fad-4d73-a9f5-3c356e68a038\"}}\r\n--" + mpBody.Boundary + "--\r\n", content);
    }
}