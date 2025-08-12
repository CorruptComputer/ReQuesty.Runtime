namespace ReQuesty.Runtime.Abstractions.Serialization;

/// <summary>
///   Proxy factory that allows the composition of before and after callbacks on existing factories.
/// </summary>
/// <param name="factoryToWrap">The concrete factory to wrap.</param>
/// <param name="onBeforeSerialization">The callback to invoke before the serialization of any model object.</param>
/// <param name="onAfterSerialization">The callback to invoke after the serialization of any model object.</param>
/// <param name="onStartSerialization">The callback to invoke when serialization of the entire model has started.</param>
public class SerializationWriterProxyFactory(ISerializationWriterFactory factoryToWrap,
    Action<IParsable> onBeforeSerialization,
    Action<IParsable> onAfterSerialization,
    Action<IParsable, ISerializationWriter> onStartSerialization) : ISerializationWriterFactory
{
    /// <summary>
    ///   The valid content type for the <see cref="SerializationWriterProxyFactory"/>
    /// </summary>
    public string ValidContentType { get { return ProxiedSerializationWriterFactory.ValidContentType; } }

    /// <summary>
    ///   The factory that is being proxied.
    /// </summary>
    protected readonly ISerializationWriterFactory ProxiedSerializationWriterFactory = factoryToWrap ?? throw new ArgumentNullException(nameof(factoryToWrap));
    private readonly Action<IParsable> _onBefore = onBeforeSerialization;
    private readonly Action<IParsable> _onAfter = onAfterSerialization;
    private readonly Action<IParsable, ISerializationWriter> _onStartSerialization = onStartSerialization;

    /// <summary>
    ///   Creates a new <see cref="ISerializationWriter" /> instance for the given content type.
    /// </summary>
    /// <param name="contentType">The content type for which a serialization writer should be created.</param>
    /// <returns>A new <see cref="ISerializationWriter" /> instance for the given content type.</returns>
    public ISerializationWriter GetSerializationWriter(string contentType)
    {
        ISerializationWriter writer = ProxiedSerializationWriterFactory.GetSerializationWriter(contentType);
        Action<IParsable>? originalBefore = writer.OnBeforeObjectSerialization;
        Action<IParsable>? originalAfter = writer.OnAfterObjectSerialization;
        Action<IParsable, ISerializationWriter>? originalStart = writer.OnStartObjectSerialization;
        writer.OnBeforeObjectSerialization = (x) =>
        {
            _onBefore?.Invoke(x); // the callback set by the implementation (e.g. backing store)
            originalBefore?.Invoke(x); // some callback that might already be set on the target
        };
        writer.OnAfterObjectSerialization = (x) =>
        {
            _onAfter?.Invoke(x);
            originalAfter?.Invoke(x);
        };
        writer.OnStartObjectSerialization = (x, y) =>
        {
            _onStartSerialization?.Invoke(x, y);
            originalStart?.Invoke(x, y);
        };
        return writer;
    }
}
