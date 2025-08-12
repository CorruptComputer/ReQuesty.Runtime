using ReQuesty.Runtime.Abstractions.Serialization;

namespace ReQuesty.Runtime.Serialization.Text.Tests.Mocks
{
    public class TestEntity : IParsable
    {
        public IDictionary<string, Action<IParseNode>> GetFieldDeserializers() => throw new NotImplementedException();
        public void Serialize(ISerializationWriter writer) => throw new NotImplementedException();
    }
}
