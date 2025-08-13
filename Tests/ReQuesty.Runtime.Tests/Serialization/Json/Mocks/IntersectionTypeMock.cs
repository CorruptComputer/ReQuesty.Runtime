using ReQuesty.Runtime.Abstractions.Serialization;

namespace ReQuesty.Runtime.Tests.Serialization.Json.Mocks;

public class IntersectionTypeMock : IParsable, IComposedTypeWrapper
{
    public TestEntity? ComposedType1 { get; set; }
    public SecondTestEntity? ComposedType2 { get; set; }
    public string? StringValue { get; set; }
    public List<TestEntity>? ComposedType3 { get; set; }
    public static IntersectionTypeMock CreateFromDiscriminator(IParseNode parseNode)
    {
        IntersectionTypeMock result = new();
        if (parseNode.GetStringValue() is string stringValue)
        {
            result.StringValue = stringValue;
        }
        else if (parseNode.GetCollectionOfObjectValues<TestEntity>(TestEntity.CreateFromDiscriminator) is IEnumerable<TestEntity> values && values.Any())
        {
            result.ComposedType3 = values.ToList();
        }
        else
        {
            result.ComposedType1 = new();
            result.ComposedType2 = new();
        }
        return result;
    }
    public IDictionary<string, Action<IParseNode>> GetFieldDeserializers()
    {
        if (ComposedType1 != null || ComposedType1 != null)
        {
            return ParseNodeHelper.MergeDeserializersForIntersectionWrapper(ComposedType1, ComposedType2);
        }
        return new Dictionary<string, Action<IParseNode>>();
    }
    public void Serialize(ISerializationWriter writer)
    {
        _ = writer ?? throw new ArgumentNullException(nameof(writer));
        if (!string.IsNullOrEmpty(StringValue))
        {
            writer.WriteStringValue(null, StringValue);
        }
        else if (ComposedType3 != null)
        {
            writer.WriteCollectionOfObjectValues(null, ComposedType3);
        }
        else
        {
            writer.WriteObjectValue(null, ComposedType1, ComposedType2);
        }
    }
}