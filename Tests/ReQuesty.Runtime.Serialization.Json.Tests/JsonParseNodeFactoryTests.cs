using System.Text;
using Xunit;

namespace ReQuesty.Runtime.Serialization.Json.Tests
{
    public class JsonParseNodeFactoryTests
    {
        private readonly JsonParseNodeFactory _jsonParseNodeFactory;
        private const string TestJsonString = "{\"key\":\"value\"}";

        public JsonParseNodeFactoryTests()
        {
            _jsonParseNodeFactory = new JsonParseNodeFactory();
        }

        [Fact]
        public async Task GetsWriterForJsonContentType()
        {
            using MemoryStream jsonStream = new(Encoding.UTF8.GetBytes(TestJsonString));
            Abstractions.Serialization.IParseNode jsonWriter = await _jsonParseNodeFactory.GetRootParseNodeAsync(_jsonParseNodeFactory.ValidContentType, jsonStream);

            // Assert
            Assert.NotNull(jsonWriter);
            Assert.IsAssignableFrom<JsonParseNode>(jsonWriter);
        }

        [Fact]
        public async Task ThrowsArgumentOutOfRangeExceptionForInvalidContentType()
        {
            string streamContentType = "application/octet-stream";
            using MemoryStream jsonStream = new(Encoding.UTF8.GetBytes(TestJsonString));
            ArgumentOutOfRangeException exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await _jsonParseNodeFactory.GetRootParseNodeAsync(streamContentType, jsonStream));

            // Assert
            Assert.NotNull(exception);
            Assert.Equal($"expected a {_jsonParseNodeFactory.ValidContentType} content type", exception.ParamName);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task ThrowsArgumentNullExceptionForNoContentType(string? contentType)
        {
            using MemoryStream jsonStream = new(Encoding.UTF8.GetBytes(TestJsonString));
            ArgumentNullException exception = await Assert.ThrowsAsync<ArgumentNullException>(async () => await _jsonParseNodeFactory.GetRootParseNodeAsync(contentType!, jsonStream));

            // Assert
            Assert.NotNull(exception);
            Assert.Equal("contentType", exception.ParamName);
        }
    }
}
