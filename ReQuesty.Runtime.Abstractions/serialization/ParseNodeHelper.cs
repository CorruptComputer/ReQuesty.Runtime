namespace ReQuesty.Runtime.Abstractions.Serialization;

/// <summary>
///   Helper methods for intersection wrappers
/// </summary>
public static class ParseNodeHelper
{
    /// <summary>
    ///   Merges the given fields deserializers for an intersection type into a single collection.
    /// </summary>
    /// <param name="targets">The collection of deserializers to merge.</param>
    public static IDictionary<string, Action<IParseNode>> MergeDeserializersForIntersectionWrapper(params IParsable?[] targets)
    {
        ArgumentNullException.ThrowIfNull(targets);
        if (targets.Length == 0)
        {
            throw new ArgumentException("At least one target must be provided.", nameof(targets));
        }

        Dictionary<string, Action<IParseNode>> result = [];
        foreach (IParsable? target in targets)
        {
            if (target != null)
            {
                IDictionary<string, Action<IParseNode>> fieldDeserializers = target.GetFieldDeserializers();
                foreach (KeyValuePair<string, Action<IParseNode>> fieldDeserializer in fieldDeserializers)
                {
                    if (!result.ContainsKey(fieldDeserializer.Key))
                    {
                        result.Add(fieldDeserializer.Key, fieldDeserializer.Value);
                    }
                }
            }
        }

        return result;
    }
}