namespace ReQuesty.Runtime.Abstractions.Serialization;

/// <summary>
///   Defines a contract for models that can hold additional data besides the described properties.
/// </summary>
public interface IAdditionalDataHolder
{
    /// <summary>
    ///   Stores the additional data for this object that did not belong to the properties.
    /// </summary>
    IDictionary<string, object> AdditionalData { get; set; }
}
