using System.Text;
using ReQuesty.Runtime.Abstractions.Serialization;
using Moq;

namespace ReQuesty.Runtime.Tests.Abstractions.Serialization;

public class SerializationWriterFactoryRegistryTests
{
    private readonly SerializationWriterFactoryRegistry _serializationWriterFactoryRegistry;
    public SerializationWriterFactoryRegistryTests()
    {
        _serializationWriterFactoryRegistry = new SerializationWriterFactoryRegistry();
    }

    [Fact]
    public void ParseNodeFactoryRegistryDoesNotStickToOneContentType()
    {
        // Act and Assert
        Assert.Throws<InvalidOperationException>(() => _serializationWriterFactoryRegistry.ValidContentType);
    }

    [Fact]
    public void ReturnsExpectedRootNodeForRegisteredContentType()
    {
        // Arrange
        string streamContentType = "application/octet-stream";
        using MemoryStream testStream = new(Encoding.UTF8.GetBytes("test input"));
        Mock<ISerializationWriterFactory> mockSerializationWriterFactory = new();
        Mock<ISerializationWriter> mockSerializationWriter = new();
        mockSerializationWriterFactory.Setup(serializationWriterFactory => serializationWriterFactory.GetSerializationWriter(streamContentType)).Returns(mockSerializationWriter.Object);
        _serializationWriterFactoryRegistry.ContentTypeAssociatedFactories.TryAdd(streamContentType, mockSerializationWriterFactory.Object);
        // Act
        ISerializationWriter serializationWriter = _serializationWriterFactoryRegistry.GetSerializationWriter(streamContentType);
        // Assert
        Assert.NotNull(serializationWriter);
        Assert.Equal(mockSerializationWriter.Object, serializationWriter);
    }
    [Fact]
    public void ReturnsExpectedSerializationWriterForVendorSpecificContentTyp()
    {
        // Arrange
        string applicationJsonContentType = "application/json";
        Mock<ISerializationWriterFactory> mockSerializationWriterFactory = new();
        Mock<ISerializationWriter> mockSerializationWriter = new();
        mockSerializationWriterFactory.Setup(serializationWriterFactory => serializationWriterFactory.GetSerializationWriter(applicationJsonContentType)).Returns(mockSerializationWriter.Object);
        _serializationWriterFactoryRegistry.ContentTypeAssociatedFactories.TryAdd(applicationJsonContentType, mockSerializationWriterFactory.Object);
        // Act
        ISerializationWriter serializationWriter = _serializationWriterFactoryRegistry.GetSerializationWriter("application/vnd+json");
        // Assert
        Assert.NotNull(serializationWriter);
        Assert.Equal(mockSerializationWriter.Object, serializationWriter);
    }

    [Fact]
    public void ThrowsInvalidOperationExceptionForUnregisteredContentType()
    {
        // Arrange
        string streamContentType = "application/octet-stream";
        // Act
        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => _serializationWriterFactoryRegistry.GetSerializationWriter(streamContentType));
        // Assert
        Assert.NotNull(exception);
        Assert.Equal($"Content type {streamContentType} does not have a factory registered to be parsed", exception.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ThrowsArgumentNullExceptionForNoContentType(string? contentType)
    {
        // Act
        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => _serializationWriterFactoryRegistry.GetSerializationWriter(contentType!));
        // Assert
        Assert.NotNull(exception);
        Assert.Equal("contentType", exception.ParamName);
    }
}
