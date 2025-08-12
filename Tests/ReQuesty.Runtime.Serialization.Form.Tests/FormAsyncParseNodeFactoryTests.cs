using System.Text;

namespace ReQuesty.Runtime.Serialization.Form.Tests;

public class FormAsyncParseNodeFactoryTests
{
    private readonly FormParseNodeFactory _formParseNodeFactory = new();
    private const string TestJsonString = "key=value";
    [Fact]
    public async Task GetsWriterForFormContentType()
    {
        using MemoryStream formStream = new(Encoding.UTF8.GetBytes(TestJsonString));
        Abstractions.Serialization.IParseNode formParseNode = await _formParseNodeFactory.GetRootParseNodeAsync(_formParseNodeFactory.ValidContentType, formStream);

        // Assert
        Assert.NotNull(formParseNode);
        Assert.IsAssignableFrom<FormParseNode>(formParseNode);
    }
    [Fact]
    public async Task ThrowsArgumentOutOfRangeExceptionForInvalidContentType()
    {
        string streamContentType = "application/octet-stream";
        using MemoryStream jsonStream = new(Encoding.UTF8.GetBytes(TestJsonString));
        ArgumentOutOfRangeException exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            async () => await _formParseNodeFactory.GetRootParseNodeAsync(streamContentType, jsonStream));

        // Assert
        Assert.NotNull(exception);
        Assert.Equal($"expected a {_formParseNodeFactory.ValidContentType} content type", exception.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task ThrowsArgumentNullExceptionForNoContentType(string? contentType)
    {
        using MemoryStream jsonStream = new(Encoding.UTF8.GetBytes(TestJsonString));
        ArgumentNullException exception = await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await _formParseNodeFactory.GetRootParseNodeAsync(contentType!, jsonStream));

        // Assert
        Assert.NotNull(exception);
        Assert.Equal("contentType", exception.ParamName);
    }
}