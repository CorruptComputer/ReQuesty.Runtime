using ReQuesty.Runtime.Abstractions.Authentication;
using ReQuesty.Runtime.Abstractions.Serialization;
using Xunit;

namespace ReQuesty.Runtime.Tests
{
    public class BundleTests
    {
        [Fact]
        public void ThrowsArgumentNullExceptionOnNullAuthenticationProvider()
        {
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => new DefaultRequestAdapter(null!));
            Assert.Equal("authenticationProvider", exception.ParamName);
        }

        [Fact]
        public void SerializersAreRegisteredAsExpected()
        {
            // setup
            _ = new DefaultRequestAdapter(new AnonymousAuthenticationProvider());

            // validate
            int serializerCount = SerializationWriterFactoryRegistry.DefaultInstance.ContentTypeAssociatedFactories.Count;
            int deserializerCount = ParseNodeFactoryRegistry.DefaultInstance.ContentTypeAssociatedFactories.Count;

            // Debug output for this, tests pass locally but in CI there seems to be an extra serializer registered here.
            foreach (KeyValuePair<string, ISerializationWriterFactory> factory in SerializationWriterFactoryRegistry.DefaultInstance.ContentTypeAssociatedFactories)
            {
                Console.WriteLine($"Serializer: {factory.Key} -> {factory.Value.GetType().Name}");
            }

            Assert.Equal(4, serializerCount); // four serializers present
            Assert.Equal(3, deserializerCount);// three deserializers present

            ICollection<string> serializerKeys = SerializationWriterFactoryRegistry.DefaultInstance.ContentTypeAssociatedFactories.Keys;
            ICollection<string> deserializerKeys = ParseNodeFactoryRegistry.DefaultInstance.ContentTypeAssociatedFactories.Keys;

            Assert.Contains("application/json", serializerKeys);
            Assert.Contains("application/json", deserializerKeys);// Serializer and deserializer present for application/json

            Assert.Contains("text/plain", serializerKeys);
            Assert.Contains("text/plain", deserializerKeys);// Serializer and deserializer present for text/plain

            Assert.Contains("application/x-www-form-urlencoded", serializerKeys);
            Assert.Contains("application/x-www-form-urlencoded", deserializerKeys);// Serializer and deserializer present for application/x-www-form-urlencoded

            Assert.Contains("multipart/form-data", serializerKeys);// Serializer present for multipart/form-data
        }
    }
}
