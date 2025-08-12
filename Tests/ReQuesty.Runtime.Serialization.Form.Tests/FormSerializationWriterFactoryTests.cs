namespace ReQuesty.Runtime.Serialization.Form.Tests;
public class FormSerializationWriterFactoryTests
{
    private readonly FormSerializationWriterFactory _formSerializationFactory = new();

    [Fact]
    public void GetsWriterForFormContentType()
    {
        Abstractions.Serialization.ISerializationWriter formWriter = _formSerializationFactory.GetSerializationWriter(_formSerializationFactory.ValidContentType);

        // Assert
        Assert.NotNull(formWriter);
        Assert.IsAssignableFrom<FormSerializationWriter>(formWriter);
    }

    [Fact]
    public void ThrowsArgumentOutOfRangeExceptionForInvalidContentType()
    {
        string streamContentType = "application/octet-stream";
        ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(() => _formSerializationFactory.GetSerializationWriter(streamContentType));

        // Assert
        Assert.NotNull(exception);
        Assert.Equal($"expected a {_formSerializationFactory.ValidContentType} content type", exception.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ThrowsArgumentNullExceptionForNoContentType(string? contentType)
    {
        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => _formSerializationFactory.GetSerializationWriter(contentType!));

        // Assert
        Assert.NotNull(exception);
        Assert.Equal("contentType", exception.ParamName);
    }
}
