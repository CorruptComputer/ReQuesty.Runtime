using ReQuesty.Runtime.Abstractions.Serialization;
using ReQuesty.Runtime.Serialization.Json;

namespace ReQuesty.Runtime.Tests.Serialization.Json;

public class JsonSerializationWriterFactoryTests
{
    private readonly JsonSerializationWriterFactory _jsonSerializationFactory;

    public JsonSerializationWriterFactoryTests()
    {
        _jsonSerializationFactory = new JsonSerializationWriterFactory();
    }

    [Fact]
    public void GetsWriterForJsonContentType()
    {
        ISerializationWriter jsonWriter = _jsonSerializationFactory.GetSerializationWriter(_jsonSerializationFactory.ValidContentType);

        // Assert
        Assert.NotNull(jsonWriter);
        Assert.IsAssignableFrom<JsonSerializationWriter>(jsonWriter);
    }

    [Fact]
    public void ThrowsArgumentOutOfRangeExceptionForInvalidContentType()
    {
        string streamContentType = "application/octet-stream";
        ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(() => _jsonSerializationFactory.GetSerializationWriter(streamContentType));

        // Assert
        Assert.NotNull(exception);
        Assert.Equal($"expected a {_jsonSerializationFactory.ValidContentType} content type", exception.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ThrowsArgumentNullExceptionForNoContentType(string? contentType)
    {
        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => _jsonSerializationFactory.GetSerializationWriter(contentType!));

        // Assert
        Assert.NotNull(exception);
        Assert.Equal("contentType", exception.ParamName);
    }
}
