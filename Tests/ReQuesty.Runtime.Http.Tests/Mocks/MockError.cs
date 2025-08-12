using ReQuesty.Runtime.Abstractions;
using ReQuesty.Runtime.Abstractions.Serialization;

namespace ReQuesty.Runtime.Http.Tests.Mocks;

public class MockError(string message) : ApiException(message), IParsable
{
    public IDictionary<string, Action<IParseNode>> GetFieldDeserializers()
    {
        return new Dictionary<string, Action<IParseNode>>();
    }

    public void Serialize(ISerializationWriter writer)
    {
    }
}

