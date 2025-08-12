using System.Text;

namespace ReQuesty.Runtime.Serialization.Form.Tests;

public class FormParseNodeFactoryTests
{
    private readonly FormParseNodeFactory _formParseNodeFactory = new();
    private const string TestJsonString = "key=value";
    [Fact]
    public void GetsWriterForFormContentType()
    {
        using MemoryStream formStream = new(Encoding.UTF8.GetBytes(TestJsonString));
        Abstractions.Serialization.IParseNode formParseNode = _formParseNodeFactory.GetRootParseNode(_formParseNodeFactory.ValidContentType, formStream);

        // Assert
        Assert.NotNull(formParseNode);
        Assert.IsAssignableFrom<FormParseNode>(formParseNode);
    }
    [Fact]
    public void ThrowsArgumentOutOfRangeExceptionForInvalidContentType()
    {
        string streamContentType = "application/octet-stream";
        using MemoryStream jsonStream = new(Encoding.UTF8.GetBytes(TestJsonString));
        ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(() => _formParseNodeFactory.GetRootParseNode(streamContentType, jsonStream));

        // Assert
        Assert.NotNull(exception);
        Assert.Equal($"expected a {_formParseNodeFactory.ValidContentType} content type", exception.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ThrowsArgumentNullExceptionForNoContentType(string? contentType)
    {
        using MemoryStream jsonStream = new(Encoding.UTF8.GetBytes(TestJsonString));
        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => _formParseNodeFactory.GetRootParseNode(contentType!, jsonStream));

        // Assert
        Assert.NotNull(exception);
        Assert.Equal("contentType", exception.ParamName);
    }
}