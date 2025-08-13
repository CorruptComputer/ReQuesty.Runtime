using ReQuesty.Runtime.Abstractions.Serialization;
using ReQuesty.Runtime.Abstractions.Store;
using ReQuesty.Runtime.Serialization.Json;
using ReQuesty.Runtime.Tests.Serialization.Json.Mocks;

namespace ReQuesty.Runtime.Tests.Serialization.Json;

public class IParsableExtensionsTests
{
    private const string _jsonContentType = "application/json";
    private readonly SerializationWriterFactoryRegistry _serializationWriterFactoryRegistry;

    public IParsableExtensionsTests()
    {
        _serializationWriterFactoryRegistry = new SerializationWriterFactoryRegistry();
        _serializationWriterFactoryRegistry.ContentTypeAssociatedFactories.TryAdd(_jsonContentType, new BackingStoreSerializationWriterProxyFactory(new JsonSerializationWriterFactory()));
    }

    [Theory]
    [InlineData(null)]
    [InlineData(true)]
    [InlineData(false)]
    public void GetSerializationWriter_RetunsJsonSerializationWriter(bool? serializeOnlyChangedValues)
    {
        // Arrange

        // Act
        using ISerializationWriter writer = serializeOnlyChangedValues.HasValue
          ? _serializationWriterFactoryRegistry.GetSerializationWriter(_jsonContentType, serializeOnlyChangedValues.Value)
          : _serializationWriterFactoryRegistry.GetSerializationWriter(_jsonContentType);

        // Assert
        Assert.NotNull(writer);
        Assert.IsType<JsonSerializationWriter>(writer);
    }

    [Fact]
    public void GetSerializationWriterSerializedChangedTrue_RetunsEmptyJson()
    {
        // Arrange
        BackedTestEntity testUser = new() { Id = "1", Name = "testUser" };
        testUser.BackingStore.InitializationCompleted = true;
        using ISerializationWriter writer = _serializationWriterFactoryRegistry.GetSerializationWriter(_jsonContentType, true);

        // Act
        writer.WriteObjectValue(null, testUser);
        using Stream stream = writer.GetSerializedContent();
        string serializedContent = GetStringFromStream(stream);

        // Assert
        Assert.NotNull(serializedContent);
        Assert.Equal("{}", serializedContent);
    }

    [Fact]
    public void GetSerializationWriterSerializedChangedTrue_ChangedName_ReturnsJustName()
    {
        // Arrange
        BackedTestEntity testUser = new() { Id = "1", Name = "testUser" };
        testUser.BackingStore.InitializationCompleted = true;
        testUser.Name = "Stephan";
        using ISerializationWriter writer = _serializationWriterFactoryRegistry.GetSerializationWriter(_jsonContentType, true);

        // Act
        writer.WriteObjectValue(null, testUser);
        using Stream stream = writer.GetSerializedContent();
        string serializedContent = GetStringFromStream(stream);

        // Assert
        Assert.NotNull(serializedContent);
        Assert.Equal("{\"name\":\"Stephan\"}", serializedContent);
    }

    [Fact]
    public void GetSerializationWriterSerializedChangedFalse_SerializesEntireObject()
    {
        // Arrange
        BackedTestEntity testUser = new() { Id = "1", Name = "testUser" };
        testUser.BackingStore.InitializationCompleted = true;
        using ISerializationWriter writer = _serializationWriterFactoryRegistry.GetSerializationWriter(_jsonContentType, false);

        // Act
        writer.WriteObjectValue(null, testUser);
        using Stream stream = writer.GetSerializedContent();
        string serializedContent = GetStringFromStream(stream);

        // Assert
        Assert.NotNull(serializedContent);
        Assert.Equal("{\"id\":\"1\",\"name\":\"testUser\"}", serializedContent);
    }

    private static string GetStringFromStream(Stream stream)
    {
        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }
}
