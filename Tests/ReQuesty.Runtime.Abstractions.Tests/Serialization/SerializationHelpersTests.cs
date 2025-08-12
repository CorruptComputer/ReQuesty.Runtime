using System.Text;
using ReQuesty.Runtime.Abstractions.Serialization;
using ReQuesty.Runtime.Abstractions.Tests.Mocks;
using Moq;
using Xunit;

namespace ReQuesty.Runtime.Abstractions.Tests.Serialization;

public class SerializationHelpersTests
{
    private const string _jsonContentType = "application/json";
    [Fact]
    public void DefensiveObject()
    {
        Assert.Throws<ArgumentNullException>(() => ReQuestySerializer.SerializeAsStream(null!, (TestEntity)null!));
        Assert.Throws<ArgumentNullException>(() => ReQuestySerializer.SerializeAsStream(_jsonContentType, (TestEntity)null!));
    }
    [Fact]
    public void DefensiveObjectCollection()
    {
        Assert.Throws<ArgumentNullException>(() => ReQuestySerializer.SerializeAsStream(null!, (IEnumerable<TestEntity>)null!));
        Assert.Throws<ArgumentNullException>(() => ReQuestySerializer.SerializeAsStream(_jsonContentType, (IEnumerable<TestEntity>)null!));
    }
    [Fact]
    public async Task SerializesObject()
    {
        Mock<ISerializationWriter> mockSerializationWriter = new();
        mockSerializationWriter.Setup(x => x.GetSerializedContent()).Returns(new MemoryStream(UTF8Encoding.UTF8.GetBytes("{'id':'123'}")));
        Mock<ISerializationWriterFactory> mockSerializationWriterFactory = new();
        mockSerializationWriterFactory.Setup(x => x.GetSerializationWriter(It.IsAny<string>())).Returns(mockSerializationWriter.Object);
        SerializationWriterFactoryRegistry.DefaultInstance.ContentTypeAssociatedFactories[_jsonContentType] = mockSerializationWriterFactory.Object;

        string result = await ReQuestySerializer.SerializeAsStringAsync(_jsonContentType, new TestEntity()
        {
            Id = "123"
        });

        Assert.Equal("{'id':'123'}", result);

        mockSerializationWriterFactory.Verify(x => x.GetSerializationWriter(It.IsAny<string>()), Times.Once);
        mockSerializationWriter.Verify(x => x.WriteObjectValue(It.IsAny<string>(), It.IsAny<TestEntity>()), Times.Once);
        mockSerializationWriter.Verify(x => x.GetSerializedContent(), Times.Once);
    }
    [Fact]
    public async Task SerializesObjectCollection()
    {
        Mock<ISerializationWriter> mockSerializationWriter = new();
        mockSerializationWriter.Setup(x => x.GetSerializedContent()).Returns(new MemoryStream(UTF8Encoding.UTF8.GetBytes("[{'id':'123'}]")));
        Mock<ISerializationWriterFactory> mockSerializationWriterFactory = new();
        mockSerializationWriterFactory.Setup(x => x.GetSerializationWriter(It.IsAny<string>())).Returns(mockSerializationWriter.Object);
        SerializationWriterFactoryRegistry.DefaultInstance.ContentTypeAssociatedFactories[_jsonContentType] = mockSerializationWriterFactory.Object;

        string result = await ReQuestySerializer.SerializeAsStringAsync(_jsonContentType, new List<TestEntity> {
            new()
            {
                Id = "123"
            }
        });

        Assert.Equal("[{'id':'123'}]", result);

        mockSerializationWriterFactory.Verify(x => x.GetSerializationWriter(It.IsAny<string>()), Times.Once);
        mockSerializationWriter.Verify(x => x.WriteCollectionOfObjectValues(It.IsAny<string>(), It.IsAny<IEnumerable<TestEntity>>()), Times.Once);
        mockSerializationWriter.Verify(x => x.GetSerializedContent(), Times.Once);
    }
}