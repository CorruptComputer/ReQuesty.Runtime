using ReQuesty.Runtime.Abstractions;
using ReQuesty.Runtime.Abstractions.Serialization;

namespace ReQuesty.Runtime.Tests.Serialization.Json.Mocks;

public class DerivedTestEntity : TestEntity
{
    /// <summary>
    /// Date enrolled in primary school
    /// </summary>
    public Date? EnrolmentDate { get; set; }
    public override IDictionary<string, Action<IParseNode>> GetFieldDeserializers()
    {
        IDictionary<string, Action<IParseNode>> parentDeserializers = base.GetFieldDeserializers();
        parentDeserializers.Add("enrolmentDate", n => { EnrolmentDate = n.GetDateValue(); });
        return parentDeserializers;
    }
    public override void Serialize(ISerializationWriter writer)
    {
        base.Serialize(writer);
        writer.WriteDateValue("enrolmentDate", EnrolmentDate);
    }
}