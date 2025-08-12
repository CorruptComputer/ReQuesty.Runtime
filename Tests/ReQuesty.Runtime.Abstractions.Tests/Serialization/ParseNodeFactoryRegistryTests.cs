using System.Text;
using ReQuesty.Runtime.Abstractions.Serialization;
using Moq;
using Xunit;

namespace ReQuesty.Runtime.Abstractions.Tests.Serialization
{
    public class ParseNodeFactoryRegistryTests
    {
        private readonly ParseNodeFactoryRegistry _parseNodeFactoryRegistry;
        public ParseNodeFactoryRegistryTests()
        {
            _parseNodeFactoryRegistry = new ParseNodeFactoryRegistry();
        }

        [Fact]
        public void ParseNodeFactoryRegistryDoesNotStickToOneContentType()
        {
            // Act and Assert
            Assert.Throws<InvalidOperationException>(() => _parseNodeFactoryRegistry.ValidContentType);
        }

        [Fact]
        public async Task ReturnsExpectedRootNodeForRegisteredContentTypeAsync()
        {
            // Arrange
            string streamContentType = "application/octet-stream";
            using MemoryStream testStream = new(Encoding.UTF8.GetBytes("test input"));
            Mock<IAsyncParseNodeFactory> mockParseNodeFactory = new();
            Mock<IParseNode> mockParseNode = new();
            mockParseNodeFactory.Setup(parseNodeFactory => parseNodeFactory.GetRootParseNodeAsync(streamContentType, It.IsAny<Stream>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(mockParseNode.Object));
            _parseNodeFactoryRegistry.ContentTypeAssociatedFactories.TryAdd(streamContentType, mockParseNodeFactory.Object);
            // Act
            IParseNode rootParseNode = await _parseNodeFactoryRegistry.GetRootParseNodeAsync(streamContentType, testStream);
            // Assert
            Assert.NotNull(rootParseNode);
            Assert.Equal(mockParseNode.Object, rootParseNode);
        }
        [Fact]
        public async Task ReturnsExpectedRootNodeForVendorSpecificContentTypeAsync()
        {
            // Arrange
            string applicationJsonContentType = "application/json";
            using MemoryStream testStream = new(Encoding.UTF8.GetBytes("{\"test\": \"input\"}"));
            Mock<IAsyncParseNodeFactory> mockParseNodeFactory = new();
            Mock<IParseNode> mockParseNode = new();
            mockParseNodeFactory.Setup(parseNodeFactory => parseNodeFactory.GetRootParseNodeAsync(applicationJsonContentType, It.IsAny<Stream>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(mockParseNode.Object));
            _parseNodeFactoryRegistry.ContentTypeAssociatedFactories.TryAdd(applicationJsonContentType, mockParseNodeFactory.Object);
            // Act
            IParseNode rootParseNode = await _parseNodeFactoryRegistry.GetRootParseNodeAsync("application/vnd+json", testStream);
            // Assert
            Assert.NotNull(rootParseNode);
            Assert.Equal(mockParseNode.Object, rootParseNode);
        }

        [Fact]
        public async Task ThrowsInvalidOperationExceptionForUnregisteredContentTypeAsync()
        {
            // Arrange
            string streamContentType = "application/octet-stream";
            using MemoryStream testStream = new(Encoding.UTF8.GetBytes("test input"));
            // Act
            InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await _parseNodeFactoryRegistry.GetRootParseNodeAsync(streamContentType, testStream));
            // Assert
            Assert.NotNull(exception);
            Assert.Equal($"Content type {streamContentType} does not have a factory registered to be parsed", exception.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task ThrowsArgumentNullExceptionForNoContentTypeAsync(string? contentType)
        {
            // Arrange
            using MemoryStream testStream = new(Encoding.UTF8.GetBytes("test input"));
            // Act
            ArgumentNullException exception = await Assert.ThrowsAsync<ArgumentNullException>(async () => await _parseNodeFactoryRegistry.GetRootParseNodeAsync(contentType!, testStream));
            // Assert
            Assert.NotNull(exception);
            Assert.Equal("contentType", exception.ParamName);
        }
    }
}
