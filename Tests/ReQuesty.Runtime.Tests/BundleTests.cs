using ReQuesty.Runtime.Abstractions.Authentication;
using ReQuesty.Runtime.Abstractions.Serialization;

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
            // registers the serializers and deserializers with the DefaultInstance
            _ = new DefaultRequestAdapter(new AnonymousAuthenticationProvider());

            // validate
            //int serializerCount = SerializationWriterFactoryRegistry.DefaultInstance.ContentTypeAssociatedFactories.Count;
            //int deserializerCount = ParseNodeFactoryRegistry.DefaultInstance.ContentTypeAssociatedFactories.Count;

            // TODO: These other tests add things to the registry, so this count may be different depending on timing of the tests run:
            //   Tests/ReQuesty.Runtime.Tests/Abstractions/Serialization/SerializationHelpersTests.cs
            //   Tests/ReQuesty.Runtime.Tests/Abstractions/ApiClientBuilderTests.cs
            // So these two asserts can be skipped for now.
            // This wasn't an issue for Kiota before since these were not in the same project, so that static instance was not shared between those and this test.
            // I don't want to manage multiple nuget packages for this though, so too bad.
            //Assert.Equal(4, serializerCount);
            //Assert.Equal(3, deserializerCount);

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
