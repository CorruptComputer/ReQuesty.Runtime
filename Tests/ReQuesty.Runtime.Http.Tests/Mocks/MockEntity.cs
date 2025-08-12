using ReQuesty.Runtime.Abstractions.Serialization;

namespace ReQuesty.Runtime.Http.Tests.Mocks;

public class MockEntity : IParsable
{
    public IDictionary<string, Action<IParseNode>> GetFieldDeserializers()
    {
        return new Dictionary<string, Action<IParseNode>>();
    }

    public void Serialize(ISerializationWriter writer)
    {

    }
    public static MockEntity Factory(IParseNode parseNode) => new();
}
