using Xunit;

namespace ReQuesty.Runtime.Serialization.Text.Tests
{
    public class TextParseNodeFactoryTests
    {
        [Fact]
        public void GetRootParseNode_ThrowsArgumentNullException_WhenContentTypeIsNull()
        {
            // Arrange
            TextParseNodeFactory factory = new();
            string? contentType = null;
            using MemoryStream content = new();

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => factory.GetRootParseNode(contentType!, content));
        }

        [Fact]
        public void GetRootParseNode_ThrowsArgumentOutOfRangeException_WhenContentTypeIsInvalid()
        {
            // Arrange
            TextParseNodeFactory factory = new();
            string contentType = "application/json";
            using MemoryStream content = new();

            // Act and Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => factory.GetRootParseNode(contentType, content));
        }

        [Fact]
        public void GetRootParseNode_ThrowsArgumentNullException_WhenContentIsNull()
        {
            // Arrange
            TextParseNodeFactory factory = new();
            string contentType = "text/plain";
            Stream? content = null;

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => factory.GetRootParseNode(contentType, content!));
        }

        [Fact]
        public async Task GetRootParseNodeAsync_ThrowsArgumentNullException_WhenContentTypeIsNull()
        {
            // Arrange
            TextParseNodeFactory factory = new();
            string? contentType = null;
            using MemoryStream content = new();

            // Act and Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => factory.GetRootParseNodeAsync(contentType!, content));
        }

        [Fact]
        public async Task GetRootParseNodeAsync_ThrowsArgumentOutOfRangeException_WhenContentTypeIsInvalid()
        {
            // Arrange
            TextParseNodeFactory factory = new();
            string contentType = "application/json";
            using MemoryStream content = new();

            // Act and Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => factory.GetRootParseNodeAsync(contentType, content));
        }

        [Fact]
        public async Task GetRootParseNodeAsync_ThrowsArgumentNullException_WhenContentIsNull()
        {
            // Arrange
            TextParseNodeFactory factory = new();
            string contentType = "text/plain";
            Stream? content = null;

            // Act and Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => factory.GetRootParseNodeAsync(contentType, content!));
        }
    }
}
