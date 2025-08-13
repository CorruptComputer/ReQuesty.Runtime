using ReQuesty.Runtime.Abstractions.Serialization;

namespace ReQuesty.Runtime.Tests.Serialization.Text.Mocks;

public class TestEntity : IParsable
{
    public IDictionary<string, Action<IParseNode>> GetFieldDeserializers() => throw new NotImplementedException();
    public void Serialize(ISerializationWriter writer) => throw new NotImplementedException();
}
