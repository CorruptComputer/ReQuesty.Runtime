using ReQuesty.Runtime.Abstractions.Serialization;
using ReQuesty.Runtime.Serialization.Text;

namespace ReQuesty.Runtime.Tests.Serialization.Text;

public class TextSerializationWriterFactoryTests
{
    [Fact]
    public void TextSerializationWriterFactory_GetSerializationWriter()
    {
        TextSerializationWriterFactory factory = new();
        ISerializationWriter writer = factory.GetSerializationWriter("text/plain");
        Assert.NotNull(writer);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void TextSerializationWriterFactory_GetSerializationWriter_ThrowsOnNullContentType(string? contentType)
    {
        TextSerializationWriterFactory factory = new();
        Assert.Throws<ArgumentNullException>(() => factory.GetSerializationWriter(contentType!));
    }

    [Fact]
    public void TextSerializationWriterFactory_GetSerializationWriter_ThrowsOnInvalidContentType()
    {
        TextSerializationWriterFactory factory = new();
        Assert.Throws<ArgumentOutOfRangeException>(() => factory.GetSerializationWriter("application/json"));
    }
}
