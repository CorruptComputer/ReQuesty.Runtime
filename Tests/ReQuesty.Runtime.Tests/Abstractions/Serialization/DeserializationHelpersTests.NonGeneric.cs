using ReQuesty.Runtime.Abstractions.Serialization;
using Moq;
using ReQuesty.Runtime.Tests.Abstractions.Mocks;

namespace ReQuesty.Runtime.Tests.Abstractions.Serialization;

public partial class DeserializationHelpersTests
{

    [Fact]
    public async Task DeserializesObjectUntypedWithoutReflectionAsync()
    {
        string strValue = "{'id':'123'}";
        Mock<IParseNode> mockParseNode = new();
        mockParseNode.Setup(x => x.GetObjectValue(It.IsAny<ParsableFactory<TestEntity>>())).Returns(new TestEntity()
        {
            Id = "123"
        });
        Mock<IAsyncParseNodeFactory> mockJsonParseNodeFactory = new();
        mockJsonParseNodeFactory.Setup(x => x.GetRootParseNodeAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(mockParseNode.Object));
        mockJsonParseNodeFactory.Setup(x => x.ValidContentType).Returns(_jsonContentType);
        ParseNodeFactoryRegistry.DefaultInstance.ContentTypeAssociatedFactories[_jsonContentType] = mockJsonParseNodeFactory.Object;

        TestEntity? result = (TestEntity?)await ReQuestySerializer.DeserializeAsync(typeof(TestEntity), _jsonContentType, strValue);

        Assert.NotNull(result);
        Assert.Equal("123", result.Id);
    }

    [Fact]
    public async Task DeserializesCollectionOfObjectUntypedAsync()
    {
        string strValue = "{'id':'123'}";
        Mock<IParseNode> mockParseNode = new();
        mockParseNode.Setup(x => x.GetCollectionOfObjectValues(It.IsAny<ParsableFactory<TestEntity>>())).Returns(new List<TestEntity> {
            new()
            {
                Id = "123"
            }
        });
        Mock<IAsyncParseNodeFactory> mockJsonParseNodeFactory = new();
        mockJsonParseNodeFactory.Setup(x => x.GetRootParseNodeAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(mockParseNode.Object));
        mockJsonParseNodeFactory.Setup(x => x.ValidContentType).Returns(_jsonContentType);
        ParseNodeFactoryRegistry.DefaultInstance.ContentTypeAssociatedFactories[_jsonContentType] = mockJsonParseNodeFactory.Object;

        IEnumerable<IParsable> result = await ReQuestySerializer.DeserializeCollectionAsync(typeof(TestEntity), _jsonContentType, strValue);

        Assert.NotNull(result);
        Assert.Single(result);
        TestEntity? first = result.First() as TestEntity;
        Assert.NotNull(first);
        Assert.Equal("123", first.Id);
    }
}
