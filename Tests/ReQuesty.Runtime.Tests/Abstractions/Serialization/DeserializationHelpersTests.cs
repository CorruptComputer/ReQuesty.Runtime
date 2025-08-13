using ReQuesty.Runtime.Abstractions.Serialization;
using Moq;
using ReQuesty.Runtime.Tests.Abstractions.Mocks;

namespace ReQuesty.Runtime.Tests.Abstractions.Serialization;

public partial class DeserializationHelpersTests
{
    private const string _jsonContentType = "application/json";

    [Fact]
    public async Task DefensiveObjectAsync()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await ReQuestySerializer.DeserializeAsync<TestEntity>(null!, (Stream)null!, null!));
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await ReQuestySerializer.DeserializeAsync<TestEntity>(_jsonContentType, (Stream)null!, null!));
        using MemoryStream stream = new();
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await ReQuestySerializer.DeserializeAsync<TestEntity>(_jsonContentType, stream, null!));
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await ReQuestySerializer.DeserializeAsync<TestEntity>(_jsonContentType, "", null!));
    }
    [Fact]
    public async Task DefensiveObjectCollectionAsync()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await ReQuestySerializer.DeserializeCollectionAsync<TestEntity>(null!, (Stream)null!, null!, default));
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await ReQuestySerializer.DeserializeCollectionAsync<TestEntity>(_jsonContentType, (Stream)null!, null!));
        using MemoryStream stream = new();
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await ReQuestySerializer.DeserializeCollectionAsync<TestEntity>(_jsonContentType, stream, null!));
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await ReQuestySerializer.DeserializeCollectionAsync<TestEntity>(_jsonContentType, "", null!));
    }


    [Fact]
    public async Task DeserializesObjectWithoutReflectionAsync()
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

        TestEntity? result = await ReQuestySerializer.DeserializeAsync(_jsonContentType, strValue, TestEntity.CreateFromDiscriminatorValue);

        Assert.NotNull(result);
    }
    [Fact]
    public async Task DeserializesObjectWithReflectionAsync()
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

        TestEntity? result = await ReQuestySerializer.DeserializeAsync<TestEntity>(_jsonContentType, strValue);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task DeserializesCollectionOfObjectAsync()
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

        IEnumerable<TestEntity> result = await ReQuestySerializer.DeserializeCollectionAsync(_jsonContentType, strValue, TestEntity.CreateFromDiscriminatorValue);

        Assert.NotNull(result);
        Assert.Single(result);
    }
}
