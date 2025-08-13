using ReQuesty.Runtime.Abstractions.Serialization;

namespace ReQuesty.Runtime.Tests.Serialization.Json.Mocks;

public class ConverterTestEntity : IParsable
{
    public Guid? Id { get; set; }

    public IDictionary<string, Action<IParseNode>> GetFieldDeserializers() => new Dictionary<string, Action<IParseNode>>
    {
        { "id", n => Id = n.GetGuidValue() }
    };

    public void Serialize(ISerializationWriter writer)
    {
        _ = writer ?? throw new ArgumentNullException(nameof(writer));
        writer.WriteGuidValue("id", Id);
    }
}